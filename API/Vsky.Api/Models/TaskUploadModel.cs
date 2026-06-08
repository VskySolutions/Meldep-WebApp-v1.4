using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record TaskUploadModel : BaseSearchModel
    {
        public string ProjectId { get; set; }

        public IFormFile FileXls { get; set; }
    }
}
