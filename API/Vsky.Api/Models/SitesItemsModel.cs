using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public class SitesItemsModel
    {
    }
    public record SitesItemsSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public List<string> ItemSubcategoryIds { get; set; }
        public string ItemName { get; set; }

    }
}
