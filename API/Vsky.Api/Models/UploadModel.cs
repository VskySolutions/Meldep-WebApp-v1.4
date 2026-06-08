using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Vsky.Api.Models
{
    public class UploadModel
    {
        public string TaskId { get; set; }

        public List<IFormFile> Files { get; set; }
    }
}
