using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Vsky.Services.AzureBlobImage
{
    public class AzureBlobImageServices : IAzureBlobImageServices
    {
        private readonly BlobServiceClient _serviceClient;
        private readonly Vsky.Services.Logging.ILogger _loggerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AzureBlobImageServices(IConfiguration config, Logging.ILogger loggerService, IHttpContextAccessor httpContextAccessor)
        {
            _serviceClient = new BlobServiceClient(config.GetConnectionString("AzureBlob"));
            _loggerService = loggerService;
            _httpContextAccessor = httpContextAccessor;
        }

        // ================= Containers =================

        public async Task<BlobContainerClient> GetContainerByNameAsync(string containerName)
        {
            return _serviceClient.GetBlobContainerClient(containerName);
        }

        public async Task<BlobContainerClient> CreateContainerIfNotExistsAsync(string containerName)
        {
            var container = _serviceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(publicAccessType: PublicAccessType.Blob);
            return container;
        }

        // ================= Directories (Virtual) =================

        public async Task<string> GetContainerDirectoryByNameAsync(string containerName, string directoryName)
        {
            return $"{containerName}/{directoryName}";
        }

        public async Task<string> CreateContainerDirectoryIfNotExistsAsync(string containerName, string directoryName)
        {
            // Azure does NOT support real directories.
            // They exist implicitly via blob paths.

            var container = await CreateContainerIfNotExistsAsync(containerName);

            var dummyBlob = container.GetBlobClient($"{directoryName}/.keep");

            if (!await dummyBlob.ExistsAsync())
            {
                await dummyBlob.UploadAsync(BinaryData.FromString("init"));
            }

            return directoryName;
        }

        // ================= Upload Image =================
        public async Task<string> UploadImageAsync(string containerName, string directoryName, IFormFile file)
        {
            var container = await CreateContainerIfNotExistsAsync(containerName);

            string blobName = $"{directoryName}/{file.FileName}";

            var blobClient = container.GetBlobClient(blobName);
            await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true);

            return blobClient.Uri.ToString(); // PUBLIC URL
        }
        // ================= Multi Upload Image or Files =================
        public async Task<List<string>> UploadFilesAsync(string containerName, string directoryName, List<IFormFile> files, string prefixValue, int existingFileCount = 0)
        {
            var uploadedUrls = new List<string>();

            if (files == null || !files.Any())
                return uploadedUrls;

            var siteName = GenerateContainerName(containerName);

            //var WEB_Domain = "https://www.meldep.com";
            //directoryName = WEB_Domain == "https://www.meldep.com"
            //    ? directoryName
            //    : "sample";

            var host = _httpContextAccessor.HttpContext?.Request?.Host.Host;
            directoryName = host == "api.meldep.com" ? directoryName : "sample";

            int index = existingFileCount + 1;

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var newFileName = $"{prefixValue}-{index}-fl{extension}";

                    var renamedFile = new FormFile(file.OpenReadStream(), 0, file.Length, file.Name, newFileName)
                    {
                        Headers = file.Headers,
                        ContentType = file.ContentType
                    };

                    var url = await UploadImageAsync(siteName, directoryName, renamedFile);
                    uploadedUrls.Add(url);

                    index++;
                }
            }
            return uploadedUrls;
        }

        // ================= Delete Image =================
        public async Task<bool> DeleteImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;


            // Example:
            // https://account.blob.core.windows.net/container/folder/file.jpg
            // AbsolutePath = /container/folder/file.jpg

            // Check if URL is Azure Blob Storage URL
            // Safe URI parsing (No exception)
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri blobUri))
                return false;

            // Azure Blob domain check
            if (!blobUri.Host.Contains("blob.core.windows.net"))
                return false;

            // Parse blob URI
            //var blobUri = new Uri(imageUrl);

            var segments = blobUri.AbsolutePath.TrimStart('/').Split('/', 2);

            string containerName = segments[0];
            string blobName = segments[1];

            var container = await CreateContainerIfNotExistsAsync(containerName);
            var blobClient = container.GetBlobClient(blobName);

            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }

        // ================= Delete Multi Image =================
        public async Task DeleteMultipleImages(List<string> imageUrls)
        {
            if (imageUrls == null || !imageUrls.Any())
                return;

            foreach (var imageUrl in imageUrls)
            {
                try
                {
                    await DeleteImage(imageUrl);
                }
                catch (Exception ex)
                {
                    // log error
                    _loggerService.Error($"Failed to delete: {imageUrl}", ex, null);
                }
            }
        }

        // ================= Editor Image =================
        public async Task<string> ProcessHtmlAndManageImagesAsync(
            string newHtml,
            string siteName,
            string directoryName,
            string prefixName,
            string oldHtml = null
        )
        {
            if (string.IsNullOrEmpty(newHtml))
                return newHtml;

            var containerName = GenerateContainerName(siteName);

            var host = _httpContextAccessor.HttpContext?.Request?.Host.Host;
            directoryName = host == "api.meldep.com" ? directoryName : "sample";

            //  Upload new base64 images
            var updatedHtml = await ProcessHtmlAndUploadImagesAsync(
                newHtml,
                containerName,
                directoryName,
                prefixName
            );

            // Extract old & new image URLs
            var oldImages = ExtractImageUrls(oldHtml ?? "");
            var newImages = ExtractImageUrls(updatedHtml);

            // Find removed images
            var removedImages = oldImages.Except(newImages).ToList();

            // Delete removed images from blob
            await DeleteMultipleImages(removedImages);

            return updatedHtml;
        }

        public async Task<string> ProcessHtmlAndUploadImagesAsync(
            string html,
            string containerName,
            string directoryName,
            string prefixName
        )
        {
            if (string.IsNullOrEmpty(html))
                return html;

            var base64Images = ExtractBase64Images(html);
            //var containerName = GenerateContainerName(siteName);

            int index = 1;

            foreach (var base64Image in base64Images)
            {
                var file = ConvertBase64ToFormFile(base64Image);

                var extension = GetExtensionFromMime(file.ContentType);

                //var newFileName = $"{prefixName}-{index}{extension}";
                var newFileName = $"{prefixName}-{Guid.NewGuid().ToString()}{extension}";

                // Rename file here
                var renamedFile = new FormFile(file.OpenReadStream(), 0, file.Length, file.Name, newFileName)
                {
                    Headers = file.Headers,
                    ContentType = file.ContentType
                };

                var url = await UploadImageAsync(
                                containerName,
                                directoryName,
                                renamedFile);

                html = html.Replace(base64Image, url, StringComparison.Ordinal);
                index++;
            }
            return html;
        }

        #region Private Functions

        private string GenerateContainerName(string siteName)
        {
            if (string.IsNullOrWhiteSpace(siteName))
                return string.Empty;

            var containerName = Regex.Replace(siteName.ToLower(),
                               @"[^a-z0-9\s-]", "") // remove special chars
                                .Trim();
            //.Replace(" ", "-");

            containerName = Regex.Replace(containerName, @"\s+", "-"); // replace spaces with dash

            return $"site-{containerName}";
        }

        //Extract Base64
        private List<string> ExtractBase64Images(string html)
        {
            var list = new List<string>();

            var regex = new Regex("<img[^>]+src=[\"'](data:image[^\"']+)[\"'][^>]*>",
                                  RegexOptions.IgnoreCase);

            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                list.Add(match.Groups[1].Value);
            }

            return list;
        }

        //Extract Normal Image URLs
        private List<string> ExtractImageUrls(string html)
        {
            var list = new List<string>();

            var regex = new Regex("<img[^>]+src=[\"']([^\"']+)[\"'][^>]*>",
                                  RegexOptions.IgnoreCase);

            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                var url = match.Groups[1].Value;

                if (!url.StartsWith("data:image"))
                    list.Add(url);
            }

            return list;
        }

        //Convert Base64 -> IFormFile
        private IFormFile ConvertBase64ToFormFile(string base64String)
        {
            var parts = base64String.Split(',');
            var header = parts[0];
            var base64 = parts[1];

            byte[] bytes = Convert.FromBase64String(base64);

            var ms = new MemoryStream(bytes);

            var contentType = GetMimeType(header);

            return new FormFile(ms, 0, ms.Length, "file", Guid.NewGuid().ToString())
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        //Mime Type
        private string GetMimeType(string header)
        {
            if (header.Contains("image/png")) return "image/png";
            if (header.Contains("image/jpeg")) return "image/jpeg";
            if (header.Contains("image/jpg")) return "image/jpg";
            if (header.Contains("image/gif")) return "image/gif";
            if (header.Contains("image/webp")) return "image/webp";

            return "image/png";
        }
        private string GetExtensionFromMime(string mimeType)
        {
            return mimeType switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".png"
            };
        }
        #endregion
    }
}
