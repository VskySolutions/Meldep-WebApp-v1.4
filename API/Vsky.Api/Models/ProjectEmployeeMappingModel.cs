using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectEmployeeMappingModel : BaseEntityModel
    {
        public string ProjectId { get; set; }
        public string EmployeeId { get; set; }

        public decimal? ProductivityFactor { get; set; }
        public string RoleStartDateStr { get; set; }
        public string RoleEndDateStr { get; set; }
        public List<string> SiteProjectRoleIds { get; set; }

        public string Flag { get; set; }
        public bool Deleted { get; set; }
        public bool Manage { get; set; }
        public bool View { get; set; }
        public bool Notes { get; set; }

        public virtual EmployeeModel Employee { get; set; }
        public virtual ProjectModel Project { get; set; }
        //public virtual List<ProjectEmployeeRoleMapping> Roles { get; set; } = new List<ProjectEmployeeRoleMapping>();
        public virtual ICollection<ProjectEmployeeRoleMapping> ProjectEmployeeRoleMappings { get; set; } = new List<ProjectEmployeeRoleMapping>();
    }
}