using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Microsoft.AspNetCore.Http;

namespace Vsky.Api.Models
{
    public record InventoryItemTypeModels : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public int SortOrder { get; set; }
        public virtual Site Sites { get; set; }
    }
}
