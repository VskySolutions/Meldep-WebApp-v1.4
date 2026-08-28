using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectUserMappings
{
    public class ProjectUserMappingService : IProjectUserMappingService
    {
        #region Define Service
        /// <summary>
        /// Define Service
        /// </summary>
        private readonly IRepository<Project> _projectRepository;

        #endregion

        #region Service Initializations
        /// <summary>
        /// Service Initializations
        /// </summary>
        /// <param name="ProjectUserMappingRepository"></param>
        public ProjectUserMappingService(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Private Methods
        /// </summary>
        /// <param name="orderBy"></param>
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllProjectsForUserPermission
        // Title: GetAllProjectsForUserPermission
        // Description: This method retrieves a paginated list of projects based user role and access
        public async Task<IPagedList<Project>> GetAllProjectsForUserPermission(
            string siteId,
            bool isTemplate,
            string userId,
            string employeeId,
            string searchText,
            List<string> projectIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _projectRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.SiteId == siteId &&
                    x.IsTemplate == isTemplate);

            if (projectIds != null && projectIds.Any())
            {
                query = query.Where(x => projectIds.Contains(x.Id));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(x =>
                    x.Name != null &&
                    x.Name.Contains(searchText));
            }

            query = query.Select(x => new Project
            {
                Id = x.Id,
                Name = x.Name,
                SiteId = x.SiteId,
                ProjectModules = x.ProjectModules
                    .Where(m => !m.Deleted)
                    .Select(module => new ProjectModule
                    {
                        Id = module.Id,
                        Name = module.Name,

                        ProjectModulesUserMappings =
                            module.ProjectModulesUserMappings
                                .Where(m => !m.Deleted)
                                .Select(mapping =>
                                    new ProjectModulesUserMapping
                                    {
                                        Id = mapping.Id,
                                        FullAccess = mapping.FullAccess,
                                        ViewOnly = mapping.ViewOnly,
                                        Notes = mapping.Notes,
                                        User = new ApplicationUser
                                        {
                                            Id = mapping.User.Id,
                                            Person = new Person
                                            {
                                                Id = mapping.User.Person.Id,
                                                FullName = mapping.User.Person.FirstName + " " + mapping.User.Person.LastName,
                                            },
                                        }
                                    })
                                .ToList()
                    })
                    .ToList()
            });

            var list = new PagedList<Project>(
                query,
                page,
                pageSize);

            return list;
        }
        #endregion
    }
}
