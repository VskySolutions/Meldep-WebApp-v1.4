using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record InfraDatabaseModel : BaseEntityModel
    {
    }

    public record InfraDatabaseSearchModel : BaseSearchModel
    {
        public List<string> InfraServiceIds { get; set; }
        public string SearchText { get; set; }
    }
}
