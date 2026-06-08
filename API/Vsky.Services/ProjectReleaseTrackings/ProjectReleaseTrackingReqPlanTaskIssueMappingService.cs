using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectReleaseTrackings
{
    public class ProjectReleaseTrackingReqPlanTaskIssueMappingService : IProjectReleaseTrackingReqPlanTaskIssueMappingService
    {
        #region Define Services
        private readonly IRepository<ProjectReleaseTrackingReqPlanTaskIssueMapping> _ProjectReleaseTrackingReqPlanTaskIssueMappingRepository;
        private readonly IRepository<Requirement> _requirementRepository;
        private readonly IRepository<Issue> _issueRepository;
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<Models.ProjectWeeklyPlan> _projectWeeklyPlanRepository;
        #endregion

        #region Services Initializations
        public ProjectReleaseTrackingReqPlanTaskIssueMappingService(
            IRepository<ProjectReleaseTrackingReqPlanTaskIssueMapping> ProjectReleaseTrackingReqPlanTaskIssueMappingRepository,
            IRepository<Requirement> requirementRepository,
            IRepository<Issue> issueRepository,
            IRepository<ProjectTask> projectTaskRepository,
            IRepository<Models.ProjectWeeklyPlan> projectWeeklyPlanRepository
        )
        {
            _ProjectReleaseTrackingReqPlanTaskIssueMappingRepository = ProjectReleaseTrackingReqPlanTaskIssueMappingRepository;
            _requirementRepository = requirementRepository;
            _issueRepository = issueRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectWeeklyPlanRepository = projectWeeklyPlanRepository;
        }
        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetProjectReleaseTrackingReqPlanTaskIssueMappingById
        // Title: GetProjectReleaseTrackingReqPlanTaskIssueMappingById
        // Description: This method retrieves a GetProjectReleaseTrackingReqPlanTaskIssueMapping from the database by its unique identifier (`id`). 
        public async Task<ProjectReleaseTrackingReqPlanTaskIssueMapping> GetProjectReleaseTrackingReqPlanTaskIssueMappingById(string id)
        {
            var query = _ProjectReleaseTrackingReqPlanTaskIssueMappingRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllReqPlanTaskIssuesByProjectId
        public async Task<List<ProjectReqPlanTaskIssueItemDto>> GetAllReqPlanTaskIssuesByProjectId(string projectId, string SiteId)
        {
            var excludedTaskStatuses = new[] { "close", "completed", "on hold" };
            var excludedRequirementStatuses = new[] { "close", "on hold" };
            var excludedIssueStatuses = new[] { "closed", "on hold" };

            var requirements = _requirementRepository.TableNoTracking
            .Where(x =>
                !x.Deleted &&
                x.SiteId == SiteId &&
                x.ProjectId == projectId &&
                !excludedRequirementStatuses.Contains(x.Status.DropDownValue.ToLower())
            )
            .Select(x => new ProjectReqPlanTaskIssueItemDto
            {
                Id = x.Id,
                Number = x.RequirementNumber,
                Name = x.Title,
                Type = "Requirement",
                Date = (DateTime?)x.CreatedOnUtc
            });

            var issues = _issueRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.SiteId == SiteId &&
                    x.ProjectId == projectId &&
                    !excludedIssueStatuses.Contains(x.Status.DropDownValue.ToLower())
                )
                .Select(x => new ProjectReqPlanTaskIssueItemDto
                {
                    Id = x.Id,
                    Type = "Issue",
                    Number = x.IssueNumber,
                    Name = x.Name,
                    Date = (DateTime?)x.CreatedOnUtc
                });

            var tasks = _projectTaskRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.SiteId == SiteId &&
                    x.ProjectId == projectId &&
                    !x.Project.IsTemplate &&
                    x.Project.Active &&
                    !x.IsMoved &&
                    !excludedTaskStatuses.Contains(x.Status.DropDownValue.ToLower())
                )
                .Select(x => new ProjectReqPlanTaskIssueItemDto
                {
                    Id = x.Id,
                    Type = "Task",
                    Number = x.ProjectTaskNumber,
                    Name = x.Name,
                    Date = (DateTime?)x.StartDate
                });

            // Combine all
            var reqList = await requirements.ToListAsync();
            var issueList = await issues.ToListAsync();
            var taskList = await tasks.ToListAsync();

            var result = reqList
                .Concat(issueList)
                .Concat(taskList)
                .OrderByDescending(x => x.Date)
                .ToList();

            return result;
        }
        #endregion

        #region GetAllProjectReleaseTrackingReqPlanTaskIssueMappingByProjectReleaseTrackingId
        public async Task<List<ProjectReleaseTrackingReqPlanTaskIssueMapping>> GetAllProjectReleaseTrackingReqPlanTaskIssueMappingByProjectReleaseTrackingId(string ProjectReleaseTrackingId)
        {
            var query = _ProjectReleaseTrackingReqPlanTaskIssueMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ReleaseTrackingId == ProjectReleaseTrackingId);

            query = query.Select(m => new ProjectReleaseTrackingReqPlanTaskIssueMapping
            {
                Id = m.Id,
                RequirementId = m.RequirementId,
                TaskId = m.TaskId,
                IssueId = m.IssueId,

                Requirement = m.RequirementId != null ? new Requirement
                {
                    Id = m.Requirement.Id,
                    RequirementNumber = m.Requirement.RequirementNumber,
                    Title = m.Requirement.Title,
                    CreatedOnUtc = m.Requirement.CreatedOnUtc
                } : null,

                Task = m.TaskId != null ? new ProjectTask
                {
                    Id = m.Task.Id,
                    ProjectTaskNumber = m.Task.ProjectTaskNumber,
                    Name = m.Task.Name,
                    CreatedOnUtc = m.Task.CreatedOnUtc
                } : null,

                Issue = m.IssueId != null ? new Issue
                {
                    Id = m.Issue.Id,
                    IssueNumber = m.Issue.IssueNumber,
                    Name = m.Issue.Name,
                    CreatedOnUtc = m.Issue.CreatedOnUtc
                } : null
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region InsertProjectReleaseTrackingReqPlanTaskIssueMapping
        // Title: InsertProjectReleaseTrackingReqPlanTaskIssueMapping
        // Description: This method inserts a new InsertProjectReleaseTrackingReqPlanTaskIssueMapping entity into the repository. It takes a InsertProjectReleaseTrackingReqPlanTaskIssueMapping object as input and uses the _ProjectReleaseTrackingReqPlanTaskIssueMappingRepository to handle the insertion operation.
        public void InsertProjectReleaseTrackingReqPlanTaskIssueMapping(ProjectReleaseTrackingReqPlanTaskIssueMapping entity)
        {
            _ProjectReleaseTrackingReqPlanTaskIssueMappingRepository.Insert(entity);
        }
        #endregion

        #region DeleteProjectReleaseTrackingReqPlanTaskIssueMapping
        public void DeleteProjectReleaseTrackingReqPlanTaskIssueMapping(Models.ProjectReleaseTrackingReqPlanTaskIssueMapping entity)
        {
            entity.Deleted = true;
            _ProjectReleaseTrackingReqPlanTaskIssueMappingRepository.Update(entity);
        }
        #endregion
    }
}
