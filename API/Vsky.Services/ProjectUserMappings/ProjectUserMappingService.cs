using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
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

        private readonly IRepository<ProjectUserMapping> _projectUserMappingRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IRepository<Project> _projectRepository;

        #endregion

        #region Service Initializations
        /// <summary>
        /// Service Initializations
        /// </summary>
        /// <param name="ProjectUserMappingRepository"></param>
        public ProjectUserMappingService(IRepository<ProjectUserMapping> projectUserMappingRepository, UserManager<ApplicationUser> userManager,
            ApplicationDbContext db, IRepository<Project> projectRepository)
        {
            _projectUserMappingRepository = projectUserMappingRepository;
            _userManager = userManager;
            _db = db;
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
        public IPagedList<Project> GetAllProjectsForUserPermission(string SiteId, bool isTemplate, string SearchText, List<string> projectIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.IsTemplate == isTemplate);

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.Name.ToLower().Contains(SearchText.ToLower())
                );
            }

            query = query.Select(x => new Project
            {
                Id = x.Id,
                Name = x.Name,
                SiteId = x.SiteId,
                ProjectUserMappings = x.ProjectUserMappings.Where(m => !m.Deleted && m.ProjectId == x.Id).Select(mapping => new ProjectUserMapping
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
                }).ToList(),
                ProjectModules = x.ProjectModules.Where(m => !m.Deleted).Select(module => new ProjectModule
                {
                    Id = module.Id,
                    Name = module.Name,
                    ProjectModulesUserMappings = module.ProjectModulesUserMappings.Where(m => !m.Deleted && m.ProjectModuleId == module.Id).Select(mapping => new ProjectModulesUserMapping
                    {
                        Id = mapping.Id,
                        FullAccess = mapping.FullAccess,
                        ViewOnly = mapping.ViewOnly,
                        Notes = mapping.Notes,
                    }).ToList(),
                }).ToList(),
            });

            var list = new PagedList<Project>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetProjectUserByProjectId
        // Title: GetProjectUserByProjectId
        // Description: The method selects relevant fields from the project user entity
        public async Task<List<ProjectUserMapping>> GetProjectUserByProjectId(string SiteId, string projectId)
        {
            var query = _projectUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId && x.ProjectId == projectId).Select(x => new ProjectUserMapping
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                AspNetUserId = x.AspNetUserId,
                FullAccess = x.FullAccess,
                ViewOnly = x.ViewOnly,
                Notes = x.Notes,
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    Person = new Person
                    {
                        Id = x.User.Person.Id,
                        FirstName = x.User.Person.FirstName,
                        FullName = x.User.Person.FirstName + " " + x.User.Person.LastName,
                    },
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetProjectUserById
        // Title: GetProjectUserById
        // Description: This method retrieves a project user from the database by its unique identifier (`id`). 
        public async Task<ProjectUserMapping> GetProjectUserById(string id)
        {
            var query = _projectUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetRecordByUserIdandProjectId
        // Title: GetRecordByUserIdandProjectId
        // Description: This method retrieves a project user from the database by employeeId and projectId. 
        public async Task<ProjectUserMapping> GetRecordByUserIdandProjectId(string SiteId, string userId, string projectId)
        {
            var query = _projectUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId && x.AspNetUserId == userId && x.ProjectId == projectId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertProjectUser
        // Title : InsertProjectUser
        // Description: Inserts a new ProjectUserMapping entity into the repository.
        public void InsertProjectUser(ProjectUserMapping entity)
        {
            _projectUserMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectUser
        // Title : UpdateProjectUsers
        // Description: Updates an existing ProjectUserMapping entity in the repository.
        public void UpdateProjectUser(ProjectUserMapping entity)
        {
            _projectUserMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectUser
        // Title : DeleteProjectUsers
        // Description: Deletes a ProjectUserMapping entity from the repository.
        public void DeleteProjectUser(ProjectUserMapping entity)
        {
            entity.Deleted = true;
            _projectUserMappingRepository.Update(entity);
        }
        #endregion
    }
}
