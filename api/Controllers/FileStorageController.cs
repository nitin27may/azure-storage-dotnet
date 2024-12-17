using AzureStorageApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageApi.Controllers;

[ApiController]
[Route("api/file")]
public class FileStorageController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public FileStorageController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("upload")] // POST api/file/upload
    public async Task<IActionResult> UploadFileAsync(string shareName, string directoryName, string fileName, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        await _fileStorageService.UploadFileAsync(shareName, directoryName, fileName, stream);
        return Ok();
    }

    [HttpGet("download")] // GET api/file/download
    public async Task<IActionResult> DownloadFileAsync(string shareName, string directoryName, string fileName)
    {
        var stream = await _fileStorageService.DownloadFileAsync(shareName, directoryName, fileName);
        return File(stream, "application/octet-stream", fileName);
    }

    [HttpDelete("delete")] // DELETE api/file/delete
    public async Task<IActionResult> DeleteFileAsync(string shareName, string directoryName, string fileName)
    {
        await _fileStorageService.DeleteFileAsync(shareName, directoryName, fileName);
        return NoContent();
    }
}