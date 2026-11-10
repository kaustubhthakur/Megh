
using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ConnectedAccountService : IConnectedAccountService
{
    private readonly ApplicationDbContext _context;

    public ConnectedAccountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ConnectedAccountResponseDto>> GetAccountsAsync(
        int userId)
    {
        return await _context.ConnectedAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.IsConnected)
            .Select(a => new ConnectedAccountResponseDto
            {
                Id = a.Id,
                Email = a.Email,
                Provider = a.Provider,
                TotalBytes = a.TotalBytes,
                UsedBytes = a.UsedBytes,
                AvailableBytes = a.AvailableBytes,
                IsConnected = a.IsConnected,
                LastSyncedAt = a.LastSyncedAt,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ConnectedAccountResponseDto?> GetAccountByIdAsync(
        int userId, int accountId)
    {
        return await _context.ConnectedAccounts
            .AsNoTracking()
            .Where(a =>
                a.Id == accountId &&
                a.UserId == userId &&
                a.IsConnected)
            .Select(a => new ConnectedAccountResponseDto
            {
                Id = a.Id,
                Email = a.Email,
                Provider = a.Provider,
                TotalBytes = a.TotalBytes,
                UsedBytes = a.UsedBytes,
                AvailableBytes = a.AvailableBytes,
                IsConnected = a.IsConnected,
                LastSyncedAt = a.LastSyncedAt,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DisconnectAccountAsync(
        int userId, int accountId)
    {
        var account = await _context.ConnectedAccounts
            .FirstOrDefaultAsync(a =>
                a.Id == accountId &&
                a.UserId == userId &&
                a.IsConnected);

        if (account == null)
            return false;

        account.IsConnected = false;

        account.EncryptedAccessToken = string.Empty;
        account.EncryptedRefreshToken = null;
        account.TokenExpiresAt = null;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<StorageSummaryDto> GetStorageSummaryAsync(
        int userId)
    {
        var accounts = await _context.ConnectedAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.IsConnected)
            .ToListAsync();

        long totalBytes = accounts.Sum(a => a.TotalBytes ?? 0);
        long usedBytes = accounts.Sum(a => a.UsedBytes ?? 0);

        long availableBytes = accounts.Sum(a =>
            a.AvailableBytes ??
            (a.TotalBytes.HasValue && a.UsedBytes.HasValue
                ? Math.Max(0, a.TotalBytes.Value - a.UsedBytes.Value)
                : 0));

        double usedPercentage = totalBytes > 0
            ? (double)usedBytes / totalBytes * 100
            : 0;

        double availablePercentage = totalBytes > 0
            ? (double)availableBytes / totalBytes * 100
            : 0;

        return new StorageSummaryDto
        {
            TotalBytes = totalBytes,
            UsedBytes = usedBytes,
            AvailableBytes = availableBytes,
            ConnectedAccounts = accounts.Count,
            UsedPercentage = Math.Round(usedPercentage, 2),
            AvailablePercentage = Math.Round(availablePercentage, 2)
        };
    }
}
