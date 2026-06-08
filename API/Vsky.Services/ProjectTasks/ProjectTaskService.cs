using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;
using Vsky.Services.DropDownTypes;
using Vsky.Services.ProjectUserMappings;
using Vsky.Services.Sites;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.ProjectTasks
{
    public class ProjectTaskService : IProjectTaskService
    {
        #region Define Services
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IRepository<Notes> _notesRepository;
        private readonly IRepository<VWProjectTaskStatusSummary> _vWProjectTaskStatusSummary;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        //private readonly IRepository<Timesheet> _timesheetRepository;
        private readonly IRepository<TimesheetLines> _timesheetLinerepository;
        private readonly ICommonService _commonService;
        private readonly IRepository<ProjectUserMapping> _projectUserMappingRepository;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations
        public ProjectTaskService(
            IRepository<ProjectTask> projectTaskRepository,
            IDropDownTypeService dropDownTypeService,
            IRepository<Notes> notesRepository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IRepository<VWProjectTaskStatusSummary> vWProjectTaskStatusSummary,
            IRepository<TimesheetLines> timesheetLinerepository,
            ICommonService commonService,
            IRepository<ProjectUserMapping> projectUserMappingRepository,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _projectTaskRepository = projectTaskRepository;
            _dropDownTypeService = dropDownTypeService;
            _notesRepository = notesRepository;
            _userManager = userManager;
            _db = db;
            _vWProjectTaskStatusSummary = vWProjectTaskStatusSummary;
            _timesheetLinerepository = timesheetLinerepository;
            _commonService = commonService;
            _projectUserMappingRepository = projectUserMappingRepository;
            _applicationUserRoleService = applicationUserRoleService;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetById
        public async Task<ProjectTask> GetById(string id)
        {
            return await _projectTaskRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        public async Task<int> GetLastProjectTaskNumber()
        {
            var query = await _projectTaskRepository.TableNoTracking.OrderByDescending(m => m.ProjectTaskNumber).FirstOrDefaultAsync();
            return query == null ? 1 : query.ProjectTaskNumber;
        }
        public async Task<decimal> GetLastSortOrderOfProjectTasks(string listId)
        {
            var query = await _projectTaskRepository.TableNoTracking.Where(m => !m.Deleted && m.ProjectModuleId == listId).OrderByDescending(m => m.SortOrder).FirstOrDefaultAsync();
            return query == null ? 1.1m : query.SortOrder;
        }
        #endregion

        #region GetAllProjectTasks
        public async Task<IPagedList<ProjectTask>> GetAllProjectTasks(
            string siteId,
            string userId,
            string searchText,
            bool isTemplate,
            int projectTaskNumber,
            List<string> projectIds,
            List<string> moduleIds,
            List<string> projectTaskIds,
            List<string> leadIds,
            List<string> statusIds,
            List<string> priorityIds,
            List<string> customerIds,
            List<string> companyContactIds,
            List<string> activityOwners,
            string taskName,
            List<string> tagIds,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        )
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => x.SiteId == siteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && x.Project.IsTemplate == isTemplate && x.Project.Active && !x.IsMoved);

            bool isAdmin = await IsCurrentUserAdmin(userId, siteId);
            if (!isAdmin)
            {
                query = query.Where(x => x.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == userId && (m.FullAccess || m.ViewOnly || m.Notes)));

                // Check if user exists in ProjectModulesUserMappings
                bool userExistsInModule = query.Any(x => x.ProjectModule.ProjectModulesUserMappings.Any(m => !m.Deleted && m.AspNetUserId == userId));
                if (userExistsInModule)
                {
                    query = query.Where(x => x.ProjectModule.ProjectModulesUserMappings.Any(m => !m.Deleted && m.AspNetUserId == userId && (m.FullAccess || m.ViewOnly || m.Notes)));
                }
            }

            if (projectTaskNumber != 0) query = query.Where(x => x.ProjectTaskNumber == projectTaskNumber);
            if (projectIds?.Any() == true) query = query.Where(x => projectIds.Contains(x.ProjectId));
            if (moduleIds?.Any() == true) query = query.Where(x => moduleIds.Contains(x.ProjectModuleId));
            if (projectTaskIds?.Any() == true) query = query.Where(x => projectTaskIds.Contains(x.Id));
            if (leadIds?.Any() == true) query = query.Where(x => x.Project.ProjectEmployeeMappings.Any(m => leadIds.Contains(m.EmployeeId) && !m.Deleted && m.EmployeeRoleDropdown.DropDownValue == "Project Lead"));
            if (statusIds?.Any() == true) query = query.Where(x => statusIds.Contains(x.StatusId));
            if (priorityIds?.Any() == true) query = query.Where(x => priorityIds.Contains(x.PriorityId));
            if (tagIds?.Any() == true) query = query.Where(x => x.ProjectTask_Tags.Any(t => !t.Deleted && t.AspNetUserId == userId && tagIds.Contains(t.Tags.Id)));
            if (customerIds?.Any() == true) query = query.Where(x => customerIds.Contains(x.Project.CustomerId));
            if (companyContactIds?.Any() == true) query = query.Where(x => companyContactIds.Contains(x.Project.CompanyContactId));
            if (activityOwners?.Any() == true) query = query.Where(x => x.ProjectActivities.Any(a => activityOwners.Contains(a.AssignedToId)));

            if (!string.IsNullOrWhiteSpace(taskName))
            {
                taskName = taskName.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(taskName));
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                DateTime.TryParse(searchText, out var parsedDate);
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    m.ProjectTaskNumber.ToString().Contains(searchText) ||
                    m.Project.Name.ToLower().Contains(searchText) ||
                    m.ProjectModule.Name.ToLower().Contains(searchText) ||
                    m.EstimateTime.ToString().Contains(searchText) ||
                    m.Name.ToLower().Contains(searchText) ||
                    m.Area.DropDownValue.ToLower().Contains(searchText) ||
                    m.Workspace.DropDownValue.ToLower().Contains(searchText) ||
                    m.Action.DropDownValue.ToLower().Contains(searchText) ||
                    m.Priority.DropDownValue.ToLower().Contains(searchText) ||
                    m.Status.DropDownValue.ToLower().Contains(searchText) ||
                    m.Type.DropDownValue.ToLower().Contains(searchText) ||
                    m.StartDate.HasValue && m.StartDate.Value.Date == parsedDate.Date ||
                    m.EndDate.HasValue && m.EndDate.Value.Date == parsedDate.Date ||
                    m.ProjectActivities.Sum(a => a.EstimateHours).ToString().ToLower().Contains(searchText) ||
                    m.ProjectTask_Tags.Any(t => t.Tags.Name.ToLower().Contains(searchText)));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "calendar")
                {
                    query = query.OrderBy(t => t.Project.Name).ThenByDescending(t => t.CreatedOnUtc);
                }
                else
                {
                    var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                    query = query.OrderBy(orderBy);
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            // Apply multi-level dictionary sorting
            if (sorts != null && sorts.Count > 0)
            {
                query = _commonService.ApplySorting(query, sorts);
            }

            //Get taskIds and fetch total timesheet hours
            var taskIds = query.Select(x => x.Id).ToList();

            var timesheetSums = _timesheetLinerepository.TableNoTracking
            .Where(t => !t.Deleted && taskIds.Contains(t.ProjectTaskId))
            .GroupBy(t => t.ProjectTaskId)
            .Select(g => new
            {
                ProjectTaskId = g.Key,
                TotalHours = g.Sum(t => (decimal?)t.Hours) ?? 0
            }).ToDictionary(g => g.ProjectTaskId, g => g.TotalHours);

            var pagedList = new PagedList<ProjectTask>(query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                PriorityId = x.PriorityId,
                ProjectModuleId = x.ProjectModuleId,
                ProjectTaskNumber = x.ProjectTaskNumber,
                EstimateTime = x.EstimateTime,
                TaskMonth = x.TaskMonth,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Color = x.Color,
                //SiteId = x.SiteId,
                Description = x.Description,
                SortOrder = x.SortOrder,
                AssignedToId = x.AssignedToId,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                TotalTimesheetEstHours = timesheetSums.ContainsKey(x.Id) ? timesheetSums[x.Id] : 0,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    StartDate = x.Project.StartDate,
                    GoLiveDate = x.Project.GoLiveDate,
                    //SiteId = x.Project.SiteId,
                    IsTemplate = x.Project.IsTemplate,
                    ProjectStatus = new DropDown
                    {
                        Id = x.Project.ProjectStatus.Id,
                        DropDownValue = x.Project.ProjectStatus.DropDownValue,
                    },
                    ProjectNotesCount = _notesRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == x.Project.Id && m.Type == "Projects"),
                    ProjectUserMappings = x.Project.ProjectUserMappings
                        .Where(m => !m.Deleted && m.ProjectId == x.Project.Id && (isAdmin || m.AspNetUserId == userId))
                        .Take(1).Select(m => new ProjectUserMapping
                        {
                            Id = m.Id,
                            FullAccess = m.FullAccess,
                            ViewOnly = m.ViewOnly,
                            Notes = m.Notes
                        }).ToList(),
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name,
                    ProjectModulesUserMappings = x.ProjectModule.ProjectModulesUserMappings
                    .Where(m => !m.Deleted && m.ProjectModuleId == x.ProjectModule.Id && (isAdmin || m.AspNetUserId == userId))
                    .Take(1)
                    .Select(m => new ProjectModulesUserMapping
                    {
                        Id = m.Id,
                        FullAccess = m.FullAccess,
                        ViewOnly = m.ViewOnly,
                        Notes = m.Notes
                    }).ToList()
                },
                Status = new DropDown { Id = x.Status.Id, DropDownValue = x.Status.DropDownValue },
                Priority = new DropDown { Id = x.Priority.Id, DropDownValue = x.Priority.DropDownValue },
                Type = new DropDown { Id = x.Type.Id, DropDownValue = x.Type.DropDownValue },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person
                    {
                        Id = x.AssignedTo.Person.Id,
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                        FullName = x.AssignedTo.Person.FirstName + " " + x.AssignedTo.Person.LastName,
                        BgColor = x.AssignedTo.Person.BgColor,
                        Color = x.AssignedTo.Person.Color
                    }
                },
                Area = new DropDown
                {
                    Id = x.Area.Id,
                    DropDownValue = x.Area.DropDownValue
                },
                Workspace = new DropDown
                {
                    Id = x.Workspace.Id,
                    DropDownValue = x.Workspace.DropDownValue
                },
                Action = new DropDown
                {
                    Id = x.Action.Id,
                    DropDownValue = x.Action.DropDownValue
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FirstName = x.UpdatedBy.Person.FirstName,
                        LastName = x.UpdatedBy.Person.LastName,
                    }
                },
                ProjectTask_Tags = x.ProjectTask_Tags.Where(t => !t.Deleted && !t.Tags.Deleted && t.AspNetUserId == userId).OrderBy(t => t.Tags.Name).Select(t => new ProjectTask_Tags
                {
                    Id = t.Id,
                    TagId = t.TagId,
                    TaskId = t.TaskId,
                    Tags = new Tags { Id = t.Tags.Id, Name = t.Tags.Name, Color = t.Tags.Color, BgColor = t.Tags.BgColor }
                }).ToList(),
                ProjectActivities = x.ProjectActivities.Where(a => !a.Deleted && a.ActivityStatus.DropDownValue.ToLower() != "Close")
                    .Select(a => new ProjectActivity
                    {
                        Id = a.Id,
                        Name = a.Name,
                        TargetMonth = a.TargetMonth,
                        EstimateHours = a.EstimateHours,
                        Project = new Project { Id = a.Project.Id, Name = a.Project.Name },
                        ProjectModule = new ProjectModule { Id = a.ProjectModule.Id, Name = a.ProjectModule.Name },
                        AssignedTo = new Employee
                        {
                            Id = a.AssignedTo.Id,
                            Person = new Person
                            {
                                Id = a.AssignedTo.Person.Id,
                                FirstName = a.AssignedTo.Person.FirstName,
                                LastName = a.AssignedTo.Person.LastName,
                                FullName = a.AssignedTo.Person.FirstName + " " + a.AssignedTo.Person.LastName,
                                PrimaryEmailAddress = a.AssignedTo.Person.PrimaryEmailAddress,
                                BgColor = a.AssignedTo.Person.BgColor,
                                Color = a.AssignedTo.Person.Color,
                            }
                        }
                    }).ToList(),
                ProjectTaskRelatedMappings = x.ProjectTaskRelatedMappings.Where(m => m.TaskId == x.Id && !m.Deleted && (m.RequirementId != null || m.IssueId != null))
                .Select(m => new ProjectTaskRelatedMapping
                {
                    Id = m.Id,
                    RequirementId = m.RequirementId,
                    IssueId = m.IssueId,
                    Issue = m.Issue == null ? null : new Issue
                    {
                        IssueNumber = m.Issue.IssueNumber,
                        Status = m.Issue.Status == null ? null : new DropDown { Id = m.Issue.Status.Id, DropDownValue = m.Issue.Status.DropDownValue }
                    },
                    Requirement = m.Requirement == null ? null : new Requirement
                    {
                        RequirementNumber = m.Requirement.RequirementNumber,
                        Status = m.Requirement.Status == null ? null : new DropDown { Id = m.Requirement.Status.Id, DropDownValue = m.Requirement.Status.DropDownValue }
                    }
                }).ToList(),
                ProjectTaskNotesCount = _notesRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Project Task")

            }), page, pageSize);

            return pagedList;
        }

        public List<VWProjectTaskStatusSummary> GetTaskStatusSummaryByProjectIds(List<string> projectIds)
        {
            var list = _vWProjectTaskStatusSummary.TableNoTracking.Where(x => projectIds.Contains(x.ProjectId)).ToList();
            return list;
        }

        public IPagedList<ProjectTask> GetAllProjectTasksForDashboard(string SiteId,
            string projectId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        )
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => x.SiteId == SiteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && !x.Project.IsTemplate && x.Project.Active && !x.IsMoved && x.ProjectId == projectId);

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                PriorityId = x.PriorityId,
                ProjectModuleId = x.ProjectModuleId,
                ProjectTaskNumber = x.ProjectTaskNumber,
                EstimateTime = x.EstimateTime,
                TaskMonth = x.TaskMonth,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                //SiteId = x.SiteId,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    ProjectStatus = new DropDown
                    {
                        Id = x.Project.ProjectStatus.Id,
                        DropDownValue = x.Project.ProjectStatus.DropDownValue,
                    }
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                Description = x.Description,
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                },
                ProjectActivities = x.ProjectActivities.Select(p => new ProjectActivity
                {
                    Id = p.Id,
                    Name = p.Name,
                    EstimateHours = p.EstimateHours,
                    Project = new Project
                    {
                        Id = p.Project.Id,
                        Name = p.Project.Name
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = p.ProjectModule.Id,
                        Name = p.ProjectModule.Name
                    }
                }).ToList()
            });
            var list = new PagedList<ProjectTask>(query, page, pageSize);
            return list;
        }

        public IPagedList<ProjectTask> GetAllProjectTaskDetailsList(string SiteId,
            List<string> projectIds,
            string sortByFilterId,
            string taskName,
            List<string> activityOwners,
            List<string> customerIds,
            List<string> companyContactIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        )
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => x.SiteId == SiteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && !x.Project.IsTemplate && x.Project.Active && !x.IsMoved);

            // project filter
            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (activityOwners != null && activityOwners.Any())
                query = query.Where(x => x.ProjectActivities.Any(x => !x.Deleted && activityOwners.Contains(x.AssignedToId)));

            if (!string.IsNullOrWhiteSpace(taskName))
            {
                taskName = taskName.Trim().ToLower(); // Normalize input
                query = query.Where(x => x.Name.ToLower().Contains(taskName));  // Partial match for the name
            }

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.Project.CustomerId));

            if (companyContactIds != null && companyContactIds.Any())
                query = query.Where(x => companyContactIds.Contains(x.Project.CompanyContactId));

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                PriorityId = x.PriorityId,
                ProjectModuleId = x.ProjectModuleId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                //SiteId = x.SiteId,
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
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                ProjectActivities = x.ProjectActivities.Select(p => new ProjectActivity
                {
                    Id = p.Id,
                    Name = p.Name,
                    EstimateHours = p.EstimateHours,
                    AssignedTo = new Employee
                    {
                        Id = p.AssignedTo.Id,
                        Person = new Person
                        {
                            Id = p.AssignedTo.Person.Id,
                            FullName = p.AssignedTo.Person.FirstName + " " + p.AssignedTo.Person.LastName
                        }
                    }
                }).ToList()
            });
            var list = new PagedList<ProjectTask>(query, page, pageSize);
            return list;
        }

        public IPagedList<ProjectTask> GetAllHighPrioritiesTaskForDashboard(string SiteId,
            string activityOwnerId,
            string highPriorityId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        )
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => x.SiteId == SiteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && !x.Project.IsTemplate && x.Project.Active && !x.IsMoved && x.PriorityId == highPriorityId);
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedOnUtc);
            }
            query = query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectTaskNumber = x.ProjectTaskNumber,
                EstimateTime = x.EstimateTime,
                TaskMonth = x.TaskMonth,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                //SiteId = x.SiteId,
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
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                },
                ProjectActivities = x.ProjectActivities.Where(p => !p.Deleted).Select(p => new ProjectActivity
                {
                    Id = p.Id,
                    Name = p.Name,
                    EstimateHours = p.EstimateHours
                }).ToList()
            });
            var list = new PagedList<ProjectTask>(query, page, pageSize);
            return list;
        }

        public IPagedList<ProjectTask> GetProjectTaskForCopy(string SiteId, string projectTaskId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue)
        {
            var query = _projectTaskRepository.TableNoTracking;
            //global filter
            query = query.Where(x => x.SiteId == SiteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && !x.Project.IsTemplate && x.Project.Active && !x.IsMoved && x.Id == projectTaskId);

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                ProjectTaskNumber = x.ProjectTaskNumber,
                TaskMonth = x.TaskMonth,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                //SiteId = x.SiteId,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                }
            });
            var list = new PagedList<ProjectTask>(query, page, pageSize);
            return list;
        }

        public async Task<IPagedList<ProjectTask>> GetAllProjectTasksForNotes(
            string siteId,
            string userId,
            string searchText,
            bool isShowCloseStatus,
            int projectTaskNumber,
            List<string> projectIds,
            List<string> projectModuleIds,
            List<string> projectLeadsIds,
            List<string> statusIds,
            List<string> priorityIds,
            List<string> customerIds,
            List<string> companyContactIds,
            List<string> activityOwners,
            string taskName,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        )
        {
            //var includedTaskStatuses = new[] { "Open", "In Development" };

            var query = _projectTaskRepository.TableNoTracking.Where(x => x.SiteId == siteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && !x.Project.IsTemplate && x.Project.Active && !x.IsMoved);

            if (projectTaskNumber != 0) query = query.Where(x => x.ProjectTaskNumber == projectTaskNumber);
            if (projectIds?.Any() == true) query = query.Where(x => projectIds.Contains(x.ProjectId));
            if (projectModuleIds?.Any() == true) query = query.Where(x => projectModuleIds.Contains(x.ProjectModuleId));
            if (projectLeadsIds?.Any() == true) query = query.Where(x => x.Project.ProjectEmployeeMappings.Any(m => projectLeadsIds.Contains(m.EmployeeId) && m.EmployeeRoleDropdown.DropDownValue == "Project Lead"));
            //if (statusIds?.Any() == true) query = query.Where(x => statusIds.Contains(x.StatusId));
            if (priorityIds?.Any() == true) query = query.Where(x => priorityIds.Contains(x.PriorityId));
            if (customerIds?.Any() == true) query = query.Where(x => customerIds.Contains(x.Project.CustomerId));
            if (companyContactIds?.Any() == true) query = query.Where(x => companyContactIds.Contains(x.Project.CompanyContactId));
            if (activityOwners?.Any() == true) query = query.Where(x => x.ProjectActivities.Any(a => activityOwners.Contains(a.AssignedToId)));

            if (statusIds != null && statusIds.Any())
            {
                query = query.Where(x => statusIds.Contains(x.StatusId));
            }
            else if (!isShowCloseStatus)
            {
                query = query.Where(x => x.Status.DropDownValue != "Close");
            }

            if (!string.IsNullOrWhiteSpace(taskName))
            {
                taskName = taskName.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(taskName));
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    m.Name.ToLower().Contains(searchText) ||
                    m.Status.DropDownValue.ToLower().Contains(searchText)
                    );
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            var pagedList = new PagedList<ProjectTask>(query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectId = x.ProjectId,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectTaskNotesCount = _notesRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Project Task")

            }), page, pageSize);

            return pagedList;
        }
        #endregion

        #region GetAllProjectTaskListForDropdown
        public async Task<List<ProjectTask>> GetAllProjectTaskListForDropdown(string SiteId,
            string projectId = null,
            string projectModuleId = null,
            string employeeId = null
        )
        {
            var query = _projectTaskRepository.Table
                .Include(x => x.Status)
                .Where(x => x.SiteId == SiteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && !x.Project.IsTemplate && x.Project.Active && !x.IsMoved);

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                query = query.Where(x => x.ProjectId == projectId && !x.Project.Deleted);
            }

            if (!string.IsNullOrWhiteSpace(projectModuleId) && projectModuleId != "undefined")
            {
                query = query.Where(x => x.ProjectModuleId == projectModuleId && !x.ProjectModule.Deleted);
            }

            // Exclude close status
            query = query.Where(x => x.Status.DropDownValue != "Close");

            // Get current month
            var now = DateTime.UtcNow;
            int currentMonth = now.Month;
            int currentYear = now.Year;

            query = query.OrderBy(x => x.Name).Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name + " (" + x.ProjectActivities.Count(m =>
                    !m.Deleted &&
                    !m.Project.Deleted &&
                    !m.ProjectModule.Deleted &&
                    !m.Task.Deleted &&
                    !m.ProjectModule.IsMoved &&
                    !m.Task.IsMoved &&
                    m.Active &&
                    m.Project.Active &&
                    m.AssignedToId == employeeId &&
                    m.ActivityStatus.DropDownValue.ToLower() == "open"
                ) + ")"
            });

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<List<CommonDropDown>> GetAllProjectMultiTaskListForDropdown(string siteId, bool isTemplate, string projectId = null, string projectModuleId = null)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => x.SiteId == siteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && x.Project.IsTemplate == isTemplate && x.Project.Active && !x.IsMoved);

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                var projectIdArray = projectId.Split(',');
                query = query.Where(x => projectIdArray.Contains(x.ProjectId) && !x.Project.Deleted);
            }

            if (!string.IsNullOrWhiteSpace(projectModuleId))
            {
                var projectModuleIdArray = projectModuleId.Split(',');
                query = query.Where(x => projectModuleIdArray.Contains(x.ProjectModuleId) && !x.ProjectModule.Deleted);
            }

            var list = await query
                 .OrderBy(x => x.Name.Replace(" ", ""))
                 .Select(x => new CommonDropDown
                 {
                     Text = x.Name,
                     Value = x.Id,
                 }).ToListAsync();

            return list;
        }

        public async Task<List<CommonDropDown>> GetAllProjectTaskWithProjectListForDropdown(string siteId, string LoggedUserId)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => x.SiteId == siteId && !x.Deleted && !x.Project.Deleted && !x.ProjectModule.Deleted && !x.Project.IsTemplate && x.Project.Active && !x.IsMoved);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, siteId);

            if (!IsAdmin)
            {
                //query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId));
                var allowedProjectIds = await _projectUserMappingRepository.TableNoTracking
                    .Where(x => !x.Deleted && x.AspNetUserId == LoggedUserId)
                    .Select(x => x.ProjectId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(x => allowedProjectIds.Contains(x.ProjectId));
            }

            var list = await query
               .OrderBy(x => x.ProjectModule.Name)
               .Select(x => new CommonDropDown
               {
                   Text = string.Concat(x.ProjectModule.Name, " / ", x.Name, " / ", x.ProjectTaskNumber, " / ", x.Project.Name),
                   Value = x.Id,
               }).ToListAsync();

            return list;
        }
        #endregion

        #region GetProjectTaskDetailsById
        // Title: GetProjectTaskDetailsById
        // Description: The method selects relevant fields from the ProjectTask entity, including related entities such as ProjectTask status, and returns a `ProjectTask` object with these details. 
        public async Task<ProjectTask> GetProjectTaskDetailsById(string id)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectTaskNumber = x.ProjectTaskNumber,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                AreaId = x.AreaId,
                WorkspaceId = x.WorkspaceId,
                ActionId = x.ActionId,
                StatusId = x.StatusId,
                PriorityId = x.PriorityId,
                EstimateTime = x.EstimateTime,
                TaskMonth = x.TaskMonth,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                AssignedToId = x.AssignedToId,
                TypeId = x.TypeId,
                SortOrder = x.SortOrder,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    ProjectStatus = new DropDown
                    {
                        Id = x.Project.ProjectStatus.Id,
                        DropDownValue = x.Project.ProjectStatus.DropDownValue,
                    }
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                Area = new DropDown
                {
                    Id = x.Area.Id,
                    DropDownValue = x.Area.DropDownValue
                },
                Workspace = new DropDown
                {
                    Id = x.Workspace.Id,
                    DropDownValue = x.Workspace.DropDownValue
                },
                Action = new DropDown
                {
                    Id = x.Action.Id,
                    DropDownValue = x.Action.DropDownValue
                },
                Description = x.Description,
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                Type = new DropDown
                {
                    Id = x.Type.Id,
                    DropDownValue = x.Type.DropDownValue
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
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FirstName = x.UpdatedBy.Person.FirstName,
                        LastName = x.UpdatedBy.Person.LastName,
                    }
                },
                ProjectActivities = x.ProjectActivities.Where(x => !x.Deleted && x.ActivityStatus.DropDownValue.ToLower() != "close").OrderByDescending(x => x.CreatedOnUtc).Select(p => new ProjectActivity
                {
                    Id = p.Id,
                    Name = p.Name,
                    AssignedToId = p.AssignedToId,
                    ActivityStatusId = p.ActivityStatusId,
                    EstimateHours = p.EstimateHours,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Description = p.Description,
                    ActivityStatus = new DropDown
                    {
                        Id = p.ActivityStatus.Id,
                        DropDownValue = p.ActivityStatus.DropDownValue
                    },
                    Project = new Project
                    {
                        Id = p.Project.Id,
                        Name = p.Project.Name
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = p.ProjectModule.Id,
                        Name = p.ProjectModule.Name
                    },
                    AssignedTo = new Employee
                    {
                        Id = p.AssignedTo.Id,
                        Person = new Person

                        {
                            FirstName = p.AssignedTo.Person.FirstName,
                            LastName = p.AssignedTo.Person.LastName,
                        }
                    },
                }).ToList(),
                ProjectTaskStatusLog = x.ProjectTaskStatusLog.OrderByDescending(m => m.StatusChangedDate).Select(p => new ProjectTaskStatusLog
                {
                    Id = p.Id,
                    StatusChangedDate = p.StatusChangedDate,
                    Task = new ProjectTask
                    {
                        Id = p.Task.Id,
                        Name = p.Task.Name
                    },
                    Status = new DropDown
                    {
                        Id = p.Status.Id,
                        DropDownValue = p.Status.DropDownValue
                    },
                    StatusChangedByEmployee = new Employee
                    {
                        Person = new Person
                        {
                            Id = p.StatusChangedByEmployee.Person.Id,
                            FullName = p.StatusChangedByEmployee.Person.FirstName + " " + p.StatusChangedByEmployee.Person.LastName
                        }
                    }
                }).ToList(),
                ProjectTaskFilesList = x.ProjectTaskFilesList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectTaskFiles
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
                }).ToList(),
                ProjectTaskRelatedMappings = x.ProjectTaskRelatedMappings.Where(m => m.TaskId == x.Id && !m.Deleted && (m.RequirementId != null || m.IssueId != null))
                .Select(m => new ProjectTaskRelatedMapping
                {
                    Id = m.Id,
                    RequirementId = m.RequirementId,
                    IssueId = m.IssueId,
                    Issue = m.Issue == null ? null : new Issue
                    {
                        IssueNumber = m.Issue.IssueNumber,
                        Status = m.Issue.Status == null ? null : new DropDown { Id = m.Issue.Status.Id, DropDownValue = m.Issue.Status.DropDownValue }
                    },
                    Requirement = m.Requirement == null ? null : new Requirement
                    {
                        RequirementNumber = m.Requirement.RequirementNumber,
                        Status = m.Requirement.Status == null ? null : new DropDown { Id = m.Requirement.Status.Id, DropDownValue = m.Requirement.Status.DropDownValue }
                    }
                }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectTaskDetailsByIds
        public async Task<List<ProjectTask>> GetProjectTaskDetailsByIds(string SiteId, string[] ids)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => !x.Deleted && ids.Any(m => m == x.Id) && x.SiteId == SiteId);

            query = query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectTaskNumber = x.ProjectTaskNumber,
                EstimateTime = x.EstimateTime,
                TaskMonth = x.TaskMonth,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CreatedOnUtc = x.CreatedOnUtc,
                //SiteId = SiteId,
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
                Description = x.Description,
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                    }
                },
                ProjectActivities = x.ProjectActivities.Select(p => new ProjectActivity
                {
                    Id = p.Id,
                    Name = p.Name,
                    Project = new Project
                    {
                        Id = p.Project.Id,
                        Name = p.Project.Name
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = p.ProjectModule.Id,
                        Name = p.ProjectModule.Name
                    }
                }).ToList()
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetProjectTaskByName
        // Title: GetProjectByName
        // Description: This method retrieves a project based on its name and customer ID. It allows an optional exclusion of a project by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific project. The method returns the first matching project or null if no match is found.
        public async Task<ProjectTask> GetProjectTaskByName(string name, string projectId, string projectModuleId, string id = null)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower() && x.ProjectId == projectId && x.ProjectModuleId == projectModuleId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region IsProjectTaskSortOrderExists
        // Title: IsProjectTaskSortOrderExists
        // Description: This method check Project task sort order based on its siteId.
        public async Task<bool> IsProjectTaskSortOrderExists(string siteId, string ProjectId, decimal sortOrder, string moduleId, string id = null)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.ProjectId == ProjectId && x.ProjectModuleId == moduleId && x.SortOrder == sortOrder);
            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            return await query.AnyAsync();
        }
        #endregion

        #region GetTaskByProjectModuleId
        public async Task<List<ProjectTask>> GetTaskByProjectModuleId(string projectModuleId, string pageName = "", bool isShowCloseStatus = false)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModuleId == projectModuleId);
            query = query.OrderByDescending(x => x.CreatedOnUtc);

            if (pageName == "PL") // planner page
            {
                if (isShowCloseStatus == false)
                    query = query.Where(x => x.Status.DropDownValue != "Close");//Show data without close status
            }

            query = query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectModuleId = x.ProjectModuleId,
                EstimateTime = x.EstimateTime,
                TaskMonth = x.TaskMonth,
                StartDate = x.StartDate,
                StatusId = x.StatusId,
                PriorityId = x.PriorityId,
                EndDate = x.EndDate,
                ActivitiesCount = x.ProjectActivities.Where(m => !m.Deleted).Count(),
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
                Description = x.Description,
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                },
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllTaskByProjectModuleIdForMoveModuleAsProject
        public async Task<List<ProjectTask>> GetAllTaskByProjectModuleIdForMoveModuleAsProject(string projectModuleId)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModuleId == projectModuleId);
            query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                SiteId = x.SiteId,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                AreaId = x.AreaId,
                WorkspaceId = x.WorkspaceId,
                EstimateTime = x.EstimateTime,
                StatusId = x.StatusId,
                PriorityId = x.PriorityId,
                AssignedToId = x.AssignedToId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                SortOrder = x.SortOrder,
                Description = x.Description,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion   

        #region GetTimesheetByTaskId
        // Title: GetProjectTaskDetailsById
        // Description: The method selects relevant fields from the ProjectTask entity, including related entities such as ProjectTask status, and returns a `ProjectTask` object with these details. 
        public async Task<ProjectTask> GetTimesheetByTaskId(string id)
        {
            var query = _projectTaskRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                ProjectTaskNumber = x.ProjectTaskNumber,
                EstimateTime = x.EstimateTime,
                TaskMonth = x.TaskMonth,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CreatedOnUtc = x.CreatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedToId,
                    Person = new Person
                    {
                        Id = x.AssignedTo.PersonId,
                        FullName = x.AssignedTo.Person.FullName
                    }
                },
                ProjectActivities = x.ProjectActivities.Select(p => new ProjectActivity
                {
                    Id = p.Id,
                    Name = p.Name,
                    Project = new Project
                    {
                        Id = p.Project.Id,
                        Name = p.Project.Name
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = p.ProjectModule.Id,
                        Name = p.ProjectModule.Name
                    }
                }).ToList()
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectIdAndProjectModuleIdByTaskId
        public async Task<(string ProjectId, string ProjectModuleId)> GetProjectIdAndProjectModuleIdByTaskId(string id)
        {
            var data = await _projectTaskRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == id)
                .Select(x => new
                {
                    ProjectId = x.Project.Id,
                    ProjectModuleId = x.ProjectModule.Id
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return (null, null);

            return (data.ProjectId, data.ProjectModuleId);
        }
        #endregion

        #region GetAllTasksByProjectId
        public IPagedList<ProjectTask> GetAllTasksByProjectId(string SiteId, string projectId, string searchTaskText, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _projectTaskRepository.Table.Where(x => !x.Deleted && x.SiteId == SiteId && x.ProjectId == projectId);

            //sorting
            if (!string.IsNullOrWhiteSpace(searchTaskText))
            {
                searchTaskText = searchTaskText.ToLower();

                query = query.Where(m =>
                    m.Name.ToLower().Contains(searchTaskText));
            }


            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new ProjectTask
            {
                Id = x.Id,
                Name = x.Name,
                EstimateTime = x.EstimateTime,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CreatedOnUtc = x.CreatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                ProjectTaskNotesCount = _notesRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Project Task")
            });

            var list = new PagedList<ProjectTask>(query, page, pageSize);
            return list;
        }
        #endregion

        #region InsertProjectTask
        public void InsertProjectTask(ProjectTask entity)
        {
            _projectTaskRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectTask
        public void UpdateProjectTask(ProjectTask entity)
        {
            _projectTaskRepository.Update(entity);
        }
        #endregion

        #region InsertProjectTaskList
        public void InsertProjectTaskList(IList<ProjectTask> entities)
        {
            _projectTaskRepository.Insert(entities);
        }
        #endregion

        #region UpdateProjectTaskList
        public void UpdateProjectTaskList(IList<ProjectTask> entities)
        {
            _projectTaskRepository.Update(entities);
        }
        #endregion

        #region DeleteProjectTask
        public void DeleteProjectTask(ProjectTask entity)
        {
            entity.Deleted = true;
            entity.Active = false;
            _projectTaskRepository.Update(entity);
        }
        public void DeleteProjectTaskList(List<ProjectTask> entities)
        {
            var records = new List<ProjectTask>();
            foreach (var entity in entities)
            {
                entity.Deleted = true;
                entity.Active = false;
                records.Add(entity);
            }
            _projectTaskRepository.Update(records);
        }
        #endregion

        private IQueryable<ProjectTask> ApplySorting(IQueryable<ProjectTask> query, string sortBy, bool descending)
        {
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "status":
                        query = descending ? query.OrderByDescending(x => x.Status.DropDownValue) : query.OrderBy(x => x.Status.DropDownValue);
                        break;
                    case "priority":
                        query = descending ? query.OrderByDescending(x => x.Priority.DropDownValue) : query.OrderBy(x => x.Priority.DropDownValue);
                        break;
                    case "project":
                        query = descending ? query.OrderByDescending(x => x.Project.Name) : query.OrderBy(x => x.Project.Name);
                        break;
                    default:
                        query = descending ? query.OrderByDescending(x => x.CreatedOnUtc) : query.OrderBy(x => x.CreatedOnUtc);
                        break;
                }
            }
            else
            {
                // Default sorting by StatusId if no sortBy parameter is given
                query = query.OrderBy(x => x.StatusId);
            }

            return query;
        }
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
        private async Task<bool> IsCurrentUserAdmin(string CId, string SiteId)
        {
            var userdata = await _userManager.FindByIdAsync(CId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            //var roles = await _userManager.GetRolesAsync(user);
            var roles = await _applicationUserRoleService.GetRoleNamesByUserAndSite(user.Id, SiteId);
            var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin");

            return isAdmin;
        }
    }
}
