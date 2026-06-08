using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectEmployeeMappingModel : BaseEntityModel
    {
        public string ProjectId { get; set; }

        public string EmployeeId { get; set; }

        public string EmployeeDesignationId { get; set; }
        public string ProjectUserMappingId { get; set; }
        public decimal? ProductivityFactor { get; set; }

        public string RoleStartDateStr { get; set; }

        public string RoleEndDateStr { get; set; }

        public string Flag { get; set; }

        public bool Deleted { get; set; }

        public virtual EmployeeModel Employee { get; set; }

        public virtual ProjectModel Project { get; set; }

        public virtual DropDown EmployeeRoleDropdown { get; set; }
        public virtual ProjectUserMapping ProjectUserMapping { get; set; }
    }
}