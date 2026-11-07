
namespace backend.Services;

public record ProviderStorageInfo(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes
);

public interface IStorageProvider
{
    string ProviderName { get; }

    Task<ProviderStorageInfo> GetStorageInfoAsync(
        int connectedAccountId,
        CancellationToken cancellationToken
    );

    Task<string> UploadChunkAsync(
        int connectedAccountId,
        Stream content,
        string fileName,
        CancellationToken cancellationToken
    );

    Task<Stream> DownloadChunkAsync(
        int connectedAccountId,
        string providerFileId,
        CancellationToken cancellationToken
    );

    Task DeleteChunkAsync(
        int connectedAccountId,
        string providerFileId,
        CancellationToken cancellationToken
    );
}
