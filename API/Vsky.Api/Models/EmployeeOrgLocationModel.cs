using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeOrgLocationModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public string OrgLocationId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string OrgLocationStartDateStr { get; set; }

        public string OrgLocationEndDateStr { get; set; }
        public string Flag { get; set; }

        public bool Active { get; set; }
        public string Duration { get; set; }
        public string Note { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual DropDown OrgLocation { get; set; }

        public virtual ICollection<EmployeeOrgLocationModel> OrgLocationEntities { get; set; } = new List<EmployeeOrgLocationModel>();
    }
}

