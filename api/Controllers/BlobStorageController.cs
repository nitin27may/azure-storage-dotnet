using AzureBlobApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobApi.Controllers;

[ApiController]
[Route("api/blob")]
public class BlobStorageController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;

    public BlobStorageController(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    [HttpPost("upload")] // POST api/blob/upload
    public async Task<IActionResult> UploadBlobAsync(string containerName, string blobName, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        await _blobStorageService.UploadBlobAsync(containerName, blobName, stream);
        return Ok();
    }

    [HttpPost("upload-large")] // POST api/blob/upload-large
    public async Task<IActionResult> UploadLargeBlobAsync(string containerName, string blobName, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        await _blobStorageService.UploadLargeBlobAsync(containerName, blobName, stream);
        return Ok();
    }

    [HttpGet("download")] // GET api/blob/download
    public async Task<IActionResult> DownloadBlobAsync(string containerName, string blobName)
    {
        var stream = await _blobStorageService.DownloadBlobAsync(containerName, blobName);
        return File(stream, "application/octet-stream", blobName);
    }

    [HttpGet("download-bytes")] // GET api/blob/download-bytes
    public async Task<IActionResult> DownloadBlobAsByteArrayAsync(string containerName, string blobName)
    {
        var byteArray = await _blobStorageService.GetBlobAsByteArrayAsync(containerName, blobName);
        return File(byteArray, "application/octet-stream", blobName);
    }

    [HttpGet("sas")] // GET api/blob/sas
    public async Task<IActionResult> GetBlobSasUriAsync(string containerName, string blobName, DateTimeOffset expiryTime)
    {
        var sasUri = await _blobStorageService.GetBlobSasUriAsync(containerName, blobName, expiryTime);
        return Ok(sasUri);
    }

    [HttpDelete("delete")] // DELETE api/blob/delete
    public async Task<IActionResult> DeleteBlobAsync(string containerName, string blobName)
    {
        await _blobStorageService.DeleteBlobAsync(containerName, blobName);
        return NoContent();
    }

    [HttpGet("list")] // GET api/blob/list
    public async Task<IActionResult> GetAllBlobsAsync(string containerName, string path = null)
    {
        var blobs = await _blobStorageService.GetAllBlobsAsync(containerName, path);
        return Ok(blobs);
    }
}