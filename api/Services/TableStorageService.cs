using Azure.Data.Tables;

namespace AzureStorageApi.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableServiceClient _tableServiceClient;

    public TableStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _tableServiceClient = new TableServiceClient(connectionString);
    }

    public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
    {
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();
        await tableClient.AddEntityAsync(entity);
    }

    public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
    {
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        return await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
    }

    public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
    {
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }
}
