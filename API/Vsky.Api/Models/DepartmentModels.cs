using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record DepartmentModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Active { get; set; }
        public virtual Site Site { get; set; }

    }

    public record DepartmentSearchModel : BaseSearchModel
    {
        public List<string> DepartmentIds { get; set; }
        public string SearchText { get; set; }
    }

    public record DepartmentListModel : BasePagedListModel<DepartmentModel>
    {
        public bool editing { get; set; }
    }
    public record DepartmentUploadModel : BaseEntityModel
    {
    }
}