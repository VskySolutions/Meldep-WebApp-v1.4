using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
//using OfficeOpenXml;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.EmployeeLeaves;
using Vsky.Services.Sites;

namespace Vsky.Services.ProjectActivities
{
    public class ProjectActivityService : IProjectActivityService
    {
        #region Define Services
        private readonly IRepository<ProjectActivity> _projectActivityRepository;
        private readonly IRepository<ProjectWeeklyPlanDatesReqTaskIssueMapping> _projectWeeklyPlanDatesReqTaskIssueMapping;
        private readonly IRepository<Notes> _notesRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations
        public ProjectActivityService
            (
            IRepository<ProjectActivity> projectActivityRepository,
            IRepository<ProjectWeeklyPlanDatesReqTaskIssueMapping> projectWeeklyPlanDatesReqTaskIssueMapping,
            IRepository<Notes> notesRepository,
            UserManager<ApplicationUser> userManager,
            ICommonService commonService
            )
        {
            _projectActivityRepository = projectActivityRepository;
            _projectWeeklyPlanDatesReqTaskIssueMapping = projectWeeklyPlanDatesReqTaskIssueMapping;
            _notesRepository = notesRepository;
            _userManager = userManager;
            _commonService = commonService;
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

        #region GetAllProjectActivities
        // Title: GetAllProjectActivities
        // Description: This method retrieves a paginated list of project activities based on various search criteria such as project activity name,project module,activity owner,activity name,activity status,target month. The method allows for both full and lookup (limited) data retrieval modes.
        // Optimization Completed - MT
        public async Task<IPagedList<ProjectActivity>> GetAllProjectActivities(string SiteId, 
            string userId, 
            string createdBy, 
            string SearchText,
            string activeStatus,
            List<string> projectIds, 
            List<string> projectModuleIds, 
            List<string> assignedToIds, 
            List<string> activityNameIds,
            List<string> activityStatusIds,
            List<string> statusIds,
            List<string> customerIds,
            List<string> companyContactIds,
            DateTime? SprintWeekEndDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending, 
            int page = 1,
            int pageSize = int.MaxValue, 
            bool lookup = false
        )
        {
            var excludedProjectStatuses = new[] { "Completed", "Cancelled", "On Hold" };
            var excludedTaskStatuses = new[] { "New", "Close", "Completed" };
            var excludedActivityStatuses = new[] { "Completed", "Close" };

            // Initial query
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && !excludedProjectStatuses.Contains(x.Project.ProjectStatus.DropDownValue));

            //bool isAdmin = await IsCurrentUserAdmin(userId);
            //if (!isAdmin)
            //    query = query.Where(x => x.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == userId && (m.FullAccess || m.ViewOnly || m.Notes)));

            // Apply filters
            if (!string.IsNullOrWhiteSpace(activeStatus)) query = query.Where(x => activeStatus == "Active" ? x.Active : !x.Active);

            if (projectIds?.Any() == true)
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (projectModuleIds?.Any() == true)
                query = query.Where(x => projectModuleIds.Contains(x.ProjectModuleId));

            if (assignedToIds?.Any() == true)
                query = query.Where(x => assignedToIds.Contains(x.AssignedToId));

            if (activityNameIds?.Any() == true)
                query = query.Where(x => activityNameIds.Contains(x.Name));

            // "Activity Status" exclusion ONLY if user has NOT selected activityStatusIds
            if (activityStatusIds?.Any() != true)
                query = query.Where(x => !excludedActivityStatuses.Contains(x.ActivityStatus.DropDownValue));
            else
                query = query.Where(x => activityStatusIds.Contains(x.ActivityStatusId));

            // task status filter
            // "Task Status" exclusion ONLY if user has NOT selected taskStatusId
            if (statusIds?.Any() != true)
                query = query.Where(x => !excludedTaskStatuses.Contains(x.Task.Status.DropDownValue));
            else
                query = query.Where(x => statusIds.Contains(x.Task.StatusId));

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.Project.CustomerId));

