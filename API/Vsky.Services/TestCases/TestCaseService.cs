using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;

namespace Vsky.Services.TestCases
{
    public class TestCaseService : ITestCaseService
    {
        #region Define Services
        private readonly IRepository<TestPlan> _testPlanRepository;
        private readonly IRepository<TestCaseExecutionLog> _testCaseExecutionLogRepository;
        private readonly IRepository<TestCase> _testCaseRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations

        public TestCaseService(
            IRepository<TestPlan> testPlanRepository,
            IRepository<TestCaseExecutionLog> testCaseExecutionLogRepository,
            IRepository<TestCase> testCaseRepository,
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _testPlanRepository = testPlanRepository;
            _testCaseExecutionLogRepository = testCaseExecutionLogRepository;
            _testCaseRepository = testCaseRepository;
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

        #region GetAllTestCases
        // Title: GetAllTestCases
        // Description: This method retrieves a paginated list of Test Case based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<TestCase>> GetAllTestCases(
            string SiteId,
            string LoggedUserId,
            string SearchText,
            int testCaseNumber,
            List<string> projectIds,
            List<string> planIds,
            List<string> testedBys,
            List<string> statusIds,
            string versionNumber,
            DateTime? fromDate,
            DateTime? toDate,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _testCaseRepository.TableNoTracking.Where(x => !x.Deleted && !x.TestPlan.Deleted && x.SiteId == SiteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId && (m.FullAccess || m.ViewOnly || m.Notes)));

            if (testCaseNumber != 0)
                query = query.Where(x => x.TestCaseNumber == testCaseNumber);

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (planIds != null && planIds.Any())
                query = query.Where(x => planIds.Contains(x.PlanId));

            if (testedBys != null && testedBys.Any())
                query = query.Where(x => testedBys.Contains(x.TestedBy));

            if (statusIds != null && statusIds.Any())
            {
                query = query.Where(x =>
                    statusIds.Contains(
                        x.ProjectReleaseTrackingReqPlanTaskIssueMappings
                            .SelectMany(m => m.TestCaseExecutionLog)
                            .OrderByDescending(l => l.CreatedOnUtc)
                            .Select(l => l.StatusId)
                            .FirstOrDefault()
                        ?? x.StatusId
                    ));
            }

            if (!string.IsNullOrWhiteSpace(versionNumber))
            {
                versionNumber = versionNumber.Trim();

                query = query.Where(x =>
                    x.ProjectReleaseTrackingReqPlanTaskIssueMappings
                        .Any(m => m.ReleaseTracking.VersionNumber.Contains(versionNumber)));
            }

            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.CreatedOnUtc >= fromDate);
            if (toDate != null)
                query = query.Where(a => a.CreatedOnUtc <= toDate);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchText = SearchText.Trim().ToLower();
                DateTime.TryParse(SearchText, out var parsedDate);

