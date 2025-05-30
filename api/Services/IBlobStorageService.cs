﻿using AzureStorageApi.Models;

namespace AzureBlobApi.Services;

public interface IBlobStorageService
{
    Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType);
    Task UploadLargeBlobAsync(string containerName, string blobName, Stream content, string contentType, int blockSize = 4 * 1024 * 1024);
    Task UploadChunkAsync(string containerName, string blobName, Stream chunkData, int chunkIndex, int totalChunks, string contentType);
    Task<Stream> DownloadBlobAsync(string containerName, string blobName);
    Task<byte[]> GetBlobAsByteArrayAsync(string containerName, string blobName);
    string GetBlobSasUri(string containerName, string blobName, DateTimeOffset expiryTime);
    Task DeleteBlobAsync(string containerName, string blobName);
    Task<List<BlobDetails>> GetAllBlobsAsync(string containerName, string path = null, bool includeSasUri = false, DateTimeOffset? sasExpiryTime = null);
    Task UploadFileToPathAsync(string containerName, string path, Stream content);
    Task<bool> VerifyBlobChecksumAsync(string containerName, string blobName, string expectedChecksum);
    Task<BlobSasDetails> GetUploadSasUrlAsync(string containerName, string blobName, string contentType);
}
