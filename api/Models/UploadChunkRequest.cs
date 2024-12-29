namespace AzureStorageApi.Models;

public class UploadChunkRequest
{
    public IFormFile Chunk { get; set; }
    public string ContainerName { get; set; }
    public string BlobName { get; set; }
    public int ChunkIndex { get; set; }
    public int TotalChunks { get; set; }
}
