using Azure.Storage.Queues;

namespace AzureStorageApi.Services;

public class QueueStorageService : IQueueStorageService
{
    private readonly QueueServiceClient _queueServiceClient;

    public QueueStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _queueServiceClient = new QueueServiceClient(connectionString);
    }

    public async Task SendMessageAsync(string queueName, string message)
    {
        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(message);
    }

    public async Task<string> ReceiveMessageAsync(string queueName)
    {
        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        var message = await queueClient.ReceiveMessageAsync();
        if (message.Value != null)
        {
            await queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
            return message.Value.MessageText;
        }
        return null;
    }

    public async Task DeleteQueueAsync(string queueName)
    {
        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        await queueClient.DeleteIfExistsAsync();
    }
}