            if (companyContactIds != null && companyContactIds.Any())
                query = query.Where(x => companyContactIds.Contains(x.Project.CompanyContactId));

            if (SprintWeekEndDate != null)
            {
                query = query.Where(x =>
                    x.Task.ProjectWeeklyPlanDatesReqTaskIssueMappingList.Any(m =>
                        !m.Deleted &&
                        m.TaskId == x.TaskId &&
                        m.ProjectWeeklyPlanDates.WeekDate.Date == SprintWeekEndDate.Value.Date &&
                        m.ProjectWeeklyPlanDates.PlanType.DropDownValue.ToLower() == "weekly"
                    ));
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Task.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProjectModule.Name.ToLower().Contains(SearchText.ToLower()) ||
                    (m.AssignedTo.Person.FirstName + " " + m.AssignedTo.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ActivityStatus.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.Task.Status.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.EstimateHours.ToString().ToLower().Contains(SearchText.ToLower()) ||
                    //m.TargetMonth.ToString().ToLower().Contains(SearchText.ToLower()) ||
                    m.StartDate.Value.Date == parsedDate.Date ||
                    m.EndDate.Value.Date == parsedDate.Date
                );
            }

            query = GetSorted(query, sortBy, descending);

            // Apply multi-level dictionary sorting
            if (sorts != null && sorts.Count > 0)
            {
                query = _commonService.ApplySorting(query, sorts);
            }

