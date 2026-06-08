using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CustomerModel : BaseEntityModel
    {
        public string CompanyId { get; set; }
        public string PersonId { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CustomerType { get; set; }
        public string CustomerTypeId { get; set; }
        public string ParentCustomerName { get; set; }
        public string AssignedToId { get; set; }
        public string AssignedToName { get; set; }
        public int CustomerNoteCount { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }        
        public DateTime? AssignedDate { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedById { get; set; }

        // for customer files
        public string FileName { get; set; }
        public int Year { get; set; }
    }
    public record CustomerSearchModel : BaseSearchModel
    {
        public string CustomerId { get; set; }
        public string EmployeeId { get; set; }
    }

    public record CustomerListModel : BasePagedListModel<CustomerModel>
    {
    }
}
