using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CompanyContactsModels : BaseEntityModel
    {
        public string AlternateEmail { get; set; }
        //public string SiteId { get; set; }
        public string AlternatePhoneNumber { get; set; }
        public string CompanyId { get; set; }
        public string PersonId { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string Flag { get; set; }

        public int CompanyContactNotesCount { get; set; }
        public virtual CompanyModel Company { get; set; }
        //public virtual Site Site { get; set; }
        public virtual PersonModel Person { get; set; }
    }
    public record CompanyContactSearchModel : BaseSearchModel
    {
        public string CompanyId { get; set; }
        public string SearchText { get; set; }
    }

    public record CompanyContactListModel : BasePagedListModel<CompanyContactsModels>
    {
    }
}
