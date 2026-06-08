using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;
//using Vsky.Models.SwimLane;

namespace Vsky.Api.Models
{
    public record ProjectModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string CustomerId { get; set; }
        public string CompanyContactId { get; set; }
        public string ProjectTypeId { get; set; }
        public string ProjectPriorityId { get; set; }
        public string ProjectStatusId { get; set; }
        public string ProjectCoordinatorId { get; set; }
        public string CoordinatorId { get; set; }
        public string LeadId { get; set; }
        public string ProjectCategoryId { get; set; }
        public string ProjectSubcategoryId { get; set; }
        public string PlanApproverId { get; set; }

        public int Year { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string ActiveStatus { get; set; }
        public string EmployeeMappings { get; set; }

        public DateTime? StartDate { get; set; }
        public string StartDateStr { get; set; }
        public DateTime? EndDate { get; set; }
        public string EndDateStr { get; set; }
        public DateTime? GoLiveDate { get; set; }
        public string GoLiveDateStr { get; set; }


        public bool Active { get; set; }
        public bool editing { get; set; }


        public string Tab { get; set; }
        public string[] TeamMemberIds { get; set; }
        public string[] SystemAnalystIds { get; set; }
        public string[] SystemArchitectIds { get; set; }
        public string[] BillingAdminIds { get; set; }
        public string[] DevLeadIds { get; set; }
        public string[] TestLeadIds { get; set; }


        public int ProjectNotesCount { get; set; }
        public int ProjectMessageCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public int TotalTaskCount { get; set; }
        public int ProjectSwimlaneCount { get; set; }
        public int CompletedIssueCount { get; set; }
        public int TotalIssueCount { get; set; }
        public int CompletedRequirementCount { get; set; }
        public int TotalRequirementCount { get; set; }
        public decimal TotalTaskEstimateHours { get; set; }
        public decimal TotalActivityHours { get; set; }
        public int TotalModuleCount { get; set; }
        public int TotalTasksCount { get; set; }


        public bool IsTemplate { get; set; }
        public string TemplateName { get; set; }
        public string IsFrom { get; set; }

      
        public string ProjectFileFlag { get; set; }
        public string ProjectColor { get; set; }
        public bool IsPinned { get; set; }
        public int SortOrder { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public string IsProjectOrTemplate { get; set; }
        public List<IFormFile> ProjectFiles { get; set; }
        public List<string> ExistingFiles { get; set; }
       
        public List<string> LeadIds { get; set; }
        public List<string> CoordinatorIds { get; set; }
        public bool IsCharter { get; set; }
        //public string[] LeadIds { get; set; }

        //public virtual Company Customer { get; set; }
        //public virtual Person CompanyContact { get; set; }
        public virtual CompanyClients Customer { get; set; }
        public virtual CompanyContacts CompanyContact { get; set; }
        public virtual DropDownModel ProjectPriority { get; set; }
        public virtual DropDownModel ProjectStatus { get; set; }
        public virtual DropDownModel ProjectType { get; set; }
        public virtual EmployeeModel ProjectCoordinator { get; set; }
        public virtual EmployeeModel PlanApprover { get; set; }
        public virtual DropDownType ProjectCategories { get; set; }
        public virtual DropDown ProjectCategoriesSubCategories { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<ProjectActivityModel> ProjectActivities { get; set; } = new List<ProjectActivityModel>();
        public virtual ICollection<ProjectEmployeeMappingModel> ProjectEmployeeMappings { get; set; } = new List<ProjectEmployeeMappingModel>();
        public virtual ICollection<ProjectFilesModel> ProjectFileList { get; set; } = new List<ProjectFilesModel>();
        public virtual ICollection<ProjectTaskModel> ProjectTasks { get; set; } = new List<ProjectTaskModel>();
        public virtual ICollection<ProjectModuleModel> ProjectModules { get; set; } = new List<ProjectModuleModel>();
        public virtual ICollection<ProjectsMessagesModel> ProjectsMessages { get; set; } = new List<ProjectsMessagesModel>();
        public virtual ICollection<ProjectUserMappingModel> ProjectUserMappings { get; set; } = new List<ProjectUserMappingModel>();
        public virtual ICollection<ProjectTags> ProjectTags { get; set; } = new List<ProjectTags>();
        public virtual ICollection<ProjectColor> ProjectColors { get; set; } = new List<ProjectColor>();

        //summery function data
        public virtual ICollection<IssueModel> Issue { get; set; } = new List<IssueModel>();
        public virtual ICollection<RequirementModel> Requirement { get; set; } = new List<RequirementModel>();
        public virtual ICollection<TestPlanModel> TestPlans { get; set; } = new List<TestPlanModel>();
        public virtual ICollection<TimesheetLinesModel> TimesheetLine { get; set; } = new List<TimesheetLinesModel>();
        public virtual ICollection<ProjectWeeklyPlanModel> ProjectWeeklyPlans { get; set; } = new List<ProjectWeeklyPlanModel>();
        public ICollection<InfraProjectServices> InfraProjectServices { get; set; }
        public IEnumerable<ProjectWeeklyPlanModel> WeeklyPlan { get; set; }
        public IEnumerable<ProjectWeeklyPlanModel> MonthlyPlan { get; set; }

    }

    public record ProjectSearchModel : BaseSearchModel
    {
        public List<string> ProjectIds { get; set; }
        public List<string> ProjectTeamMemberIds { get; set; }
        public List<string> ProjectCoordinatorIds { get; set; }
        public List<string> ProjectLeadsIds { get; set; }
        public List<string> ProjectStatusIds { get; set; }
        public string StatusId { get; set; }
        public string CustomerId { get; set; }
        public List<string> ProjectPriorityIds { get; set; }
        public List<string> ProjectTypeIds { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> CompanyContactIds { get; set; }
        public string SearchText { get; set; }
        public bool IsTemplate { get; set; }
        public List<string> ProjectCategoryIds { get; set; }
        public List<string> ProjectTagIds { get; set; }
        public bool isShowCloseStatus { get; set; }
    }

    public record AllProjectPlannerSearchModel : BaseSearchModel
    {
        public string ProjectId { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> ProjectTypeIds { get; set; }
        public string Year { get; set; }
        public string filterProject { get; set; }
        public string SearchText { get; set; }

        public List<string> ProjectCoordinatorIds { get; set; }
        public List<string> ProjectLeadsIds { get; set; }
        public List<string> ProjectStatusIds { get; set; }
        public string StatusId { get; set; }
        public List<string> ProjectPriorityIds { get; set; }
        public List<string> CompanyContactIds { get; set; }

    }

    public record VW_CustomerSearchModel : BaseSearchModel
    {
        public List<string> CustomerIds { get; set; }
        public List<string> CustomerTypeIds { get; set; }
        public List<string> CustomerAssignToIds { get; set; }
        public List<string> ParentCustomerIds { get; set; }
        public List<string> ProjectTypeIds { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> ProjectStatusIds { get; set; }
        public List<string> ProjectPriorityIds { get; set; }
        public List<string> ProjectCoordinatorIds { get; set; }
        public List<string> ProjectLeadsIds { get; set; }
        public List<string> CompanyContactIds { get; set; }

        public string StatusId { get; set; }
        public string ProjectId { get; set; }
        public string SearchText { get; set; }
        public string Year { get; set; }
    }

    public record ProjectListModel : BasePagedListModel<ProjectModel>
    {
        public bool editing { get; set; }
    }
    public record ProjectUploadModel : BaseEntityModel
    {
    }
}