namespace AzureStorageApi.Models;

public class BlobDetails
{
    public string Name { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public IDictionary<string, string> Metadata { get; set; }
    public string SasUri { get; set; }

    public string ContentType { get; set; }
}
