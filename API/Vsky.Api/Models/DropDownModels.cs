using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record DropDownModel : BaseEntityModel
    {
        public string DropDownTypeId { get; set; }
        public string DropDownValue { get; set; }
        public string DropDownText { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool Active { get; set; }
        public string BgColor { get; set; }
        public string Color { get; set; }

        public virtual DropDownTypeModel DropDownType { get; set; }

    }

    public record DropDownSearchModel : BaseSearchModel
    {
        public List<string> DropDownTypeIds { get; set; }
        public string SearchText { get; set; }
    }

    public record DropDownListModel : BasePagedListModel<DropDownModel>
    {
    }

    public record DropDownViewModel : BaseEntityModel
    {
        public string DropDownTypeId { get; set; }
        public string DropdownValue { get; set; }
        public string DropDownText { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public string BgColor { get; set; }
        public string Color { get; set; }
        public bool Active { get; set; }
    }
}