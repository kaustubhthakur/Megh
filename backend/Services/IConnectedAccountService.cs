
using backend.DTOs;

namespace backend.Services;

public interface IConnectedAccountService
{
    Task<List<ConnectedAccountResponseDto>> GetAccountsAsync(int userId);

    Task<ConnectedAccountResponseDto?> GetAccountByIdAsync(
        int userId, int accountId);

    Task<bool> DisconnectAccountAsync(int userId, int accountId);

    Task<StorageSummaryDto> GetStorageSummaryAsync(int userId);
}
