
namespace backend.DTOs;

public class StorageSummaryDto
{
    public long TotalBytes { get; set; }

    public long UsedBytes { get; set; }

    public long AvailableBytes { get; set; }

    public int ConnectedAccounts { get; set; }

    public double UsedPercentage { get; set; }

    public double AvailablePercentage { get; set; }
}
