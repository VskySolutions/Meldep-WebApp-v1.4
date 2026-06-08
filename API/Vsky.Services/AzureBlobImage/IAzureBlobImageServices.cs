using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace Vsky.Services.AzureBlobImage
{
    public interface IAzureBlobImageServices
    {
        Task<BlobContainerClient> GetContainerByNameAsync(string containerName);

        Task<BlobContainerClient> CreateContainerIfNotExistsAsync(string containerName);

        Task<string> GetContainerDirectoryByNameAsync(string containerName, string directoryName);

        Task<string> CreateContainerDirectoryIfNotExistsAsync(string containerName, string directoryName);

        Task<string> UploadImageAsync(string containerName, string directoryName, IFormFile file);

        Task<List<string>> UploadFilesAsync(string containerName, string directoryName, List<IFormFile> files, string prefixValue, int existingFileCount = 0);

        Task<bool> DeleteImage(string imageUrl);

        Task DeleteMultipleImages(List<string> imageUrls);

        Task<string> ProcessHtmlAndUploadImagesAsync(
            string html,
            string siteName,
            string moduleName,
            string prefixName
        );

        Task<string> ProcessHtmlAndManageImagesAsync(
            string newHtml,
            string siteName,
            string directoryName,
            string prefixName,
            string oldHtml = null
        );
    }
}
