using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Lets containers be private without changing stored data or screens. In Signed mode, file URLs
    /// in API responses get a short-lived read signature. Signatures are removed from incoming data,
    /// so one is never saved. Rich text holding file URLs is handled too.
    /// </summary>
    public sealed class BlobUrlSigner
    {
        private const string LocalUploadsPrefix = "/uploads/";
        private static readonly Regex LocalUploadPattern = new(@"^/uploads/[^\s""'<>?]+(\?[^\s""'<>]*)?$", RegexOptions.Compiled);

        private readonly BlobServiceClient _client;
        private readonly string _host;
        private readonly Regex _urlPattern;
        private readonly TimeSpan _lifetime;
        private readonly ConcurrentDictionary<string, (string Query, DateTimeOffset RenewAfter)> _signatures = new();
        private ITimeLimitedDataProtector _localProtector;

        public BlobUrlSigner(string connectionString, int minutes)
        {
            _lifetime = TimeSpan.FromMinutes(minutes > 0 ? minutes : 60);
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            _client = new BlobServiceClient(connectionString);
            _host = _client.Uri.Host;
            _urlPattern = new Regex(
                $@"https://{Regex.Escape(_host)}/(?<container>[a-z0-9-]+)/(?<path>[^\s""'<>)\\?]*)(?<query>\?[^\s""'<>)\\]*)?",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public void UseDataProtection(IDataProtectionProvider provider)
        {
            _localProtector = provider.CreateProtector("Vsky.LocalUploads").ToTimeLimitedDataProtector();
        }

        public string Sign(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.StartsWith(LocalUploadsPrefix, StringComparison.OrdinalIgnoreCase) && LocalUploadPattern.IsMatch(value) && _localProtector != null)
            {
                var path = StripQuery(value);
                return $"{path}?sig={Uri.EscapeDataString(_localProtector.Protect(path, _lifetime))}";
            }

            if (_urlPattern == null || value.IndexOf(_host, StringComparison.OrdinalIgnoreCase) < 0)
                return value;

            return _urlPattern.Replace(value, match =>
            {
                var query = GetContainerSignature(match.Groups["container"].Value.ToLowerInvariant());
                return query == null
                    ? match.Value
                    : $"https://{_host}/{match.Groups["container"].Value}/{match.Groups["path"].Value}{query}";
            });
        }

        public string Strip(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.StartsWith(LocalUploadsPrefix, StringComparison.OrdinalIgnoreCase) && LocalUploadPattern.IsMatch(value))
                return StripQuery(value);

            if (_urlPattern == null || value.IndexOf(_host, StringComparison.OrdinalIgnoreCase) < 0)
                return value;

            return _urlPattern.Replace(value, match => $"https://{_host}/{match.Groups["container"].Value}/{match.Groups["path"].Value}");
        }

        public bool IsValidLocalSignature(string path, string signature)
        {
            if (_localProtector == null || string.IsNullOrEmpty(signature))
                return false;

            try
            {
                return string.Equals(_localProtector.Unprotect(signature), path, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string StripQuery(string value)
        {
            var index = value.IndexOf('?');
            return index < 0 ? value : value.Substring(0, index);
        }

        // Signed per container and reused until half its life has passed, so URLs stay stable.
        private string GetContainerSignature(string containerName)
        {
            var now = DateTimeOffset.UtcNow;
            if (_signatures.TryGetValue(containerName, out var cached) && cached.RenewAfter > now)
                return cached.Query;

            var container = _client.GetBlobContainerClient(containerName);
            if (!container.CanGenerateSasUri)
                return null;

            var query = container.GenerateSasUri(BlobContainerSasPermissions.Read, now.Add(_lifetime)).Query;
            _signatures[containerName] = (query, now.AddTicks(_lifetime.Ticks / 2));
            return query;
        }

        public static bool IsSignedMode(IConfiguration configuration)
        {
            return string.Equals(configuration["BlobAccess:Mode"], "Signed", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Applied to every string in JSON requests and responses, only in Signed mode.</summary>
    public sealed class BlobUrlJsonConverter : JsonConverter<string>
    {
        private readonly BlobUrlSigner _signer;

        public BlobUrlJsonConverter(BlobUrlSigner signer)
        {
            _signer = signer;
        }

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return _signer.Strip(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(_signer.Sign(value));
        }
    }

    public static class LocalUploadsProtection
    {
        /// <summary>In Signed mode, files under /uploads need a valid signature, as they're public otherwise.</summary>
        public static void UseSignedLocalUploads(this IApplicationBuilder app, BlobUrlSigner signer)
        {
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.ToString();
                if (path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
                    && !signer.IsValidLocalSignature(path, context.Request.Query["sig"].ToString()))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                await next();
            });
        }
    }
}
