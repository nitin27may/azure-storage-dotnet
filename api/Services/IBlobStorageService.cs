namespace AzureBlobApi.Services;

public interface IBlobStorageService
{
    Task UploadBlobAsync(string containerName, string blobName, Stream content);
    Task UploadLargeBlobAsync(string containerName, string blobName, Stream content, int blockSize = 4 * 1024 * 1024);
    Task<Stream> DownloadBlobAsync(string containerName, string blobName);
    Task<byte[]> GetBlobAsByteArrayAsync(string containerName, string blobName);
    Task<string> GetBlobSasUriAsync(string containerName, string blobName, DateTimeOffset expiryTime);
    Task DeleteBlobAsync(string containerName, string blobName);
    Task<IEnumerable<string>> GetAllBlobsAsync(string containerName, string path = null, bool includeSasUri = false, DateTimeOffset? sasExpiryTime = null);
    Task UploadFileToPathAsync(string containerName, string path, Stream content);
    Task<bool> VerifyBlobChecksumAsync(string containerName, string blobName, string expectedChecksum);
}
