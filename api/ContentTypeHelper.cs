using Microsoft.AspNetCore.StaticFiles;

namespace AzureStorageApi;

public static class ContentTypeHelper
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new FileExtensionContentTypeProvider();

    public static string GetContentType(string fileName)
    {
        if (ContentTypeProvider.TryGetContentType(fileName, out var contentType))
        {
            return contentType;
        }

        // Default to application/octet-stream if the content type cannot be determined
        return "application/octet-stream";
    }
}
