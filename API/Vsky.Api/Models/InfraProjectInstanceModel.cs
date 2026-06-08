using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record InfraProjectInstanceModel : BaseEntityModel
    {
    }

    public record InfraProjectInstanceSearchModel : BaseSearchModel
    {
        public List<string> InfraProjectIds { get; set; }
        public List<string> PlatformIds { get; set; }
        public List<string> InstanceTypeIds { get; set; }
        public string SearchText { get; set; }
    }
}
