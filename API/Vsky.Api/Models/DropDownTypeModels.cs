using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record DropDownTypeModel : BaseEntityModel
    {     
        public string Type { get; set; }
        public string SiteId { get; set; }
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public string ModuleName { get; set; }
        public int SortOrder { get; set; }
        public bool IsAlphabeticalOrNumerical { get; set; }
        public virtual ICollection<DropDownType> DropDownTypeList { get; set; } = new List<DropDownType>();
    }

    public record DropDownTypeSearchModel : BaseSearchModel
    {
        public string ModuleName { get; set; }
        public string GroupName { get; set; }
        public List<string> DropDownTypeIds { get; set; }
        public string SearchText { get; set; }
    }

    public record DropDownTypeViewModel : BaseEntityModel
    {
        public string GroupName { get; set; }
        public string ModuleName { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public int SortOrder { get; set; }
    }

    public record DropDownTypeListModel : BasePagedListModel<DropDownTypeModel>
    {
    }
}