                query = query.Where(m =>
                    m.TestCaseNumber.ToString().Contains(SearchText) ||
                    m.Project.Name.ToLower().Contains(SearchText) ||
                    m.TestPlan.Name.ToLower().Contains(SearchText) ||
                    m.Name.ToLower().Contains(SearchText) ||
                    (
                        m.ProjectReleaseTrackingReqPlanTaskIssueMappings
                            .SelectMany(x => x.TestCaseExecutionLog)
                            .OrderByDescending(x => x.CreatedOnUtc)
                            .Select(x => x.Status.DropDownValue)
                            .FirstOrDefault()
                        ?? m.Status.DropDownValue
                    ).ToLower().Contains(SearchText) ||

                     m.ProjectReleaseTrackingReqPlanTaskIssueMappings
                        .Any(x => x.ReleaseTracking.VersionNumber.ToLower().Contains(SearchText)) ||

                    (m.TestedByEmployee.Person.FirstName + " " + m.TestedByEmployee.Person.LastName)
                        .ToLower()
                        .Contains(SearchText) ||

                    m.CreatedOnUtc.Date == parsedDate.Date
                );
            }

            query = query.Select(x => new TestCase
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                PlanId = x.PlanId,
                StatusId = x.StatusId,
                TestedBy = x.TestedBy,
                Name = x.Name,
                Description = x.Description,
                Steps = x.Steps,
                ExpectedResult = x.ExpectedResult,
                ActualResult = x.ActualResult,
                TestResult = x.TestResult,
                TestedDate = x.TestedDate,
                AreaId = x.AreaId,
                WorkspaceId = x.WorkspaceId,
                TestCaseNumber=x.TestCaseNumber,
                CreatedOnUtc = x.CreatedOnUtc,

                ProjectReleaseTrackingReqPlanTaskIssueMappingId = x.ProjectReleaseTrackingReqPlanTaskIssueMappings
                .Where(m => !m.Deleted)
                .OrderByDescending(m => m.ReleaseTracking.CreatedOnUtc)
                .Select(m => m.Id)
                .FirstOrDefault(),

                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                TestedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.TestedByEmployee.Person.Id,
                        FullName = x.TestedByEmployee.Person.FirstName + " " + x.TestedByEmployee.Person.LastName,
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    ProjectUserMappings = x.Project.ProjectUserMappings.Where(m => !m.Deleted && m.ProjectId == x.Project.Id && (IsAdmin || m.AspNetUserId == LoggedUserId)).Select(mapping => new ProjectUserMapping
                    {
                        Id = mapping.Id,
                        FullAccess = mapping.FullAccess,
                        ViewOnly = mapping.ViewOnly,
                        Notes = mapping.Notes
                    }).Take(1).ToList(),
                },
                Status = x.ProjectReleaseTrackingReqPlanTaskIssueMappings
                .Where(m => !m.Deleted)
                .OrderByDescending(m => m.ReleaseTracking.CreatedOnUtc)
                .SelectMany(m => m.TestCaseExecutionLog
                    .Where(l => !l.Deleted)
                    .OrderByDescending(l => l.CreatedOnUtc)
                    .Take(1))
                .Select(l => new DropDown
                {
                    Id = l.Status.Id,
                    DropDownValue = l.Status.DropDownValue
                })
                .FirstOrDefault()
                ?? new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                TestPlan = new TestPlan
                {
                    Id = x.TestPlan.Id,
                    Name = x.TestPlan.Name
                },
            });

            var list = new PagedList<TestCase>(query, page, pageSize);
            return list;
        }

        public IPagedList<TestCase> GetAllTestCasesForDashboard(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _testCaseRepository.TableNoTracking.Where(x => !x.Deleted && !x.TestPlan.Deleted && x.SiteId == SiteId && x.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            query = query.OrderByDescending(x => x.UpdatedOnUtc).Select(x => new TestCase
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                PlanId = x.PlanId,
                StatusId = x.StatusId,
                TestedBy = x.TestedBy,
                Name = x.Name,
                Description = x.Description,
                Steps = x.Steps,
                ExpectedResult = x.ExpectedResult,
                ActualResult = x.ActualResult,
                TestResult = x.TestResult,
                TestedDate = x.TestedDate,
                TestCaseNumber = x.TestCaseNumber,
                CreatedOnUtc = x.CreatedOnUtc,
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                TestedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.TestedByEmployee.Person.Id,
                        FullName = x.TestedByEmployee.Person.FirstName + " " + x.TestedByEmployee.Person.LastName,
                    }
                },
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
                TestPlan = new TestPlan
                {
                    Id = x.TestPlan.Id,
                    Name = x.TestPlan.Name
                },
            });

            var list = new PagedList<TestCase>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetTestCaseById
        // Title: GetTestCaseById
        // Description: This method retrieves a TestCase from the database by its unique identifier (`id`). 
        public async Task<TestCase> GetTestCaseById(string id)
        {
            var query = _testCaseRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLastTestCaseNumber
        // Title: GetLastTestCaseNumber
        // Description: This method retrieves the highest TestCaseNumber from the database or returns 1 if none are found. 
        public async Task<int> GetLastTestCaseNumber()
        {
            var query = await _testCaseRepository.TableNoTracking.OrderByDescending(m => m.TestCaseNumber).FirstOrDefaultAsync();
            return query == null ? 1 : query.TestCaseNumber;
        }
        #endregion

        #region GetAllTestCasesListForDropdown
        public async Task<List<TestCase>> GetAllTestCasesListForDropdown(string SiteId)
        {
            var query = _testCaseRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId);
            query = query.Select(x => new TestCase
            {
                Id = x.Id,
                Name = x.Name,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetStatusChangeLog
        public async Task<List<TestCaseStatusChangeLogDto>> GetStatusChangeLog(string mappingId)
        {
            return await _testCaseExecutionLogRepository.TableNoTracking
                .Where(x => x.ProjectReleaseTracking_ReqPlanTaskIssueMappingId == mappingId)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Select(x => new TestCaseStatusChangeLogDto
                {
                    Id = x.Id,
                    Status = x.Status.DropDownValue,
                    Comment = x.Comment,
                    ChangedBy = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName,
                    ChangedDate = x.CreatedOnUtc,
                    IsRemoved = x.Deleted
                })
                .ToListAsync();
        }
        #endregion

        #region GetTestCaseDetailsById
        // Title: GetTestCaseDetailsById
        // Description: The method selects relevant fields from the TestCase entity.
        public async Task<TestCase> GetTestCaseDetailsById(string id)
        {
            var query = _testCaseRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new TestCase
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                AreaId = x.AreaId,
                WorkspaceId = x.WorkspaceId,
                Name = x.Name,
                Description = x.Description,
                Steps = x.Steps,
                ExpectedResult = x.ExpectedResult,
                ActualResult = x.ActualResult,
                TestResult = x.TestResult,
                TestedDate = x.TestedDate,
                TestCaseNumber = x.TestCaseNumber,
                CreatedById = x.CreatedById,
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
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                TestedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.TestedByEmployee.Person.Id,
                        FullName = x.TestedByEmployee.Person.FirstName + " " + x.TestedByEmployee.Person.LastName,
                    }
                },
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
                TestPlan = new TestPlan
                {
                    Id = x.TestPlan.Id,
                    Name = x.TestPlan.Name
                },
                CreatedByUser = new ApplicationUser
                {
                    Id = x.CreatedByUser.Id,
                    Person = new Person
                    {
                        FullName = x.CreatedByUser.Person.FirstName + " " + x.CreatedByUser.Person.LastName
                    }
                },
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTestCase
        // Title: InsertTestCase
        // Description: This method inserts a new TestCase entity into the repository. It takes a TestCase object as input and uses the _testCaseRepository to handle the insertion operation.
        public void InsertTestCase(TestCase entity)
        {
            _testCaseRepository.Insert(entity);
        }
        #endregion

        #region UpdateTestCase
        // Title: UpdateTestCase
        // Description: This method updates the specified TestCase entity in the repository. It takes a TestCase object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTestCase(TestCase entity)
        {
            _testCaseRepository.Update(entity);
        }
        #endregion

        #region DeleteTestCase
        // Title: DeleteTestCase
        // Description: Marks the specified TestCase entity as deleted by setting its `Deleted` property to true. 
        public void DeleteTestCase(TestCase entity)
        {
            entity.Deleted = true;

            _testCaseRepository.Update(entity);
        }
        #endregion
        private async Task<bool> IsCurrentUserAdmin(string CId, string SiteId)
        {
            var userdata = await _userManager.FindByIdAsync(CId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            //var roles = await _userManager.GetRolesAsync(user);
            var roles = await _applicationUserRoleService.GetRoleNamesByUserAndSite(user.Id, SiteId);
            var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin") || roles.Contains("Project Admin");

            return isAdmin;
        }
    }
}
