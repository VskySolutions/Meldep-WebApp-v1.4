using System.Collections.Generic;
using System;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public class ItemCategoryModel
    {
    }
    public record ItemCategorySearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public List<string> ItemCategoryIds { get; set; }
        public List<string> ItemSubcategoryIds { get; set; }
        public string Prefix { get; set; }

    }
}
