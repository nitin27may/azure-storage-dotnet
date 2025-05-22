namespace AzureStorageApi.Models;

public class LargeFileUploadResponse
{
    public string SasUri { get; set; }
    public string BlobName { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }
    public string ContainerName { get; set; }
}
