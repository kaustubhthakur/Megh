
namespace backend.Services.CloudStorage;

public class CloudStorageObject
{
    public string Provider { get; set; } = string.Empty;

    public string ProviderFileId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
}
