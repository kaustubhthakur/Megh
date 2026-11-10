
using System.Net.Http.Headers;
using System.Text.Json;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Services.CloudStorage;

public class GoogleOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _db;
    private readonly ITokenProtectionService _protection;
    private readonly GoogleOAuthOptions _options;

    private const string Provider = "GoogleDrive";

    public GoogleOAuthService(
        HttpClient httpClient,
        ApplicationDbContext db,
        ITokenProtectionService protection,
        IOptions<GoogleOAuthOptions> options)
    {
        _httpClient = httpClient;
        _db = db;
        _protection = protection;
        _options = options.Value;
    }

    public string GetAuthorizationUrl(string state)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile https://www.googleapis.com/auth/drive.file https://www.googleapis.com/auth/drive.metadata.readonly",
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["state"] = state
        };

        var query = string.Join("&",
            parameters.Select(p =>
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        return $"https://accounts.google.com/o/oauth2/v2/auth?{query}";
    }

    public async Task<ConnectedAccount> CompleteAuthorizationAsync(
        int userId,
        string code,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["redirect_uri"] = _options.RedirectUri,
                ["grant_type"] = "authorization_code"
            }),
            cancellationToken);

        var tokenJson = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Google token exchange failed: {tokenJson}");
        }

        var token = JsonSerializer.Deserialize<TokenResponse>(
            tokenJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException(
                "Google returned an invalid token response.");

        using var userRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "https://openidconnect.googleapis.com/v1/userinfo");

        userRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        using var userResponse = await _httpClient.SendAsync(
            userRequest, cancellationToken);

        userResponse.EnsureSuccessStatusCode();

        var userJson = await userResponse.Content.ReadAsStringAsync(
            cancellationToken);

        var googleUser = JsonSerializer.Deserialize<GoogleUser>(
            userJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException(
                "Unable to retrieve Google user information.");

        if (string.IsNullOrWhiteSpace(googleUser.Email) ||
            string.IsNullOrWhiteSpace(googleUser.Subject))
        {
            throw new InvalidOperationException(
                "Google did not return a valid email or user ID.");
        }

        var account = await _db.ConnectedAccounts.FirstOrDefaultAsync(
            a => a.UserId == userId &&
                 a.Provider == Provider &&
                 a.Email == googleUser.Email,
            cancellationToken);

        if (account == null)
        {
            account = new ConnectedAccount
            {
                UserId = userId,
                Provider = Provider,
                Email = googleUser.Email
            };

            _db.ConnectedAccounts.Add(account);
        }

        account.ProviderUserId = googleUser.Subject;
        account.EncryptedAccessToken =
            _protection.Protect(token.AccessToken);

        if (!string.IsNullOrWhiteSpace(token.RefreshToken))
        {
            account.EncryptedRefreshToken =
                _protection.Protect(token.RefreshToken);
        }

        account.TokenExpiresAt =
            DateTime.UtcNow.AddSeconds(token.ExpiresIn);

        account.IsConnected = true;

        await _db.SaveChangesAsync(cancellationToken);

        return account;
    }

    public async Task<string> GetValidAccessTokenAsync(
        int userId,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        var account = await _db.ConnectedAccounts.FirstOrDefaultAsync(
            a => a.Id == accountId &&
                 a.UserId == userId &&
                 a.Provider == Provider &&
                 a.IsConnected,
            cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException(
                "Connected Google account not found.");
        }

        if (account.TokenExpiresAt.HasValue &&
            account.TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(2))
        {
            return _protection.Unprotect(
                account.EncryptedAccessToken);
        }

        if (string.IsNullOrWhiteSpace(account.EncryptedRefreshToken))
        {
            throw new InvalidOperationException(
                "Google refresh token is unavailable. Reconnect your account.");
        }

        var refreshToken = _protection.Unprotect(
            account.EncryptedRefreshToken);

        using var response = await _httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            }),
            cancellationToken);

        var json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Google token refresh failed: {json}");
        }

        var token = JsonSerializer.Deserialize<TokenResponse>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException(
                "Invalid Google refresh response.");

        account.EncryptedAccessToken =
            _protection.Protect(token.AccessToken);

        account.TokenExpiresAt =
            DateTime.UtcNow.AddSeconds(token.ExpiresIn);

        await _db.SaveChangesAsync(cancellationToken);

        return token.AccessToken;
    }

    public async Task<object> SyncQuotaAsync(
        int userId,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetValidAccessTokenAsync(
            userId, accountId, cancellationToken);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://www.googleapis.com/drive/v3/about?fields=user,storageQuota");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(
            request, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Google quota request failed: {json}");
        }

        using var document = JsonDocument.Parse(json);

        var quota = document.RootElement.GetProperty("storageQuota");

        long? total = ReadQuotaValue(quota, "limit");

        long? used = ReadQuotaValue(quota, "usageInDrive")
                     ?? ReadQuotaValue(quota, "usage");

        var account = await _db.ConnectedAccounts.FirstAsync(
            a => a.Id == accountId && a.UserId == userId,
            cancellationToken);

        account.TotalBytes = total;
        account.UsedBytes = used;
        account.AvailableBytes =
            total.HasValue && used.HasValue
                ? Math.Max(0, total.Value - used.Value)
                : null;

        account.LastSyncedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new
        {
            account.Email,
            account.TotalBytes,
            account.UsedBytes,
            account.AvailableBytes,
            account.LastSyncedAt
        };
    }

    private static long? ReadQuotaValue(
        JsonElement quota,
        string property)
    {
        if (!quota.TryGetProperty(property, out var value) ||
            value.ValueKind != JsonValueKind.String ||
            !long.TryParse(value.GetString(), out var result))
        {
            return null;
        }

        return result;
    }

    private sealed class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
    }

    private sealed class GoogleUser
    {
        public string Subject { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
