using System;
using System.Collections.Generic;
using Vsky.Core;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vsky.Models
{
    public class ProjectEmployeeMapping : BaseEntity
    {
        public string ProjectId { get; set; }

        public string EmployeeId { get; set; }

        public decimal? ProductivityFactor { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public bool Deleted { get; set; }
        [NotMapped]
        public bool Manage { get; set; }
        [NotMapped]
        public bool View { get; set; }
        [NotMapped]
        public bool Notes { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Project Project { get; set; }
        public virtual VW_Project VW_Project { get; set; }
        [NotMapped]
        //public virtual List<ProjectEmployeeRoleMapping> Roles { get; set; } = new List<ProjectEmployeeRoleMapping>();
        public virtual ICollection<ProjectEmployeeRoleMapping> ProjectEmployeeRoleMappings { get; set; } = new List<ProjectEmployeeRoleMapping>();
    }
    public class ProjectEmployeeRoleMapping : BaseEntity
    {
        public string ProjectEmployeeMappingId { get; set; }
        public string SiteProjectRoleId { get; set; }

        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public virtual ProjectEmployeeMapping ProjectEmployeeMapping { get; set; }
        public virtual SitesProjectRoles SitesProjectRoles { get; set; }
    }
    public class ProjectCharterEmployee
    {
        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
    public bool IsActivityAssigned { get; set; }

        public List<ProjectCharterEmployeeAssignedHours> EmployeeAssignedHours { get; set; } = new();
    }
    public class ProjectCharterEmployeeAssignedHours
    {
        public DateTime WeekendDate { get; set; }

        [JsonConverter(typeof(HoursConverter))]
        public decimal TotalHours { get; set; }
    }
}