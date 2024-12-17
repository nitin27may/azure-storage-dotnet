using AzureStorageApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageApi.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueStorageController : ControllerBase
{
    private readonly IQueueStorageService _queueStorageService;

    public QueueStorageController(IQueueStorageService queueStorageService)
    {
        _queueStorageService = queueStorageService;
    }

    [HttpPost("send")] // POST api/queue/send
    public async Task<IActionResult> SendMessageAsync(string queueName, string message)
    {
        await _queueStorageService.SendMessageAsync(queueName, message);
        return Ok();
    }

    [HttpGet("receive")] // GET api/queue/receive
    public async Task<IActionResult> ReceiveMessageAsync(string queueName)
    {
        var message = await _queueStorageService.ReceiveMessageAsync(queueName);
        return Ok(message);
    }

    [HttpDelete("delete")] // DELETE api/queue/delete
    public async Task<IActionResult> DeleteQueueAsync(string queueName)
    {
        await _queueStorageService.DeleteQueueAsync(queueName);
        return NoContent();
    }
}
