using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CompanyClientsModel : BaseEntityModel
    {
        public string CompanyId { get; set; }
        public string PersonId { get; set; }
        public string SiteId { get; set; }
        public string CustomerTypeId { get; set; }
        public string AssignedToId { get; set; }
        public string ParentCustomerId { get; set; }
        public string ClientId { get; set; }
        public string? ParentCustomerName { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string? CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string? UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public int CustomerNoteCount { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Person? Person { get; set; }
        public virtual Site Sites { get; set; }
        public virtual DropDown? CustomerType { get; set; }
        public virtual Employee? AssignedTo { get; set; }
        public virtual ApplicationUser? CreatedBy { get; set; }
        public virtual ApplicationUser? UpdatedBy { get; set; }
        //public virtual ICollection<CompanyContactsModels> CompanyContactModel { get; set; } = new List<CompanyContactsModels>();
    }
    public record CompanyClientsSearchModel : BaseSearchModel
    {
        public string CustomerId { get; set; }
        public string EmployeeId { get; set; }
        public string CustomerTypeId { get; set; }
        public string SearchText { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> EmployeeIds { get; set; }
        public List<string> AssignedToIds { get; set; }
        public List<string> CustomerTypeIds { get; set; }
        public List<string> ParentCustomerIds { get; set; }
    }

    public record CompanyClientsListModel : BasePagedListModel<CompanyClientsModel>
    {
    }
}
