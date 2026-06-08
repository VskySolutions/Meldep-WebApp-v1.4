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
using Vsky.Services.Sites;

namespace Vsky.Services.TestPlans
{
    public class TestPlanService : ITestPlanService
    {
        #region Define Services
        private readonly IRepository<TestPlan> _testPlanRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations

        public TestPlanService(
            IRepository<TestPlan> testPlanRepository, 
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _testPlanRepository = testPlanRepository;
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

        #region GetAllTestPlans
        // Title: GetAllTestPlans
        // Description: This method retrieves a paginated list of test plan based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<TestPlan>> GetAllTestPlans(string SiteId, string LoggedUserId, string SearchText, int testPlanNumber, List<string> projectIds, string name, List<string> planMakerIds, List<string> planReviewerIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _testPlanRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId && (m.FullAccess || m.ViewOnly || m.Notes)));

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (planMakerIds != null && planMakerIds.Any())
                query = query.Where(x => planMakerIds.Contains(x.PlanMakerId));

            if (planReviewerIds != null && planReviewerIds.Any())
                query = query.Where(x => planReviewerIds.Contains(x.PlanReviewerId));

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }

            if (testPlanNumber != 0)
                query = query.Where(x => x.TestPlanNumber == testPlanNumber);

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
                query = query.Where(m =>
                    //m.TestPlanNumber.ToString().Contains(SearchText.ToLower()) ||
                    m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Name.ToLower().Contains(SearchText.ToLower()) ||
                    (m.PlanMaker.Person.FirstName + " " + m.PlanMaker.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    (m.PlanReviewer.Person.FirstName + " " + m.PlanReviewer.Person.LastName).ToLower().Contains(SearchText.ToLower())
                );
            }
            query = query.Select(x => new TestPlan
            {
                Id = x.Id,
                PlanMakerId = x.PlanMakerId,
                PlanReviewerId = x.PlanReviewerId,
                ProjectId = x.ProjectId,
                Name = x.Name,
                Description = x.Description,
                TestPlanNumber = x.TestPlanNumber,
                PlanMaker = new Employee
                {
                    Person = new Person
                    {
                        Id = x.PlanMaker.Person.Id,
                        FullName = x.PlanMaker.Person.FirstName + " " + x.PlanMaker.Person.LastName,
                    }
                },
                PlanReviewer = new Employee
                {
                    Person = new Person
                    {
                        Id = x.PlanReviewer.Person.Id,
                        FullName = x.PlanReviewer.Person.FirstName + " " + x.PlanReviewer.Person.LastName,
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
            });

            var list = new PagedList<TestPlan>(query, page, pageSize);

            return list;
        }

        public IPagedList<TestPlan> GetAllTestPlanForDashboard(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _testPlanRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.ProjectId == projectId);
            
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            query = query.OrderByDescending(x => x.UpdatedOnUtc).Select(x => new TestPlan
            {
                Id = x.Id,
                PlanMakerId = x.PlanMakerId,
                PlanReviewerId = x.PlanReviewerId,
                ProjectId = x.ProjectId,
                Name = x.Name,
                Description = x.Description,
                TestPlanNumber = x.TestPlanNumber,
                PlanMaker = new Employee
                {
                    Person = new Person
                    {
                        Id = x.PlanMaker.Person.Id,
                        FullName = x.PlanMaker.Person.FirstName + " " + x.PlanMaker.Person.LastName,
                    }
                },
                PlanReviewer = new Employee
                {
                    Person = new Person
                    {
                        Id = x.PlanReviewer.Person.Id,
                        FullName = x.PlanReviewer.Person.FirstName + " " + x.PlanReviewer.Person.LastName,
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
            });

            var list = new PagedList<TestPlan>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetTestPlanById
        // Title: GetTestPlanById
        // Description: This method retrieves a TestPlan from the database by its unique identifier (`id`). 
        public async Task<TestPlan> GetTestPlanById(string id)
        {
            var query = _testPlanRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLastTestPlanNumber
        // Title: GetLastTestPlanNumber
        // Description: This method retrieves the highest TestPlanNumber from the database or returns 1 if none are found. 
        public async Task<int> GetLastTestPlanNumber()
        {
            var query = await _testPlanRepository.TableNoTracking.OrderByDescending(m => m.TestPlanNumber).FirstOrDefaultAsync();
            return query == null ? 1 : query.TestPlanNumber;
        }
        #endregion

        #region GetTestPlanByName
        // Title: GetTestPlanByName
        // Description: This method retrieves a test plan based on its name and Id. It allows an optional exclusion of a test plan by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific test plan. The method returns the first matching test plan or null if no match is found.
        public async Task<TestPlan> GetTestPlanByName(string SiteId, string name, string ProjectId, string id = null)
        {
            var query = _testPlanRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Name.ToLower() == name.ToLower() && x.ProjectId == ProjectId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetAllTestPlansListForDropdown
        public async Task<List<CommonDropDown>> GetAllTestPlansListForDropdown(string SiteId, string projectId = null)
        {
            var query = _testPlanRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                var projectIdArray = projectId.Split(',');

                // Filter query based on the array of project IDs
                query = query.Where(x => projectIdArray.Contains(x.ProjectId));
            }

            var list = await query
                .OrderBy(x => x.Name)
                .Select(x => new CommonDropDown
            {
                Value = x.Id,
                Text = x.Name,
            }).ToListAsync();

            return list;
        }
        #endregion

        #region GetTestPlanDetailsById
        // Title: GetTestPlanDetailsById
        // Description: The method selects relevant fields from the TestPlan entity.
        public async Task<TestPlan> GetTestPlanDetailsById(string id)
        {
            var query = _testPlanRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new TestPlan
            {
                Id = x.Id,
                PlanMakerId = x.PlanMakerId,
                PlanReviewerId = x.PlanReviewerId,
                ProjectId = x.ProjectId,
                Name = x.Name,
                AreaId = x.AreaId,
                WorkspaceId = x.WorkspaceId,
                Description = x.Description,
                TestPlanNumber = x.TestPlanNumber,
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
                PlanMaker = new Employee
                {
                    Person = new Person
                    {
                        Id = x.PlanMaker.Person.Id,
                        FullName = x.PlanMaker.Person.FirstName + " " + x.PlanMaker.Person.LastName,
                    }
                },
                PlanReviewer = new Employee
                {
                    Person = new Person
                    {
                        Id = x.PlanReviewer.Person.Id,
                        FullName = x.PlanReviewer.Person.FirstName + " " + x.PlanReviewer.Person.LastName,
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },

            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTestPlan
        // Title: InsertTestPlan
        // Description: This method inserts a new TestPlan entity into the repository. It takes a TestPlan object as input and uses the _testPlanRepository to handle the insertion operation.
        public void InsertTestPlan(TestPlan entity)
        {
            _testPlanRepository.Insert(entity);
        }
        #endregion

        #region UpdateTestPlan
        // Title: UpdateTestPlan
        // Description: This method updates the specified TestPlan entity in the repository. It takes a TestPlan object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTestPlan(TestPlan entity)
        {
            _testPlanRepository.Update(entity);
        }
        #endregion

        #region DeleteTestPlan
        // Title: DeleteTestPlan
        // Description: Marks the specified TestPlan entity as deleted by setting its `Deleted` property to true. 
        public void DeleteTestPlan(TestPlan entity)
        {
            entity.Deleted = true;

            _testPlanRepository.Update(entity);
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
