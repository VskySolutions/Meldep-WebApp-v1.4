using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record SOPProcessModel : BaseEntityModel
    {
    }

    public record SOPProcessSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
    }
    public record SOPProcessListModel : BasePagedListModel<SOPProcessModel>
    {

    }
}

