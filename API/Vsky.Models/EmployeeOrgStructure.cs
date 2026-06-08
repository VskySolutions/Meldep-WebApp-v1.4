using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class EmployeeOrgStructure : BaseEntity
{
    public string SiteId { get; set; }
   
    public string EmployeeId { get; set; }

    public string ManagerId { get; set; }

    public string DepartmentId { get; set; }

    public string RoleId { get; set; }

    public int Year { get; set; }

    public int? Level { get; set; }

    public int? SortOrder { get; set; }

    public string Responsibilities { get; set; }

    public string Color { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public string UpdatedById { get; set; }

    public bool Deleted { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual Employee Manager { get; set; }
    public virtual Department Department { get; set; }
    public virtual DropDown Role { get; set; }
    public virtual Site Site { get; set; }

    public virtual ICollection<EmployeeOrgStructureDesignationMapping> EmployeeOrgStructureDesignationMapping { get; set; } = new List<EmployeeOrgStructureDesignationMapping>();
}

public class OrgChartPreview
{
    public string Id { get; set; }
    public string? ParentId { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public string Department { get; set; }
    public string Responsibilities { get; set; }
    public string Color { get; set; }
    public string Image { get; set; }
    public int SortOrder { get; set; }
}

public class EmployeeOrgStructureDesignationMapping : BaseEntity
{
    public string EmployeeOrgStructureId { get; set; }
    public string EmployeeDesignationId { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public virtual EmployeeOrgStructure EmployeeOrgStructure { get; set; }
    public virtual DropDown EmployeeDesignation { get; set; }
}