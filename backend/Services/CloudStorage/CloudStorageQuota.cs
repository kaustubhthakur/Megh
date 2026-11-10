
namespace backend.Services.CloudStorage;

public class CloudStorageQuota
{
    public long TotalBytes { get; set; }

    public long UsedBytes { get; set; }

    public long AvailableBytes { get; set; }
}
