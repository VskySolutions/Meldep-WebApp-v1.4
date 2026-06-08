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

namespace Vsky.Services.Requirements
{
    public class RequirementGroupService : IRequirementGroupService
    {
        #region Define Services
        private readonly IRepository<RequirementGroup> _requirementGroupRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations

        public RequirementGroupService(
            IRepository<RequirementGroup> requirementGroupRepository, 
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _requirementGroupRepository = requirementGroupRepository;
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

        #region GetAllRequirementGroups
        // Title: GetAllRequirementGroups
        // Description: This method retrieves a paginated list of Requirement Group based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<RequirementGroup>> GetAllRequirementGroups(string SiteId, string LoggedUserId, string SearchText, int requirementGroupNumber, List<string> projectIds, string name, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _requirementGroupRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId && (m.FullAccess || m.ViewOnly || m.Notes)));
            //query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId));

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }

            if (requirementGroupNumber != 0)
                query = query.Where(x => x.RequirementGroupNumber == requirementGroupNumber);

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
                      //m.RequirementGroupNumber.ToString().Contains(SearchText.ToLower()) ||
                      m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                      m.Name.ToLower().Contains(SearchText.ToLower()));
            }
            query = query.Select(x => new RequirementGroup
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                Name = x.Name,
                Description = x.Description,
                RequirementGroupNumber=x.RequirementGroupNumber,
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
                    }).Take(1).ToList()
                }
            });

            var list = new PagedList<RequirementGroup>(query, page, pageSize);
            return list;
        }

        public IPagedList<RequirementGroup> GetAllRequirementGroupsForDashboard(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _requirementGroupRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId && x.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            query = query.Select(x => new RequirementGroup
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                Name = x.Name,
                Description = x.Description,
                RequirementGroupNumber = x.RequirementGroupNumber,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
            });

            var list = new PagedList<RequirementGroup>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetRequirementGroupById
        // Title: GetRequirementGroupById
        // Description: This method retrieves a RequirementGroup from the database by its unique identifier (`id`). 
        public async Task<RequirementGroup> GetRequirementGroupById(string id)
        {
            var query = _requirementGroupRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetRequirementGroupByName
        // Title: GetRequirementGroupByName
        // Description: This method retrieves a Requirement Group based on its name and Id. It allows an optional exclusion of a Requirement Group by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific Requirement Group. The method returns the first matching Requirement Group or null if no match is found.
        public async Task<RequirementGroup> GetRequirementGroupByName(string name, string ProjectId, string id = null)
        {
            var query = _requirementGroupRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower() && x.ProjectId == ProjectId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllRequirementGroupsListForDropdown
        public async Task<List<CommonDropDown>> GetAllRequirementGroupsListForDropdown(string SiteId, string projectId = null)
        {
            var query = _requirementGroupRepository.TableNoTracking.Where(m => !m.Deleted && m.Project.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                var projectIdArray = projectId.Split(',');

                // Filter query based on the array of project IDs
                query = query.Where(x => projectIdArray.Contains(x.ProjectId));
            }

            var list = await query.Select(x => new CommonDropDown
            {
                Value = x.Id,
                Text = x.Name,
            }).ToListAsync();

            return list;
        }
        #endregion

        #region GetRequirementGroupDetailsById
        // Title: GetRequirementGroupDetailsById
        // Description: The method selects relevant fields from the RequirementGroup entity.
        public async Task<RequirementGroup> GetRequirementGroupDetailsById(string id)
        {
            var query = _requirementGroupRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new RequirementGroup
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                Name = x.Name,
                Description = x.Description,
                RequirementGroupNumber = x.RequirementGroupNumber,
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

        #region InsertRequirementGroup
        // Title: InsertRequirementGroup
        // Description: This method inserts a new RequirementGroup entity into the repository. It takes a RequirementGroup object as input and uses the _requirementGroupRepository to handle the insertion operation.
        public void InsertRequirementGroup(RequirementGroup entity)
        {
            _requirementGroupRepository.Insert(entity);
        }
        #endregion

        #region UpdateRequirementGroup
        // Title: UpdateRequirementGroup
        // Description: This method updates the specified RequirementGroup entity in the repository. It takes a RequirementGroup object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateRequirementGroup(RequirementGroup entity)
        {
            _requirementGroupRepository.Update(entity);
        }
        #endregion

        #region DeleteRequirementGroup
        // Title: DeleteRequirementGroup
        // Description: Marks the specified RequirementGroup entity as deleted by setting its `Deleted` property to true. 
        public void DeleteRequirementGroup(RequirementGroup entity)
        {
            entity.Deleted = true;

            _requirementGroupRepository.Update(entity);
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
