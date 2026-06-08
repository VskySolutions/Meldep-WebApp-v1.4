using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Lead : BaseEntity
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

        [NotMapped]
        public int LeadNotesCount { get; set; }

        public virtual Person Person { get; set; }
        public virtual CompanyClients Client { get; set; }
        public virtual DropDown LeadSources { get; set; }
        public virtual DropDown LeadGroup { get; set; }
        public virtual Company Company { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<LeadActivityLogs> LeadActivityLogs { get; set; } = new List<LeadActivityLogs>();


        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string Address { get; set; }
        //public string Email { get; set; }
        //public string PhoneNumber { get; set; }
        //public string Description { get; set; }
        //public string City { get; set; }
        //public string CountryId { get; set; }
        //public string StateProvinceId { get; set; }
        //public string ProjectId { get; set; }
        //public DateTime? CreatedOnUtc { get; set; }
        //public DateTime? UpdatedOnUtc { get; set; }
        //public string CreatedById { get; set; }
        //public string UpdatedById { get; set; }
        //public bool Deleted { get; set; }
        //public string LeadSources { get; set; }
        //public string Address2 { get; set; }
        //public string ProjectType { get; set; }
        //public string JobStatus { get; set; }
        //public string ZipCode { get; set; }
        //public virtual Country Country { get; set; }
        //public virtual StateProvince StateProvince { get; set; }
        //public virtual Site Site { get; set; }
        //public virtual Project Project { get; set; }
        //public virtual DropDown LeadSourcesType { get; set; }
        //public virtual DropDown LeadProjectType { get; set; }
        //public virtual DropDown LeadJobStatus { get; set; }

    }
}
