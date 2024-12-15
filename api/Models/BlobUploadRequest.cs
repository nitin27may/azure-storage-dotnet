namespace AzureStorageApi.Models;

public class BlobUploadRequest
{
    public string ContainerName { get; set; }
    public string BlobName { get; set; }
    public IFormFile File { get; set; }
}
