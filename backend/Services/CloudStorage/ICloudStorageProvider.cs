
namespace backend.Services.CloudStorage;

public interface ICloudStorageProvider
{
    string ProviderName { get; }

    Task<string> UploadAsync(
        string accessToken,
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string accessToken,
        string fileId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string accessToken,
        string fileId,
        CancellationToken cancellationToken = default);

    Task<long> GetAvailableStorageAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
