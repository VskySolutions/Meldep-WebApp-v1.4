using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeDepartmentModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeDepartmentId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string DepartmentStartDateStr { get; set; }

        public string DepartmentEndDateStr { get; set; }
        public string Flag { get; set; }

        public bool Active { get; set; }
        public string Duration { get; set; }
        public string Note { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual Department Department { get; set; }

        public virtual ICollection<EmployeeDepartmentModel> DepartmentEntities { get; set; } = new List<EmployeeDepartmentModel>();
    }
}

