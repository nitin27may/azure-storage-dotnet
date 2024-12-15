using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using AzureStorageApi.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace AzureBlobApi.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task UploadBlobAsync(string containerName, string blobName, Stream content)
    {
        if (!IsValidContainerName(containerName))
        {
            throw new ArgumentException("Invalid container name.");
        }
        string blobNameWithTimestamp = GenerateBlobNameWithTimestamp(blobName);

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(blobNameWithTimestamp);
        await blobClient.UploadAsync(content, overwrite: true);
    }

    public async Task UploadLargeBlobAsync(string containerName, string blobName, Stream content, int blockSize = 4 * 1024 * 1024)
    {
        if (!IsValidContainerName(containerName))
        {
            throw new ArgumentException("Invalid container name.");
        }
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

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

    public async Task UploadChunkAsync(string containerName, string blobName, Stream chunkData, int chunkIndex, int totalChunks)
    {
        if (!IsValidContainerName(containerName))
        {
            throw new ArgumentException("Invalid container name.");
        }

        // Get the container client
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        // Get the block blob client
        var blockBlobClient = containerClient.GetBlockBlobClient(blobName);

        // Generate a unique block ID for each chunk
        var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(chunkIndex.ToString("d6")));

        // Stage the chunk as a block
        await blockBlobClient.StageBlockAsync(blockId, chunkData);

        // If this is the last chunk, commit the block list
        if (chunkIndex + 1 == totalChunks)
        {
            // Create a list of block IDs
            var blockList = Enumerable.Range(0, totalChunks)
                .Select(index => Convert.ToBase64String(Encoding.UTF8.GetBytes(index.ToString("d6"))))
                .ToList();

            // Commit the block list to assemble the final blob
            await blockBlobClient.CommitBlockListAsync(blockList);
        }
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

    public async Task<List<BlobDetails>> GetAllBlobsAsync(string containerName, string path = null, bool includeSasUri = false, DateTimeOffset? sasExpiryTime = null)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobs = containerClient.GetBlobsAsync(prefix: path);
        var blobDetailsList = new List<BlobDetails>();


        await foreach (var blobItem in blobs)
        {
            var blobDetails = new BlobDetails
            {
                Name = blobItem.Name,
                CreatedOn = blobItem.Properties.CreatedOn,
                Metadata = blobItem.Metadata,
            };

            if (includeSasUri && sasExpiryTime.HasValue)
            {
                blobDetails.SasUri = await GetBlobSasUriAsync(containerName, blobItem.Name, sasExpiryTime.Value);
            }

            blobDetailsList.Add(blobDetails);
        }

        return blobDetailsList;
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

    private bool IsValidContainerName(string containerName)
    {
        // Implement validation logic for container name
        return Regex.IsMatch(containerName, @"^[a-z0-9-]+$");
    }
    private string GenerateBlobNameWithTimestamp(string blobName)
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"); // Format: YYYYMMDDHHMMSS
        string extension = Path.GetExtension(blobName); // Get the file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(blobName); // Get the file name without extension
        return $"{fileNameWithoutExtension}_{timestamp}{extension}"; // Append timestamp
    }
}