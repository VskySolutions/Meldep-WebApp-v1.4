using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Core;

namespace Vsky.Api.Models
{
    public record InfraFTPModel : BaseEntityModel
    {
    }

    public record InfraFTPSearchModel : BaseSearchModel
    {
        public List<string> InfraServiceIds { get; set; }
        public List<string> ProtocolTypeIds { get; set; }
        public List<string> EncryptionTypeIds { get; set; }
        public string Name { get; set; }
        public string SearchText { get; set; }
    }
}
