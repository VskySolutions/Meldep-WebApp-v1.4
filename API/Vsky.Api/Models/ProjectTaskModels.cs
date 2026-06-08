using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectTaskModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string TaskId { get; set; }
        public string ProjectId { get; set; }
        public int ProjectTaskNumber { get; set; }
        public string ProjectModuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StatusId { get; set; }
        public string PriorityId { get; set; }
        public decimal EstimateTime { get; set; }
        public string Instructions { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public string AssignedToId { get; set; }
        public string TypeId { get; set; }
        public string IssueId { get; set; }
        public string RequirementId { get; set; }
        public string AreaId { get; set; }
        public string WorkspaceId { get; set; }
        public string ActionId { get; set; }

        public bool Active { get; set; }
        public bool IsMoved { get; set; }
        public bool IsIssueConverted { get; set; }
        public bool IsRequirementConverted { get; set; }

        public decimal SortOrder { get; set; }
        public string Flag { get; set; }
        public int ActivitiesCount { get; set; }
        public string TargetMonthStr { get; set; }
        public string Color { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> TaskIds { get; set; }
        public string IsCopyOrMove { get; set; }
        public bool IsDuplicate { get; set; }
        public string IsDuplicateFromId { get; set; }
        public List<IFormFile> ProjectTaskFiles { get; set; }
        public List<string> ExistingFiles { get; set; }
        public int ProjectNotesCount { get; set; }
        public string ProjectSwimlaneId { get; set; }
        public int ProjectTaskNotesCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? TaskMonth { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public decimal TotalTimesheetEstHours { get; set; }

        public virtual EmployeeModel AssignedTo { get; set; }
        public virtual DropDownModel Priority { get; set; }
        public virtual ProjectModel Project { get; set; }
        public virtual DropDownModel Status { get; set; }
        public virtual DropDown Type { get; set; }
        public virtual DropDownModel Area { get; set; }
        public virtual DropDownModel Workspace { get; set; }
        public virtual DropDownModel Action { get; set; }
        public virtual ProjectModuleModel ProjectModule { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual ICollection<ProjectActivityModel> ProjectActivityModel { get; set; } = new List<ProjectActivityModel>();
        public virtual ICollection<ProjectActivityModel> ProjectActivities { get; set; } = new List<ProjectActivityModel>();
        public virtual ICollection<ProjectTaskStatusLog> ProjectTaskStatusLog { get; set; } = new List<ProjectTaskStatusLog>();
        public virtual ICollection<ProjectTaskFilesModel> ProjectTaskFilesList { get; set; } = new List<ProjectTaskFilesModel>();
        public virtual ICollection<ProjectTask_Tags> ProjectTask_Tags { get; set; } = new List<ProjectTask_Tags>();
        public virtual ICollection<ProjectTaskRelatedMapping> ProjectTaskRelatedMappings { get; set; } = new List<ProjectTaskRelatedMapping>();
        public virtual ICollection<ProjectWeeklyPlanDatesReqTaskIssueMapping> ProjectWeeklyPlanDatesReqTaskIssueMappingList { get; set; } = new List<ProjectWeeklyPlanDatesReqTaskIssueMapping>();
    }

    public record ProjectTaskSearchModel : BaseSearchModel
    {
        // public bool CloseTask { get; set; }
        public string Name { get; set; }
        public List<string> ProjectIds { get; set; }
        public string ProjectId { get; set; }
        public string SearchTaskText { get; set; }
        public List<string> ProjectModuleIds { get; set; }
        public List<string> ProjectTaskIds { get; set; }
        public List<string> ProjectLeadsIds { get; set; }
        public List<string> StatusIds { get; set; }
        public List<string> PriorityIds { get; set; }
        public List<string> ActivityOwners { get; set; }
        public string ActivityOwnerId { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> CompanyContactIds { get; set; }
        public List<string> AssignedToIds { get; set; }
        public DateTime? StartDateStr { get; set; }
        public DateTime? EndDateStr { get; set; }
        public string TargetMonthStr { get; set; }
        public string SortByFilterId { get; set; }
        public string ProjectTaskId { get; set; }
        public string SearchText { get; set; }
        public List<string> TaskTagsIds { get; set; }
        public string ProjectSwimlaneId { get; set; }
        public string ProjectModuleId { get; set; }
        public string filterTask { get; set; }
        public bool isShowCloseStatus { get; set; }
        public string ViewType { get; set; }
        public string CalendarType { get; set; }
        public string CalendarMonthStr { get; set; }
        public int ProjectTaskNumber { get; set; }
        public int Offset { get; set; }
        public List<string> ProjectModuleStatusIds { get; set; }
        public bool IsTemplate { get; set; }
    }

    public record ProjectTaskListModel : BasePagedListModel<ProjectTaskModel>
    {
    }
    //public class ProjectTaskGroupModel
    //{
    //    public string StatusId { get; set; }  // Group Key
    //    //public IList<ProjectTaskModel> Tasks { get; set; }
    //    public string StatusName { get; set; }
    //    public List<ProjectTaskModel> Tasks { get; set; }
    //}
    public class ProjectTaskGroupModel
    {
        public string StatusId { get; set; }
        public string StatusValue { get; set; } // Status Name
        public string GroupId { get; set; }
        public string GroupValue { get; set; }

        public IList<ProjectTaskModel> Tasks { get; set; }
    }

    public class ProjectTaskListGroupedModel
    {
        public IList<ProjectTaskGroupModel> Data { get; set; }
        public int Total { get; set; }
    }
    public class ProjectTaskCalendarSearchModel
    {
        public List<int> Years { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> StatusIds { get; set; }
        public List<string> PriorityIds { get; set; }
        public List<string> ActivityOwnerIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class TaskCalendarModel
    {
        public List<TaskCalendarColumnModel> Columns { get; set; }
        public List<TaskCalendarRowModel> Rows { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class TaskCalendarColumnModel
    {
        public int Index { get; set; }
        public DateTime Date { get; set; }
        public string FilterType { get; set; }
        public string DateTooltip { get; set; }
        public string DisplayDateRange { get; set; }
    }

    public class TaskCalendarRowModel
    {
        public ProjectTask Task { get; set; }
        public int BeforeStartDateCount { get; set; }
        public int TaskDurationColspan { get; set; }
        public int AfterEndDateCount { get; set; }
    }
}