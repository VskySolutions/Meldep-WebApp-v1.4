using System;
using System.Collections.Generic;
using Vsky.Core;
using System.Text.Json.Serialization;

namespace Vsky.Models;

public class ProjectEmployeeMapping : BaseEntity
{
    public string ProjectId { get; set; }

    public string EmployeeId { get; set; }

    public string EmployeeDesignationId { get; set; }
    public string ProjectUserMappingId { get; set; }
    public decimal? ProductivityFactor { get; set; }

    public DateTime? CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public bool Deleted { get; set; }

    public virtual Employee Employee { get; set; }

    public virtual Project Project { get; set; }

    public virtual DropDown EmployeeRoleDropdown { get; set; }
    public virtual ProjectUserMapping ProjectUserMapping { get; set; }

    public virtual VW_Project VW_Project { get; set; }

}

public class ProjectCharterEmployee
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeName { get; set; }

    public List<ProjectCharterEmployeeAssignedHours> EmployeeAssignedHours { get; set; } = new();
}

public class ProjectCharterEmployeeAssignedHours
{
    public DateTime WeekendDate { get; set; }

    [JsonConverter(typeof(HoursConverter))]
    public decimal TotalHours { get; set; }
}