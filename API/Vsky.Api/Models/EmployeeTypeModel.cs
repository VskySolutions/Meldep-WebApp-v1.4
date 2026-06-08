using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeTypeModel : BaseEntityModel
    {
        public string EmployeeTypeId { get; set; }

        public string EmployeeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string StartDateStr { get; set; }

        public string EndDateStr { get; set; }

        public string Flag { get; set; }

        public bool Active { get; set; }
        public string Duration { get; set; }
        public string Note { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual DropDown EmployeeTypeDropdown { get; set; }

        public virtual ICollection<EmployeeTypeModel> typeEntities { get; set; } = new List<EmployeeTypeModel>();
    }
}
