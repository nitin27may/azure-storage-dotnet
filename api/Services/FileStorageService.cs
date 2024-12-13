using Azure.Storage.Files.Shares;

namespace AzureStorageApi.Services;

public class FileStorageService : IFileStorageService
{
    private readonly ShareServiceClient _shareServiceClient;

    public FileStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _shareServiceClient = new ShareServiceClient(connectionString);
    }

    public async Task UploadFileAsync(string shareName, string directoryName, string fileName, Stream content)
    {
        var shareClient = _shareServiceClient.GetShareClient(shareName);
        await shareClient.CreateIfNotExistsAsync();

        var directoryClient = shareClient.GetDirectoryClient(directoryName);
        await directoryClient.CreateIfNotExistsAsync();

        var fileClient = directoryClient.GetFileClient(fileName);
        await fileClient.CreateAsync(content.Length);
        await fileClient.UploadAsync(content);
    }

    public async Task<Stream> DownloadFileAsync(string shareName, string directoryName, string fileName)
    {
        var shareClient = _shareServiceClient.GetShareClient(shareName);
        var directoryClient = shareClient.GetDirectoryClient(directoryName);
        var fileClient = directoryClient.GetFileClient(fileName);

        var downloadInfo = await fileClient.DownloadAsync();
        return downloadInfo.Value.Content;
    }

    public async Task DeleteFileAsync(string shareName, string directoryName, string fileName)
    {
        var shareClient = _shareServiceClient.GetShareClient(shareName);
        var directoryClient = shareClient.GetDirectoryClient(directoryName);
        var fileClient = directoryClient.GetFileClient(fileName);
        await fileClient.DeleteIfExistsAsync();
    }
}
