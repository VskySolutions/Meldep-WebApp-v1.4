using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Sites;

namespace Vsky.Services.Issues
{
    public class IssueService : IIssueService
    {
        #region Define Services
        private readonly IRepository<Issue> _issueRepository;
        private readonly IRepository<Notes> _notesRepository;
        private readonly IRepository<VWProjectIssueStatusSummary> _vwProjectIssueStatusSummary;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations

        public IssueService(IRepository<Issue> issueRepository,
            IRepository<Notes> notesRepository,
            IRepository<VWProjectIssueStatusSummary> vwProjectIssueStatusSummary,
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _issueRepository = issueRepository;
            _notesRepository = notesRepository;
            _vwProjectIssueStatusSummary = vwProjectIssueStatusSummary;
            _userManager = userManager;
            _applicationUserRoleService = applicationUserRoleService;
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

        #region GetAllIssues
        // Title: GetAllIssues
        // Description: This method retrieves a paginated list of issue based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<Issue>> GetAllIssues(
            string SiteId,
            string LoggedUserId,
            string SearchText,
            int issueNumber,
            List<string> projectIds,
            List<string> projectModuleIds,
            string name,
            List<string> priorityIds,
            List<string> statusIds,
            List<string> issueTypeIds,
            List<string> employeeIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
            )
        {
            var query = _issueRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId && (m.FullAccess || m.ViewOnly || m.Notes)));
            //query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId));

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (projectModuleIds != null && projectModuleIds.Any())
                query = query.Where(x => projectModuleIds.Contains(x.ProjectModuleId));

            if (priorityIds != null && priorityIds.Any())
                query = query.Where(x => priorityIds.Contains(x.PriorityId));

            if (statusIds != null && statusIds.Any())
                query = query.Where(x => statusIds.Contains(x.StatusId));

            if (issueTypeIds != null && issueTypeIds.Any())
                query = query.Where(x => issueTypeIds.Contains(x.TypeId));

