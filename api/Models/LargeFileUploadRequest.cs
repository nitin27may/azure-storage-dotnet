namespace AzureStorageApi.Models;

public class LargeFileUploadRequest
{
    public string ContainerName { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
}