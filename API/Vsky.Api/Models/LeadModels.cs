using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record LeadModels : BaseEntityModel
    {
        public string SiteId { get; set; }

        public string PersonId { get; set; }
        public string ClientId { get; set; }
        public string CompanyId { get; set; }
        public string LeadGroupId { get; set; }
        public string LeadSourceId { get; set; }
        public string SalesPersonId { get; set; }
        public DateTime? LeadArrivalDate { get; set; }
        public string LeadReference { get; set; }
        public string LeadNote { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public int LeadNotesCount { get; set; }

        public virtual PersonModel Person { get; set; }
        public virtual CompanyClientsModel Client { get; set; }
        public virtual DropDownModel LeadSources { get; set; }
        public virtual DropDown LeadGroup { get; set; }
        public virtual CompanyModel Company { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual ICollection<LeadActivityLogs> LeadActivityLogs { get; set; } = new List<LeadActivityLogs>();
    }
    public record LeadSearchModels : BaseSearchModel
    {
        public string PersonId { get; set; }
        public string ClientId { get; set; }
        public string CompanyId { get; set; }
        public List<string> LeadGroupIds { get; set; }
        public string LeadSourceId { get; set; }
        public string SearchText { get; set; }
    }
    public record LeadsListModel : BasePagedListModel<LeadModels>
    {
    }
}