            if (employeeIds != null && employeeIds.Any())
                query = query.Where(x => employeeIds.Contains(x.EmployeeId));

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower(); // Normalize input
                query = query.Where(x => x.Name.ToLower().Contains(name)); // Partial match for the name
            }

            if (issueNumber != 0)
            {
                query = query.Where(x => x.IssueNumber == issueNumber);
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.IssueNumber.ToString().Contains(SearchText.ToLower()) ||
                    m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProjectModule.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Priority.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.Type.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.Status.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    (m.ReportedBy.Person.FirstName + " " + m.ReportedBy.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.CreatedOnUtc.Date == parsedDate.Date
                );
            }
            query = query.Select(x => new Issue
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                StatusId = x.StatusId,
                PriorityId = x.PriorityId,
                TypeId = x.TypeId,
                ReportedById = x.ReportedById,
                EmployeeId = x.EmployeeId,
                Name = x.Name,
                LastUpdatedDate = x.LastUpdatedDate,
                CloseDate = x.CloseDate,
                DueDate = x.DueDate,
                IsTaskCreated = x.IsTaskCreated,
                IssueNumber = x.IssueNumber,
                CreatedOnUtc = x.CreatedOnUtc,
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Type = new DropDown
                {
                    Id = x.Type.Id,
                    DropDownValue = x.Type.DropDownValue
                },
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                ClosedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ClosedByEmployee.Person.Id,
                        FullName = x.ClosedByEmployee.Person.FirstName + " " + x.ClosedByEmployee.Person.LastName,
                    }
                },
                LastModifiedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.LastModifiedByEmployee.Person.Id,
                        FullName = x.LastModifiedByEmployee.Person.FirstName + " " + x.LastModifiedByEmployee.Person.LastName,
                    }
                },
                ReportedBy = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ReportedBy.Person.Id,
                        FullName = x.ReportedBy.Person.FirstName + " " + x.ReportedBy.Person.LastName,
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    //ProjectUserMappings = x.Project.ProjectUserMappings.Where(m => !m.Deleted && m.AspNetUserId == LoggedUserId && m.ProjectId == x.ProjectId).ToList(),
                    ProjectUserMappings = x.Project.ProjectUserMappings.Where(m => !m.Deleted && m.ProjectId == x.Project.Id && (IsAdmin || m.AspNetUserId == LoggedUserId)).Select(mapping => new ProjectUserMapping
                    {
                        Id = mapping.Id,
                        FullAccess = mapping.FullAccess,
                        ViewOnly = mapping.ViewOnly,
                        Notes = mapping.Notes
                    }).Take(1).ToList(),
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                TestCase = new TestCase
                {
                    Id = x.TestCase.Id,
                    Name = x.TestCase.Name
                },
                ProjectTaskRelatedMappings = x.ProjectTaskRelatedMappings.Where(m => m.IssueId == x.Id && !m.Deleted && m.IssueId != null)
                .Select(m => new ProjectTaskRelatedMapping
                {
                    Id = m.Id,
                    TaskId = m.TaskId,
                    ProjectTask = new ProjectTask
                    {
                        ProjectTaskNumber = m.ProjectTask.ProjectTaskNumber,
                        Status = new DropDown { Id = m.ProjectTask.Status.Id, DropDownValue = m.ProjectTask.Status.DropDownValue }
                    }
                }).ToList(),
                IssueNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Issue").Count(),                
            });

            var list = new PagedList<Issue>(query, page, pageSize);
            return list;
        }

        public List<VWProjectIssueStatusSummary> GetIssueStatusSummaryByProjectIds(List<string> projectIds)
        {
            //if (projectIds == null || !projectIds.Any())
            //    return new List<VWProjectIssueStatusSummary>();

            var list = _vwProjectIssueStatusSummary.TableNoTracking.Where(x => projectIds.Contains(x.ProjectId)).ToList();
            return list;
        }


        public IPagedList<Issue> GetAllIssuesForDashboard(string SiteId, string projectId, string targetMonthStr, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _issueRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            query = query.Select(x => new Issue
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                StatusId = x.StatusId,
                PriorityId = x.PriorityId,
                TypeId = x.TypeId,
                ReportedById = x.ReportedById,
                EmployeeId = x.EmployeeId,
                Name = x.Name,
                LastUpdatedDate = x.LastUpdatedDate,
                CloseDate = x.CloseDate,
                DueDate = x.DueDate,
                IsTaskCreated = x.IsTaskCreated,
                IssueNumber = x.IssueNumber,
                CreatedOnUtc = x.CreatedOnUtc,
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Type = new DropDown
                {
                    Id = x.Type.Id,
                    DropDownValue = x.Type.DropDownValue
                },
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                ClosedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ClosedByEmployee.Person.Id,
                        FullName = x.ClosedByEmployee.Person.FirstName + " " + x.ClosedByEmployee.Person.LastName,
                    }
                },
                LastModifiedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.LastModifiedByEmployee.Person.Id,
                        FullName = x.LastModifiedByEmployee.Person.FirstName + " " + x.LastModifiedByEmployee.Person.LastName,
                    }
                },
                ReportedBy = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ReportedBy.Person.Id,
                        FullName = x.ReportedBy.Person.FirstName + " " + x.ReportedBy.Person.LastName,
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
                TestCase = new TestCase
                {
                    Id = x.TestCase.Id,
                    Name = x.TestCase.Name
                },
            });

            var list = new PagedList<Issue>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetIssueById
        // Title: GetIssueById
        // Description: This method retrieves a issue from the database by its unique identifier (`id`). 
        public async Task<Issue> GetIssueById(string id)
        {
            var query = _issueRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLastIssueNumber
        // Title: GetLastIssueNumber
        // Description: This method retrieves the highest IssueNumber from the database or returns 1 if none are found. 
        public async Task<int> GetLastIssueNumber()
        {
            var query = await _issueRepository.TableNoTracking.OrderByDescending(m => m.IssueNumber).FirstOrDefaultAsync();
            return query == null ? 1 : query.IssueNumber;
        }
        #endregion

        #region GetIssueByName
        // Title: GetIssueByName
        // Description: This method retrieves a Issue based on its name and Id. It allows an optional exclusion of a Issue by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific Issue. The method returns the first matching Issue or null if no match is found.
        public async Task<Issue> GetIssueByName(string siteId, string name, string ProjectId, string id = null)
        {
            var query = _issueRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.Name.ToLower() == name.ToLower() && x.ProjectId == ProjectId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetIssueDetailsById
        // Title: GetIssueDetailsById
        // Description: The method selects relevant fields from the issue entity.
        public async Task<Issue> GetIssueDetailsById(string id)
        {
            var query = _issueRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new Issue
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                LastUpdatedDate = x.LastUpdatedDate,
                CloseDate = x.CloseDate,
                DueDate = x.DueDate,
                IsTaskCreated = x.IsTaskCreated,
                IssueNumber = x.IssueNumber,
                AreaId = x.AreaId,
                WorkspaceId = x.WorkspaceId,
                CreatedOnUtc = x.CreatedOnUtc,
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
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                Type = new DropDown
                {
                    Id = x.Type.Id,
                    DropDownValue = x.Type.DropDownValue
                },
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                ClosedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ClosedByEmployee.Person.Id,
                        FullName = x.ClosedByEmployee.Person.FirstName + " " + x.ClosedByEmployee.Person.LastName,
                    }
                },
                LastModifiedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.LastModifiedByEmployee.Person.Id,
                        FullName = x.LastModifiedByEmployee.Person.FirstName + " " + x.LastModifiedByEmployee.Person.LastName,
                    }
                },
                ReportedBy = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ReportedBy.Person.Id,
                        FullName = x.ReportedBy.Person.FirstName + " " + x.ReportedBy.Person.LastName,
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    StartDate = x.Project.StartDate,
                    GoLiveDate = x.Project.GoLiveDate
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name,
                    StartDate = x.ProjectModule.StartDate,
                    EndDate = x.ProjectModule.EndDate
                },
                TestCase = new TestCase
                {
                    Id = x.TestCase.Id,
                    Name = x.TestCase.Name
                },
                IssueStatusChangedLog = x.IssueStatusChangedLog.OrderByDescending(m => m.StatusChangedDate).Select(p => new IssueStatusChangedLog
                {
                    Id = p.Id,
                    StatusChangedDate = p.StatusChangedDate,
                    Issue = new Issue
                    {
                        Id = p.Issue.Id,
                        Name = p.Issue.Name
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
                ProjectTaskRelatedMappings = x.ProjectTaskRelatedMappings.Where(m => m.IssueId == x.Id && !m.Deleted && m.IssueId != null)
                .Select(m => new ProjectTaskRelatedMapping
                {
                    Id = m.Id,
                    TaskId = m.TaskId,
                    ProjectTask = new ProjectTask
                    {
                        ProjectTaskNumber = m.ProjectTask.ProjectTaskNumber,
                        Status = new DropDown { Id = m.ProjectTask.Status.Id, DropDownValue = m.ProjectTask.Status.DropDownValue }
                    }
                }).ToList(),
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertIssue
        // Title: InsertIssue
        // Description: This method inserts a new Issue entity into the repository. It takes a Issue object as input and uses the _issueRepository to handle the insertion operation.
        public void InsertIssue(Issue entity)
        {
            _issueRepository.Insert(entity);
        }
        #endregion

        #region UpdateIssue
        // Title: UpdateIssue
        // Description: This method updates the specified Issue entity in the repository. It takes a Issue object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateIssue(Issue entity)
        {
            _issueRepository.Update(entity);
        }
        #endregion

        #region DeleteIssue
        // Title: DeleteIssue
        // Description: Marks the specified Issue entity as deleted by setting its `Deleted` property to true. 
        public void DeleteIssue(Issue entity)
        {
            entity.Deleted = true;

            _issueRepository.Update(entity);
        }
        #endregion

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
