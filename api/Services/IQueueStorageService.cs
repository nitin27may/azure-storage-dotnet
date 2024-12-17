namespace AzureStorageApi.Services;

public interface IQueueStorageService
{
    Task SendMessageAsync(string queueName, string message);
    Task<string> ReceiveMessageAsync(string queueName);
    Task DeleteQueueAsync(string queueName);
}
