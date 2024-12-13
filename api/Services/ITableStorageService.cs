using Azure.Data.Tables;
namespace AzureStorageApi.Services;


public interface ITableStorageService
{
    Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new();
    Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();
    Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey);
}
