using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeDesignationModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeDesignationId { get; set; }
        public string ShiftId { get; set; }
        public string LeaveApproverId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string DesignationStartDateStr { get; set; }

        public string DesignationEndDateStr { get; set; }
        public string Flag { get; set; }

        public bool Active { get; set; }
        public string Duration { get; set; }
        public string Note { get; set; }

        public virtual EmployeeModel Employee { get; set; }
        public virtual EmployeeModel LeaveApprover { get; set; }
        public virtual DropDownModel Designation { get; set; }
        public virtual DropDownModel Shift { get; set; }

        public virtual ICollection<EmployeeDesignationModel> DesignationEntities { get; set; } = new List<EmployeeDesignationModel>();
    }
}

