using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Core;

namespace Vsky.Api.Models
{
    public record InfraAccountModel : BaseEntityModel
    {
    }

    public record InfraAccountSearchModel : BaseSearchModel
    {
        public List<string> ProviderIds { get; set; }
        public List<string> InfraAccountIds { get; set; }
        public string SearchText { get; set; }
        public string CCLast4Digits { get; set; }
    }

    public record InfraAccountServicesSearchModel : BaseSearchModel
    {
        public List<string> ProjectIds { get; set; }
        public List<string> ItemTypeIds { get; set; }
        public List<string> InfraAccountIds { get; set; }
        public List<string> OwnerShipTypeIds { get; set; }
        public List<string> PaymentTermIds { get; set; }
        public string SearchText { get; set; }
    }
}
