using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class StorageService
{
    private readonly ApplicationDbContext _dbContext;

    public StorageService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StorageInfoDto?> GetStorageByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        email = email.Trim();

        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Email.ToLower() == email.ToLower())
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Username,
                u.Email,
                u.StorageUsedBytes,
                u.StorageLimitBytes
            })
            .SingleOrDefaultAsync();

        if (user is null)
        {
            return null;
        }

        var totalSpace = Math.Max(0, user.StorageLimitBytes);
        var usedSpace = Math.Max(0, user.StorageUsedBytes);

        var remainingSpace = Math.Max(0, totalSpace - usedSpace);

        var usagePercentage = totalSpace > 0
            ? Math.Round(
                (double)usedSpace / totalSpace * 100,
                2)
            : 0;

        return new StorageInfoDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,

            TotalSpaceBytes = totalSpace,
            UsedSpaceBytes = usedSpace,
            RemainingSpaceBytes = remainingSpace,

            UsagePercentage = usagePercentage,

            TotalSpaceMB = Math.Round(totalSpace / 1048576.0, 2),
            UsedSpaceMB = Math.Round(usedSpace / 1048576.0, 2),
            RemainingSpaceMB = Math.Round(remainingSpace / 1048576.0, 2)
        };
    }
}
