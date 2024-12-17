namespace AzureStorageApi.Services
{
    public interface IFileStorageService
    {
        Task UploadFileAsync(string shareName, string directoryName, string fileName, Stream content);
        Task<Stream> DownloadFileAsync(string shareName, string directoryName, string fileName);
        Task DeleteFileAsync(string shareName, string directoryName, string fileName);
    }
}
