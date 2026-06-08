using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeOrgStructureModel : BaseEntityModel
    {
        public int Year { get; set; }

        public string EmployeeId { get; set; }

        public string ManagerId { get; set; }

        public string DepartmentId { get; set; }

        public string RoleId { get; set; }

        public int? Level { get; set; }

        public int? SortOrder { get; set; }

        public string Responsibilities { get; set; }

        public string Color { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public string UpdatedById { get; set; }

        public bool Deleted { get; set; }

        public string[] EmployeeDesignationIdsArray { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Employee Manager { get; set; }
        public virtual Department Department { get; set; }
        public virtual DropDown Role { get; set; }
        public virtual Site Site { get; set; }

        public virtual ICollection<EmployeeOrgStructureDesignationMapping> EmployeeOrgStructureDesignationMapping { get; set; } = new List<EmployeeOrgStructureDesignationMapping>();
    }

    public record EmployeeOrgStructureSearchModel : BaseSearchModel
    {
        public List<string> ManagerIds { get; set; }
        public List<string> EmployeeIds { get; set; }
        public List<string> DepartmentIds { get; set; }
        public List<string> EmployeeDesignationIds { get; set; }
        public int Years { get; set; }
        public string Level { get; set; }
        public string SearchText { get; set; }
    }

    public record EmployeeOrgStructureListModel : BasePagedListModel<EmployeeOrgStructureModel>
    {
        public bool editing { get; set; }
    }
    public record EmployeeOrgStructureUploadModel : BaseEntityModel
    {
    }
}