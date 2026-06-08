using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record SalesPersonModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public virtual EmployeeModel Employee { get; set; }

        public record SalesPersonSearchModel : BaseSearchModel
        {
            public string Name { get; set; }
        }

        public record SalesPersonListModel : BasePagedListModel<SalesPersonModel>
        {
        }
    }
}
