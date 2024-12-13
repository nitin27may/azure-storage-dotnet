using Azure.Data.Tables;
using AzureStorageApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageApi.Controllers;

[ApiController]
[Route("api/table")]
public class TableStorageController : ControllerBase
{
    private readonly ITableStorageService _tableStorageService;

    public TableStorageController(ITableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService;
    }

    [HttpPost("add")] // POST api/table/add
    public async Task<IActionResult> AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
    {
        await _tableStorageService.AddEntityAsync(tableName, entity);
        return Ok();
    }

    [HttpGet("get")] // GET api/table/get
    public async Task<IActionResult> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
    {
        var entity = await _tableStorageService.GetEntityAsync<T>(tableName, partitionKey, rowKey);
        return Ok(entity);
    }

    [HttpDelete("delete")] // DELETE api/table/delete
    public async Task<IActionResult> DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteEntityAsync(tableName, partitionKey, rowKey);
        return NoContent();
    }
}
