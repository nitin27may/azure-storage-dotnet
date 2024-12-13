using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System.Text;

namespace AzureBlobApi.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task UploadBlobAsync(string containerName, string blobName, Stream content)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, overwrite: true);
    }

    public async Task UploadLargeBlobAsync(string containerName, string blobName, Stream content, int blockSize = 4 * 1024 * 1024)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blockBlobClient = containerClient.GetBlockBlobClient(blobName);
        var blockList = new List<string>();
        int blockNumber = 0;
        byte[] buffer = new byte[blockSize];
        int bytesRead;

        while ((bytesRead = await content.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(blockNumber.ToString("d6")));
            using var blockData = new MemoryStream(buffer, 0, bytesRead);
            await blockBlobClient.StageBlockAsync(blockId, blockData);
            blockList.Add(blockId);
            blockNumber++;
        }

        await blockBlobClient.CommitBlockListAsync(blockList);
    }

    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var downloadInfo = await blobClient.DownloadAsync();
        return downloadInfo.Value.Content;
    }

    public async Task<byte[]> GetBlobAsByteArrayAsync(string containerName, string blobName)
    {
        using var stream = await DownloadBlobAsync(containerName, blobName);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task<string> GetBlobSasUriAsync(string containerName, string blobName, DateTimeOffset expiryTime)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = expiryTime
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task DeleteBlobAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<IEnumerable<string>> GetAllBlobsAsync(string containerName, string path = null, bool includeSasUri = false, DateTimeOffset? sasExpiryTime = null)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobs = containerClient.GetBlobsAsync(prefix: path);
        var blobNames = new List<string>();

        await foreach (var blob in blobs)
        {
            if (includeSasUri && sasExpiryTime.HasValue)
            {
                blobNames.Add(GetBlobSasUriAsync(containerName, blob.Name, sasExpiryTime.Value).ToString());
            }
            else
            {
                blobNames.Add(blob.Name);
            }
        }

        return blobNames;
    }

    public async Task UploadFileToPathAsync(string containerName, string path, Stream content)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(path);
        await blobClient.UploadAsync(content, overwrite: true);
    }

    public async Task<bool> VerifyBlobChecksumAsync(string containerName, string blobName, string expectedChecksum)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var properties = await blobClient.GetPropertiesAsync();

        return properties.Value.ContentHash != null &&
               Convert.ToBase64String(properties.Value.ContentHash) == expectedChecksum;
    }
}