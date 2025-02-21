namespace AzureStorageApi.Models;

public class BlobSasDetails
{
    public string SasUri { get; set; }
    public string BlobName { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }
}