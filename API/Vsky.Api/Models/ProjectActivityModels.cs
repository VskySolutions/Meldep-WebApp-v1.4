using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectActivityModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public string TaskId { get; set; }
        public string ProjectModuleId { get; set; }
        public string ActivityStatusId { get; set; }
        public string Name { get; set; }
        public string StateStatus { get; set; }
        public string ProjectName { get; set; }
        public string ProjectModuleName { get; set; }
        public string TaskName { get; set; }
        public string AssignedToName { get; set; }
        public string Description { get; set; }
        public string AssignedToId { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public string TargetMonthStr { get; set; }
        public string EstimateHoursStr { get; set; }
        public decimal EstimateHours { get; set; }
        public bool Active { get; set; } = true;
        public bool Deleted { get; set; }
        public int SortOrder { get; set; }
        public string Flag { get; set; }
        public string LoginEmployee { get; set; }
        public string ProjectSwimlaneId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityNameDescription { get; set; }
        public string DisplayText { get; set; }
        public List<IFormFile> ProjectTaskActivityFiles { get; set; }
        public List<string> ExistingFiles { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? TargetMonth { get; set; }

        public int ActivitiesCount { get; set; }
        public List<string> ActivityIds { get; set; }

        public virtual EmployeeModel AssignedTo { get; set; }
        public virtual ProjectModel Project { get; set; }
        public virtual ProjectTaskModel Task { get; set; }
        public virtual ProjectModuleModel ProjectModule { get; set; }
        public virtual DropDownModel ProjectPriority { get; set; }
        public virtual DropDownModel ActivityStatus { get; set; }
        public virtual DropDownModel ProjectType { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public virtual ICollection<ProjectActivityModel> ProjectActivities { get; set; } = new List<ProjectActivityModel>();
        public virtual ICollection<ProjectActivityModel> ProjectActivityLines { get; set; } = new List<ProjectActivityModel>();
        public virtual ICollection<ProjectEmployeeMappingModel> ProjectEmployeeMappings { get; set; } = new List<ProjectEmployeeMappingModel>();
        public virtual ICollection<ProjectActivityFilesModel> ProjectTaskActivityFilesList { get; set; } = new List<ProjectActivityFilesModel>();
        public virtual ICollection<ProjectTaskModel> ProjectTasks { get; set; } = new List<ProjectTaskModel>();

    }

    public record ProjectActivitySearchModel : BaseSearchModel
    {
        public List<string> ProjectIds { get; set; }
        public string ProjectId { get; set; }
        public List<string> ProjectModuleIds { get; set; }
        public List<string> ActivityNameIds { get; set; }
        public string TargetMonthStr { get; set; }
        public string ActivityTargetMonthStr { get; set; }
        public string ActiveStatus { get; set; }
        public List<string> AssignedToIds { get; set; }
        public List<string> ActivityStatusIds { get; set; }
        public List<string> StatusIds { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> CompanyContactIds { get; set; }
        public string SearchText { get; set; }
        public string filterActivity { get; set; }
        public string ProjectSwimlaneId { get; set; }
        public string ProjectModuleId { get; set; }
        public string ProjectTaskId { get; set; }
        public bool isShowCloseStatus { get; set; }
        public bool isShowCompleteStatus { get; set; }
        public string Flag { get; set; }        
        public DateTime? SprintWeekEndDate { get; set; }
    }

    public record ProjectActivityListModel : BasePagedListModel<ProjectActivityModel>
    {
        public bool editing { get; set; }
        public List<string> ActivityNameDescription { get; set; }
    }
}