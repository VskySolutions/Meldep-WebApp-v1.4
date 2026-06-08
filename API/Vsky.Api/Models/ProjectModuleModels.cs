using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectModuleModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string ProjectModuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProjectModuleNumber { get; set; }
        public string ProjectModuleTypeId { get; set; }
        public string ProjectModuleStatusId { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public string CreatedOnUtcStr { get; set; }
        public string Notes { get; set; }
        public string ProjectId { get; set; }
        public bool Active { get; set; }
        public int SortOrder { get; set; }
        public bool IsDuplicate { get; set; }
        public bool IsMoved { get; set; }
        public string IsDuplicateFromId { get; set; }
        public string MovedFromProjectId { get; set; }
        public int ProjectTasksCount { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public List<IFormFile> ProjectModuleFiles { get; set; }
        public List<string> ExistingFiles { get; set; }
        public string ProjectSwimlaneId { get; set; }
        public string Color { get; set; }

        public int ProjectModuleNotesCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsIssueConverted { get; set; }
        public bool IsRequirementConverted { get; set; }

        public virtual Project Project { get; set; }
        public virtual DropDown ProjectModuleStatus { get; set; }
        public virtual DropDown ProjectModuleType { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual ICollection<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();
        public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
        public virtual ICollection<ProjectTaskModel> ProjectTaskModel { get; set; } = new List<ProjectTaskModel>();
        public virtual ICollection<ProjectModuleFilesModel> ProjectModuleFilesList { get; set; } = new List<ProjectModuleFilesModel>();
        public virtual ICollection<ProjectModulesUserMapping> ProjectModulesUserMappings { get; set; } = new List<ProjectModulesUserMapping>();

    }

    public record ProjectModuleSearchModel : BaseSearchModel
    {
        public List<string> ProjectIds { get; set; }
        public List<string> ProjectModuleIds { get; set; }
        public string ProjectId { get; set; }
        public List<string> ProjectModuleTypeIds { get; set; }
        public List<string> ProjectModuleStatusIds { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> CompanyContactIds { get; set; }
        public bool isShowCloseStatus { get; set; }
        public string pageName { get; set; }
        public string SearchText { get; set; }
        public string filterModule { get; set; }
    }

    public record ProjectModuleListModel : BasePagedListModel<ProjectModuleModel>
    {
    }
    public class ModuleCalendarModel
    {
        public List<TaskCalendarColumnModel> Columns { get; set; }
        public List<ModuleCalendarRowModel> Rows { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
    public class ModuleCalendarRowModel
    {
        public ProjectModule Module { get; set; }
        public int BeforeStartDateCount { get; set; }
        public int ModuleDurationColspan { get; set; }
        public int AfterEndDateCount { get; set; }
    }
    public class NextSortOrder
    {
        public int NextSortOrderOfProjectModule { get; set; }
        public decimal CurrentModuleSortOrder { get; set; }
        public int SelectedModuleSortOrder { get; set; }
        public decimal NextSortOrderOfProjectTask { get; set; }
    }
}