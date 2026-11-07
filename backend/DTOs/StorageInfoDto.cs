
namespace backend.DTOs;

public class StorageInfoDto
{
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public long TotalSpaceBytes { get; set; }

    public long UsedSpaceBytes { get; set; }

    public long RemainingSpaceBytes { get; set; }

    public double UsagePercentage { get; set; }

    public double TotalSpaceMB { get; set; }

    public double UsedSpaceMB { get; set; }

    public double RemainingSpaceMB { get; set; }
}