            // Project only necessary fields
            query = query.Select(x => new ProjectActivity
            {

                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                DueDate = x.DueDate,
                EstimateHours = x.EstimateHours,
                AssignedToId = x.AssignedToId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                //SiteId = x.SiteId,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                Active = x.Active,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    ProjectId = x.Task.ProjectId,
                    ProjectModuleId = x.Task.ProjectModuleId,
                    PriorityId = x.Task.PriorityId,
                    StartDate = x.Task.StartDate,
                    EndDate = x.Task.EndDate,
                    EstimateTime = x.Task.EstimateTime,
                    Description = x.Task.Description,
                    Status = new DropDown
                    {
                        Id = x.Task.Status.Id,
                        DropDownValue = x.Task.Status.DropDownValue
                    },
                    ProjectWeeklyPlanDatesReqTaskIssueMappingList = x.Task.ProjectWeeklyPlanDatesReqTaskIssueMappingList.Where(p => !p.Deleted && p.ProjectWeeklyPlanDates.PlanType.DropDownValue.ToLower() == "weekly").Select(p => new ProjectWeeklyPlanDatesReqTaskIssueMapping
                    {
                        Id = p.Id,
                        ProjectWeeklyPlanDates = new ProjectWeeklyPlanDates
                        {
                            Id = p.ProjectWeeklyPlanDates.Id,
                            WeekDate = p.ProjectWeeklyPlanDates.WeekDate
                        }
                    }).ToList()
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    ProjectStatus = new DropDown
                    {
                        Id = x.Project.ProjectStatus.Id,
                        DropDownValue = x.Project.ProjectStatus.DropDownValue,
                    }
                    //ProjectUserMappings = x.Project.ProjectUserMappings
                    //    .Where(m => !m.Deleted && m.ProjectId == x.Project.Id && (isAdmin || m.AspNetUserId == userId))
                    //    .Take(1).Select(m => new ProjectUserMapping
                    //    {
                    //        Id = m.Id,
                    //        FullAccess = m.FullAccess,
                    //        ViewOnly = m.ViewOnly,
                    //        Notes = m.Notes
                    //    }).ToList()
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person

                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                        FullName = x.AssignedTo.Person.FirstName + " " + x.AssignedTo.Person.LastName,
                    }
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue
                },
                ActivitiesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Project Activity").Count()
            });
            // Create a PagedList using the fetched results
            var pagedList = new PagedList<ProjectActivity>(query, page, pageSize);
            return pagedList;
        }

        public async Task<IPagedList<object>> GetAllProjectActivitiesForExpandCollapse(string SiteId,
            string userId,
            string createdBy,
            string SearchText,
            string activeStatus,
            List<string> projectIds,
            List<string> projectModuleIds,
            List<string> assignedToIds,
            List<string> activityNameIds,
            List<string> activityStatusIds,
            List<string> statusIds,
            List<string> customerIds,
            List<string> companyContactIds,
            DateTime? SprintWeekEndDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var excludedProjectStatuses = new[] { "Completed", "Cancelled", "On Hold" };
            var excludedTaskStatuses = new[] { "New", "Close", "Completed" };
            var excludedActivityStatuses = new[] { "Completed", "Close" };

            // Initial query
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && !x.Project.IsTemplate && !excludedProjectStatuses.Contains(x.Project.ProjectStatus.DropDownValue));

            //bool isAdmin = await IsCurrentUserAdmin(userId);
            //if (!isAdmin)
            //    query = query.Where(x => x.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == userId && (m.FullAccess || m.ViewOnly || m.Notes)));

            // Apply filters
            if (!string.IsNullOrWhiteSpace(activeStatus)) query = query.Where(x => activeStatus == "Active" ? x.Active : !x.Active);

            if (projectIds?.Any() == true)
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (projectModuleIds?.Any() == true)
                query = query.Where(x => projectModuleIds.Contains(x.ProjectModuleId));

            if (assignedToIds?.Any() == true)
                query = query.Where(x => assignedToIds.Contains(x.AssignedToId));

            if (activityNameIds?.Any() == true)
                query = query.Where(x => activityNameIds.Contains(x.Name));

            // "Activity Status" exclusion ONLY if user has NOT selected activityStatusIds
            if (activityStatusIds?.Any() != true)
                query = query.Where(x => !excludedActivityStatuses.Contains(x.ActivityStatus.DropDownValue));
            else
                query = query.Where(x => activityStatusIds.Contains(x.ActivityStatusId));

            // task status filter
            // "Task Status" exclusion ONLY if user has NOT selected taskStatusId
            if (statusIds?.Any() != true)
                query = query.Where(x => !excludedTaskStatuses.Contains(x.Task.Status.DropDownValue));
            else
                query = query.Where(x => statusIds.Contains(x.Task.StatusId));

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.Project.CustomerId));

            if (companyContactIds != null && companyContactIds.Any())
                query = query.Where(x => companyContactIds.Contains(x.Project.CompanyContactId));

            if (SprintWeekEndDate != null)
            {
                query = query.Where(x =>
                    x.Task.ProjectWeeklyPlanDatesReqTaskIssueMappingList.Any(m =>
                        !m.Deleted &&
                        m.TaskId == x.TaskId &&
                        m.ProjectWeeklyPlanDates.WeekDate.Date == SprintWeekEndDate.Value.Date &&
                        m.ProjectWeeklyPlanDates.PlanType.DropDownValue.ToLower() == "weekly"
                    ));
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Task.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProjectModule.Name.ToLower().Contains(SearchText.ToLower()) ||
                    (m.AssignedTo.Person.FirstName + " " + m.AssignedTo.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ActivityStatus.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.Task.Status.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.EstimateHours.ToString().ToLower().Contains(SearchText.ToLower()) ||
                    //m.TargetMonth.ToString().ToLower().Contains(SearchText.ToLower()) ||
                    m.StartDate.Value.Date == parsedDate.Date ||
                    m.EndDate.Value.Date == parsedDate.Date
                );
            }

            query = GetSorted(query, sortBy, descending);

            // Apply multi-level dictionary sorting
            if (sorts != null && sorts.Count > 0)
            {
                query = _commonService.ApplySorting(query, sorts);
            }

            // Project only necessary fields
            query = query.Select(x => new ProjectActivity
            {

                Id = x.Id,
                Name = x.Name,
                DueDate = x.DueDate,
                EstimateHours = x.EstimateHours,
                AssignedToId = x.AssignedToId,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                IsDescription = IsDescriptionEmpty(x.Description),
                Active = x.Active,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    ProjectId = x.Task.ProjectId,
                    ProjectModuleId = x.Task.ProjectModuleId,
                    Status = new DropDown
                    {
                        Id = x.Task.Status.Id,
                        DropDownValue = x.Task.Status.DropDownValue
                    },
                    ProjectWeeklyPlanDatesReqTaskIssueMappingList = x.Task.ProjectWeeklyPlanDatesReqTaskIssueMappingList.Where(p => !p.Deleted && p.ProjectWeeklyPlanDates.PlanType.DropDownValue.ToLower() == "weekly").Select(p => new ProjectWeeklyPlanDatesReqTaskIssueMapping
                    {
                        Id = p.Id,
                        ProjectWeeklyPlanDates = new ProjectWeeklyPlanDates
                        {
                            Id = p.ProjectWeeklyPlanDates.Id,
                            WeekDate = p.ProjectWeeklyPlanDates.WeekDate
                        }
                    }).ToList()
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    ProjectStatus = new DropDown
                    {
                        Id = x.Project.ProjectStatus.Id,
                        DropDownValue = x.Project.ProjectStatus.DropDownValue,
                    }
                    //ProjectUserMappings = x.Project.ProjectUserMappings
                    //    .Where(m => !m.Deleted && m.ProjectId == x.Project.Id && (isAdmin || m.AspNetUserId == userId))
                    //    .Take(1).Select(m => new ProjectUserMapping
                    //    {
                    //        Id = m.Id,
                    //        FullAccess = m.FullAccess,
                    //        ViewOnly = m.ViewOnly,
                    //        Notes = m.Notes
                    //    }).ToList()
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person

                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                        FullName = x.AssignedTo.Person.FirstName + " " + x.AssignedTo.Person.LastName,
                    }
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue
                },
                ActivitiesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "project Activities").Count()
            });

            var list = await query.ToListAsync();


            // Group backend data by project (return full activity objects)
            var groupedResult = list
                .GroupBy(x => new { x.ProjectId, ProjectName = x.Project?.Name })
                .Select(g => new
                {
                    Project = new
                    {
                        Id = g.Key.ProjectId,
                        Name = g.Key.ProjectName
                    },
                    Activities = g.ToList()
                })
                .AsQueryable();

            return new PagedList<object>(groupedResult, page, pageSize);
        }

        public IPagedList<ProjectActivity> GetAllProjectActivitiesForDashboard(string SiteId, 
            string projectId,
            string sortBy, 
            bool descending, 
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            // Initial query
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.ProjectId == projectId);

            query = GetSorted(query, sortBy, descending);
            // Project only necessary fields
            query = query.Select(x => new ProjectActivity
            {

                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                DueDate = x.DueDate,
                EstimateHours = x.EstimateHours,
                AssignedToId = x.AssignedToId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                //SiteId = x.SiteId,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    ProjectId = x.Task.ProjectId,
                    ProjectModuleId = x.Task.ProjectModuleId,
                    PriorityId = x.Task.PriorityId,
                    StartDate = x.Task.StartDate,
                    EndDate = x.Task.EndDate,
                    EstimateTime = x.Task.EstimateTime,
                    Description = x.Task.Description,
                    Status = new DropDown
                    {
                        Id = x.Task.Status.Id,
                        DropDownValue = x.Task.Status.DropDownValue
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person

                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                        FullName = x.AssignedTo.Person.FirstName + " " + x.AssignedTo.Person.LastName,
                    }
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue
                },
            });
            // Create a PagedList using the fetched results
            var pagedList = new PagedList<ProjectActivity>(query, page, pageSize);
            return pagedList;
        }

        // Helper to parse the target month string
        private DateTime ParseTargetMonth(string targetMonthStr)
        {
            if (!string.IsNullOrWhiteSpace(targetMonthStr))
            {
                var parts = targetMonthStr.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out var year) && DateTime.TryParseExact(parts[0], "MMMM", CultureInfo.CurrentCulture, DateTimeStyles.None, out var monthDate))
                {
                    return new DateTime(year, monthDate.Month, 1);
                }
            }
            return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1); // Default to current month
        }

        // Helper to parse the target month string
        private IQueryable<ProjectActivity> GetSorted(IQueryable<ProjectActivity> query, string sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                // Default sorting if no sortBy is provided
                return query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Task.Name).ThenByDescending(x => x.EstimateHours);
            }

            // Dynamic sorting based on sortBy value
            switch (sortBy)
            {
                case "project.name":
                    query = descending
                        ? query.OrderByDescending(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Task.Name)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Task.Name);
                    break;

                case "projectModule.name":
                    query = descending
                        ? query.OrderByDescending(x => x.ProjectModule.Name).ThenByDescending(x => x.Task.Name)
                        : query.OrderBy(x => x.ProjectModule.Name).ThenByDescending(x => x.Task.Name);
                    break;

                case "task.name":
                    query = descending
                        ? query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenByDescending(x => x.Task.Name)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Task.Name);
                    break;

                case "task.status.dropDownValue":
                    query = descending
                        ? query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenByDescending(x => x.Task.Status.DropDownValue)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Task.Status.DropDownValue);
                    break;

                case "name":
                    query = descending
                        ? query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenByDescending(x => x.Name)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Name);
                    break;

                case "assignedTo.person.firstname":
                    query = descending
                        ? query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenByDescending(x => x.AssignedTo.Person.FirstName)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.AssignedTo.Person.FirstName);
                    break;

                case "activityStatus.dropDownValue":
                    query = descending
                        ? query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenByDescending(x => x.ActivityStatus.DropDownValue)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.ActivityStatus.DropDownValue);
                    break;

                case "estimateHours":
                    query = descending
                        ? query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenByDescending(x => x.EstimateHours)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.EstimateHours);
                    break;

                case "startDate":
                    query = descending
                        ? query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenByDescending(x => x.StartDate)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.StartDate);
                    break;

                case "endDate":
                    query = descending
                        ? query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenByDescending(x => x.EndDate)
                        : query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.EndDate);
                    break;

                default:
                    // Fallback to default sorting
                    query = query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Task.Name).ThenByDescending(x => x.EstimateHours);
                    break;
            }

            return query;
        }
        #endregion



        #region GetProjectActivityById
        // Title: GetProjectActivityById
        // Description: This method retrieves a project activity from the database by its unique identifier (`id`). 
        public async Task<ProjectActivity> GetById(string id)
        {
            // Fetch the first matching record asynchronously
            return await _projectActivityRepository.Table
                .AsNoTracking() // Ensure no tracking for better performance on read-only operations
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);
        }
        #endregion

        #region GetAllProjectActivityListForDropdown
        public async Task<List<ProjectActivity>> GetAllProjectActivityListForDropdown(string SiteId)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId);
            query = query.Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
            });
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetProjectActivityDetailsById
        // Title: GetProjectActivityDetailsById
        // Description: The method selects relevant fields from the project activity entity.
        public async Task<ProjectActivity> GetProjectActivityDetailsById(string id)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                ActivityStatusId = x.ActivityStatusId,
                Description = x.Description,
                AssignedToId = x.AssignedToId,
                TaskId = x.TaskId,
                DueDate = x.DueDate,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                EstimateHours = x.EstimateHours,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                //SiteId = x.SiteId,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    ProjectId = x.Task.ProjectId,
                    ProjectModuleId = x.Task.ProjectModuleId,
                    PriorityId = x.Task.PriorityId,
                    StartDate = x.Task.StartDate,
                    EndDate = x.Task.EndDate,
                    EstimateTime = x.Task.EstimateTime,
                    Description = x.Task.Description,
                    Status = new DropDown
                    {
                        Id = x.Task.Status.Id,
                        DropDownValue = x.Task.Status.DropDownValue
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person

                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                    }
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue

                },
                CreatedByUser = new ApplicationUser
                {
                    Id = x.CreatedByUser.Id,
                    Person = new Person
                    {
                        FullName = x.CreatedByUser.Person.FirstName + " " + x.CreatedByUser.Person.LastName
                    }
                },
                UpdatedByUser = new ApplicationUser
                {
                    Id = x.UpdatedByUser.Id,
                    Person = new Person
                    {
                        FullName = x.UpdatedByUser.Person.FirstName + " " + x.UpdatedByUser.Person.LastName
                    }
                },
                ProjectTaskActivityFilesList = x.ProjectTaskActivityFilesList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectActivityFiles
                {
                    Id = mapping.Id,
                    CreatedBy = new ApplicationUser
                    {
                        Id = mapping.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = mapping.CreatedBy.Person.Id,
                            FirstName = mapping.CreatedBy.Person.FirstName,
                            LastName = mapping.CreatedBy.Person.LastName,
                        }
                    },
                    CreatedOnUtc = mapping.CreatedOnUtc,
                    File = new Picture
                    {
                        Id = mapping.File.Id,
                        VirtualPath = mapping.File.VirtualPath,
                        MimeType = mapping.File.MimeType,
                        SeoFilename = mapping.File.SeoFilename
                    }
                }).ToList()
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectActivityByTaskId
        // Title: GetProjectActivityByTaskId
        // Description: The method selects relevant fields from the project activity entity.
        public async Task<List<ProjectActivity>> GetProjectActivityByTaskId(string SiteId, string taskId, DateTime? TargetMonth = null)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId && x.TaskId == taskId && x.ActivityStatus.DropDownValue.ToLower() != "close").Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                ActivityStatusId = x.ActivityStatusId,
                Description = x.Description,
                AssignedToId = x.AssignedToId,
                TaskId = x.TaskId,
                DueDate = x.DueDate,
                //StartDate = x.StartDate,
                //EndDate = x.EndDate,
                EstimateHours = x.EstimateHours,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                Task = new ProjectTask
                {
                    Id = x.Task.Id
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person
                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                    }
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue
                }
            });
            var list = await query.OrderByDescending(x => x.CreatedOnUtc).ToListAsync();
            return list;
        }
        #endregion

        #region GetProjectActivityById
        // Title: GetProjectActivityById
        // Description: The method used for send weekly plan resources estimated hrs
        public async Task<ProjectActivity> GetProjectActivityById(string EmployeeId)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.AssignedToId == EmployeeId).Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                TaskId = x.TaskId,
                AssignedToId = x.AssignedToId,
                EstimateHours = x.EstimateHours,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person
                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                    }
                }
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectActivityByName
        // Title: GetProjectActivityByName
        // Description: This method retrieves a project activity based. It allows an optional exclusion of a project activity by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific project activity. The method returns the first matching project activity or null if no match is found.
        public async Task<ProjectActivity> GetProjectActivityByName(string SiteId, string name, string taskId, string id = null)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower() && x.TaskId == taskId && x.SiteId == SiteId);

            query = query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                ActivityStatusId = x.ActivityStatusId,
                Description = x.Description,
                AssignedToId = x.AssignedToId,
                TaskId = x.TaskId,
                DueDate = x.DueDate,
                EstimateHours = x.EstimateHours,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                //SiteId = SiteId,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    Status = new DropDown
                    {
                        Id = x.Task.Status.Id,
                        DropDownValue = x.Task.Status.DropDownValue
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person

                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                    }
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue
                },
            });
            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectActivityByDetails
        // Title: GetProjectActivityByDetails
        // Description: This method retrieves a project activity based. It allows an optional exclusion of a project activity by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific project activity. The method returns the first matching project activity or null if no match is found.
        public async Task<ProjectActivity> GetProjectActivityByDetails(string name, string taskId, string AssignedToId, DateTime? TargetMonth, string id = null)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower() && x.TaskId == taskId && x.AssignedToId == AssignedToId
            && x.ActivityStatus.DropDownValue.ToLower() != "close");

            query = query.Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                ActivityStatusId = x.ActivityStatusId,
                Description = x.Description,
                AssignedToId = x.AssignedToId,
                TaskId = x.TaskId,
                DueDate = x.DueDate,
                EstimateHours = x.EstimateHours,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                //SiteId = x.SiteId,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    Status = new DropDown
                    {
                        Id = x.Task.Status.Id,
                        DropDownValue = x.Task.Status.DropDownValue
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person

                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                    }
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue
                },
            });

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetProjectTaskActivityListForDropdown
        public async Task<List<ProjectActivity>> GetProjectTaskActivityListForDropdown(
            string siteId,
            string projectId = null,
            string projectModuleId = null,
            string taskId = null,
            string employeeId = null
        )
        {
            var query = _projectActivityRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == siteId && m.ActivityStatus.DropDownValue.ToLower() == "open");

            if (!string.IsNullOrEmpty(employeeId))
                query = query.Where(m => m.AssignedToId == employeeId);

            if (!string.IsNullOrEmpty(projectId))
                query = query.Where(m => m.ProjectId == projectId);

            if (!string.IsNullOrEmpty(projectModuleId))
                query = query.Where(m => m.ProjectModuleId == projectModuleId);

            if (!string.IsNullOrEmpty(taskId))
                query = query.Where(m => m.TaskId == taskId);

            var list = await query.Where(x => x.Active).OrderBy(x => x.Name).Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
                DisplayText = x.Name + " (" + x.EstimateHours.ToString("0.##") + ")",
                AssignedTo = new Employee
                {
                    Person = new Person
                    {
                        FullName = $"({x.AssignedTo.Person.FirstName} {x.AssignedTo.Person.LastName})"
                    }
                }
            }).ToListAsync();

            return list;
        }
        #endregion

        #region InsertProjectActivity
        // Title: InsertProjectActivity
        // Description: This method inserts a new Project activity entity into the repository. It takes a Project activity object as input and uses the _projectActivityRepository to handle the insertion operation.
        public void InsertProjectActivity(ProjectActivity entity)
        {
            _projectActivityRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectActivity
        // Title: UpdateProjectActivity
        // Description: This method updates the specified Project activity entity in the repository. It takes a Project activity object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProjectActivity(ProjectActivity entity)
        {
            _projectActivityRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectActivity
        // Title: DeleteProjectActivity
        // Description: Marks the specified project activity entity as deleted by setting its `Deleted` property to true. 
        public void DeleteProjectActivity(ProjectActivity entity)
        {
            entity.Deleted = true;

            _projectActivityRepository.Update(entity);
        }
        #endregion

        #region GetProjectTasksActivitiesDetailsByIds
        public async Task<List<ProjectActivity>> GetProjectTasksActivitiesDetailsByIds(string[] ids)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && ids.Any(m => m == x.Id));
            query = query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Task.Name).ThenBy(x => x.Name).Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                EstimateHours = x.EstimateHours,
                CreatedOnUtc = x.CreatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person
                    {
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                    }
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        public void InsertProjectActivityList(IList<ProjectActivity> entities)
        {
            _projectActivityRepository.Insert(entities);
        }

        public void UpdateProjectActivityList(IList<ProjectActivity> entities)
        {
            _projectActivityRepository.Update(entities);
        }

        public async Task<List<ProjectActivity>> GetProjectActivitiesByTaskId(string taskId, string pageName = "", bool isShowCloseStatus = false)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && !x.Task.Deleted && x.TaskId == taskId);

            if (pageName == "PL") // planner page
            {
                if (isShowCloseStatus == false)
                    query = query.Where(x => x.ActivityStatus.DropDownValue != "Close");//Show data without close status
            }

            query = query.OrderByDescending(x => x.CreatedOnUtc);
            query = query.Select(x => new ProjectActivity
            {
                Id = x.Id,
                DueDate = x.DueDate,
                ProjectId = x.ProjectId,
                TaskId = x.TaskId,
                ActivityStatusId = x.ActivityStatusId,
                EstimateHours = x.EstimateHours,
                Name = x.Name,
                Description = x.Description,
                AssignedToId = x.AssignedToId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                //SiteId = x.SiteId,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    Description = x.Task.Description,
                    Status = new DropDown
                    {
                        Id = x.Task.Status.Id,
                        DropDownValue = x.Task.Status.DropDownValue
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person
                    {
                        Id = x.AssignedTo.Person.Id,
                        FullName = x.AssignedTo.Person.FirstName + " " + x.AssignedTo.Person.LastName,
                    },
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue
                },
            });

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<List<ProjectActivity>> GetProjectActivitiesByModuleId(string moduleId)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && !x.ProjectModule.Deleted && x.ProjectModuleId == moduleId);

            query = query.OrderByDescending(x => x.CreatedOnUtc);
            query = query.Select(x => new ProjectActivity
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                AssignedToId = x.AssignedToId,
                TaskId = x.TaskId,
                Name = x.Name,
                Description = x.Description,
                ActivityStatusId = x.ActivityStatusId,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    Status = new DropDown
                    {
                        Id = x.Task.Status.Id,
                        DropDownValue = x.Task.Status.DropDownValue
                    }
                },
                ActivityStatus = new DropDown
                {
                    Id = x.ActivityStatus.Id,
                    DropDownValue = x.ActivityStatus.DropDownValue
                }
            });

            var list = await query.ToListAsync();
            return list;
        }

        #region GetAllProjectActivitiesByModuleIdForMoveModuleAsProject
        public async Task<List<ProjectActivity>> GetAllProjectActivitiesByModuleIdForMoveModuleAsProject(string moduleId)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && !x.ProjectModule.Deleted && x.ProjectModuleId == moduleId);

            query = query.OrderByDescending(x => x.CreatedOnUtc);
            query = query.Select(x => new ProjectActivity
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                SiteId = x.SiteId,
                ProjectModuleId = x.ProjectModuleId,
                TaskId = x.TaskId,
                AssignedToId = x.AssignedToId,
                Name = x.Name,
                Description = x.Description,
                EstimateHours = x.EstimateHours,
                ActivityStatusId = x.ActivityStatusId,
                SortOrder = x.SortOrder,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Active = x.Active
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        public void DeleteProjectActivityList(List<ProjectActivity> activityEntities)
        {
            var activities = new List<ProjectActivity>();
            foreach (var activity in activityEntities)
            {
                activity.Deleted = true;
                activity.Active = false;

                activities.Add(activity);
            }
            _projectActivityRepository.Update(activities);
        }
        //private async Task<bool> IsCurrentUserAdmin(string CId)
        //{
        //    var userdata = await _userManager.FindByIdAsync(CId);
        //    var user = await _userManager.FindByNameAsync(userdata.UserName);
        //    var roles = await _userManager.GetRolesAsync(user);
        //    var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin");

        //    return isAdmin;
        //}

        #region GetProjectActivityForTimerById
        // Title: GetProjectActivityDetailsById
        // Description: The method selects relevant fields from the project activity entity.
        public async Task<ProjectActivity> GetProjectActivityForTimerById(string id)
        {
            var query = _projectActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new ProjectActivity
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                ActivityStatusId = x.ActivityStatusId,
                Description = x.Description,
                AssignedToId = x.AssignedToId,
                TaskId = x.TaskId,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,                    
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectActivityDescriptionById
        // Title: GetProjectActivityDescriptionById
        // Description: This method retrieves a description from the database by its unique identifier (`id`). 
        public async Task<string> GetProjectActivityDescriptionById(string id)
        {
            var description = await _projectActivityRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == id)
                .Select(x => x.Description)
                .FirstOrDefaultAsync();

            return description;
        }
        #endregion


        public static bool IsDescriptionEmpty(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return true;
            string plainText = Regex.Replace(html, "<[^>]*>", "");
            plainText = plainText.Replace("&nbsp;", " ");
            return plainText.Trim() == "";
        }

    }
}
