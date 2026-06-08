using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectModulesUserMappings
{
    public class ProjectModulesUserMappingService : IProjectModulesUserMappingService
    {
        #region Define Service
        /// <summary>
        /// Define Service
        /// </summary>
        private readonly IRepository<ProjectModulesUserMapping> _projectModulesUserMappingRepository;
        private readonly ApplicationDbContext _db;
        private readonly IRepository<ProjectModule> _projectModuleRepository;

        #endregion

        #region Service Initializations
        /// <summary>
        /// Service Initializations
        /// </summary>
        /// <param name="ProjectUserMappingRepository"></param>
        public ProjectModulesUserMappingService(IRepository<ProjectModulesUserMapping> projectModulesUserMappingRepository,
            ApplicationDbContext db, IRepository<ProjectModule> projectModuleRepository)
        {
            _projectModulesUserMappingRepository = projectModulesUserMappingRepository;
            _db = db;
            _projectModuleRepository = projectModuleRepository;
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

        #region GetUsersByProjectModuleId
        // Title: GetUsersByProjectModuleId
        // Description: The method selects relevant fields from the project module user entity.
        public async Task<List<ProjectModulesUserMapping>> GetUsersByProjectModuleId(string SiteId, string projectModuleId)
        {
            var query = _projectModulesUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModule.SiteId == SiteId && x.ProjectModuleId == projectModuleId).Select(x => new ProjectModulesUserMapping
            {
                Id = x.Id,
                ProjectModuleId = x.ProjectModuleId,
                AspNetUserId = x.AspNetUserId,
                FullAccess = x.FullAccess,
                ViewOnly = x.ViewOnly,
                Notes = x.Notes,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetUsersByProjectModuleIds
        // Title: GetUsersByProjectModuleIds
        // Description: The method selects relevant fields from the project module user entity.
        public async Task<List<ProjectModulesUserMapping>> GetUsersByProjectModuleIds(string SiteId, List<string> projectModuleIds)
        {
            var query = _projectModulesUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModule.SiteId == SiteId && projectModuleIds.Contains(x.ProjectModuleId)).Select(x => new ProjectModulesUserMapping
            {
                Id = x.Id,
                ProjectModuleId = x.ProjectModuleId,
                AspNetUserId = x.AspNetUserId,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    Person = new Person
                    {
                        FirstName = x.User.Person.FirstName,
                        FullName = x.User.Person.FirstName + " " + x.User.Person.LastName
                    }
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                FullAccess = x.FullAccess,
                ViewOnly = x.ViewOnly,
                Notes = x.Notes,
            }).AsEnumerable().OrderBy(x => x.User.Person.FirstName).ThenBy(x => x.ProjectModule.Name).ToList();

            var list = query;
            return list;
        }
        #endregion

        #region GetProjectModuleUserById
        // Title: GetProjectModuleUserById
        // Description: This method retrieves a project module user from the database by its unique identifier (`id`). 
        public async Task<ProjectModulesUserMapping> GetProjectModuleUserById(string id)
        {
            var query = _projectModulesUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetRecordByUserIdandProjectModuleId
        // Title: GetRecordByUserIdandProjectModuleId
        // Description: This method retrieves a project module user from the database by employeeId and projectId. 
        public async Task<ProjectModulesUserMapping> GetRecordByUserIdandProjectModuleId(string SiteId, string userId, string projectModuleId)
        {
            var query = _projectModulesUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModule.SiteId == SiteId && x.AspNetUserId == userId && x.ProjectModuleId == projectModuleId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertProjectModuleUser
        // Title : InsertProjectModuleUser
        // Description: Inserts a new ProjectModulesUserMapping entity into the repository.
        public void InsertProjectModuleUser(Models.ProjectModulesUserMapping entity)
        {
            _projectModulesUserMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectModuleUser
        // Title : UpdateProjectModuleUser
        // Description: Updates an existing ProjectModulesUserMapping entity in the repository.
        public void UpdateProjectModuleUser(Models.ProjectModulesUserMapping entity)
        {
            _projectModulesUserMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectModuleUser
        // Title : DeleteProjectModuleUser
        // Description: Deletes a ProjectModulesUserMapping entity from the repository.
        public void DeleteProjectModuleUser(Models.ProjectModulesUserMapping entity)
        {
            entity.Deleted = true;
            _projectModulesUserMappingRepository.Update(entity);
        }
        #endregion
    }
}
