using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeStatusModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeStatusId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string StatusStartDateStr { get; set; }

        public string StatusEndDateStr { get; set; }
        public string Flag { get; set; }

        public bool Active { get; set; }
        public string Duration { get; set; }
        public string Note { get; set; }

        public virtual EmployeeModel Employee { get; set; }

        public virtual DropDownModel Status { get; set; }

        public virtual ICollection<EmployeeStatusModel> statusEntities { get; set; } = new List<EmployeeStatusModel>();
    }
}

