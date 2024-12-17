using AzureBlobApi.Services;
using AzureStorageApi.Models;
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBlobAsync([FromForm] BlobUploadRequest request)
    {
        using var stream = request.File.OpenReadStream();
        await _blobStorageService.UploadBlobAsync(request.ContainerName, request.BlobName, stream);
        return Ok();
    }

    [HttpPost("upload-large")] // POST api/blob/upload-large
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10L * 1024 * 1024 * 1024)] // 10 GB
    public async Task<IActionResult> UploadLargeBlobAsync([FromForm] BlobUploadRequest request)
    {
        using var stream = request.File.OpenReadStream();
        await _blobStorageService.UploadLargeBlobAsync(request.ContainerName, request.BlobName, stream);
        return Ok();
    }

    [HttpPost("stream-upload")]
    public async Task<IActionResult> StreamUploadAsync()
    {
        try
        {
            var containerName = Request.Headers["Container-Name"].ToString();
            var blobName = Request.Headers["Blob-Name"].ToString();

            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
            {
                return BadRequest(new { Message = "Container-Name and Blob-Name headers are required." });
            }

            // Stream data from the client directly to Azure Blob Storage
            using var stream = Request.Body;

            // Infer the Content-Type

            await _blobStorageService.UploadBlobAsync(containerName, blobName, stream);

            return Ok(new { Message = "File uploaded successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error uploading file.", Error = ex.Message });
        }
    }

    [HttpPost("upload-chunk")]
    public async Task<IActionResult> UploadChunk(
        [FromForm] IFormFile chunk,
        [FromForm] string containerName,
        [FromForm] string blobName,
        [FromForm] int chunkIndex,
        [FromForm] int totalChunks)
    {
        if (chunk == null || chunk.Length == 0)
        {
            return BadRequest(new { Message = "Chunk is missing or empty." });
        }

        try
        {
            // Stream the chunk to the service
            using var stream = chunk.OpenReadStream();
            await _blobStorageService.UploadChunkAsync(containerName, blobName, stream, chunkIndex, totalChunks);

            return Ok(new { Message = $"Chunk {chunkIndex + 1}/{totalChunks} uploaded successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error uploading chunk.", Error = ex.Message });
        }
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
        var blobs = await _blobStorageService.GetAllBlobsAsync(containerName, path, true, DateTimeOffset.UtcNow.AddHours(1));
        return Ok(blobs);
    }
}