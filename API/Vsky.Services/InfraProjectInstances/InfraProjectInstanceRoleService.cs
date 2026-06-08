using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraProjectInstances
{
    public class InfraProjectInstanceRoleService : IInfraProjectInstanceRoleService
    {
        #region Define Services
        private readonly IRepository<InfraProjectInstanceRole> _infraProjectInstanceRoleRepository;
        #endregion

        #region Services Initializations
        public InfraProjectInstanceRoleService(
            IRepository<InfraProjectInstanceRole> infraProjectInstanceRoleRepository)
        {
            _infraProjectInstanceRoleRepository = infraProjectInstanceRoleRepository;
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

        #region GetInfraProjectInstanceRoleById
        // Title: GetInfraProjectInstanceRoleById
        // Description: This method retrieves a Infra Database Project Instance Mapping from the database by its unique identifier (`id`). 
        public async Task<InfraProjectInstanceRole> GetInfraProjectInstanceRoleById(string id)
        {
            var query = _infraProjectInstanceRoleRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInfraProjectInstanceRoleByRoleName
        // Title: GetInfraProjectInstanceRoleByRoleName
        // Description: This method retrieves a InfraProjectInstanceRole based on its name. It allows an optional exclusion of a InfraProjectInstanceRole by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific Infra Project Instance Role. The method returns the first matching Infra Project Instance Role or null if no match is found.
        public async Task<InfraProjectInstanceRole> GetInfraProjectInstanceRoleByRoleName(string SiteId, string projectInstanceId, string roleName, string id = null)
        {
            var query = _infraProjectInstanceRoleRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectInstance.SiteId == SiteId && x.ProjectInstanceId == projectInstanceId && x.RoleName.ToLower() == roleName.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInfraProjectInstanceRoleInDetailByInstanceId
        // Title: GetInfraProjectInstanceRoleInDetailByInstanceId
        // Description: The method selects relevant fields from the Infra Project Instance role entity by instanceId
        public async Task<List<InfraProjectInstanceRole>> GetInfraProjectInstanceRoleInDetailByInstanceId(string instanceId)
        {
            var query = _infraProjectInstanceRoleRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectInstanceId == instanceId)
                .Select(x => new InfraProjectInstanceRole
                {
                    Id = x.Id,
                    RoleName = x.RoleName,
                    InfraProjectInstanceRoleUsers = x.InfraProjectInstanceRoleUsers.Where(x => !x.Deleted).Select(r => new InfraProjectInstanceRoleUsers
                    {
                        Id = r.Id,
                        UserName = r.UserName,
                        Password = r.Password
                    }).ToList()
                });
            var item = await query.OrderBy(x => x.RoleName).ToListAsync();
            return item;
        }
        #endregion

        // Assign ftp to project instance
        #region InsertInfraProjectInstanceRole
        public void InsertInfraProjectInstanceRole(InfraProjectInstanceRole entity)
        {
            _infraProjectInstanceRoleRepository.Insert(entity);
        }
        #endregion

        #region UpdateInfraProjectInstanceRole
        public void UpdateInfraProjectInstanceRole(Models.InfraProjectInstanceRole entity)
        {
            _infraProjectInstanceRoleRepository.Update(entity);
        }
        #endregion

        #region DeleteInfraProjectInstanceRole
        public void DeleteInfraProjectInstanceRole(InfraProjectInstanceRole entity)
        {
            entity.Deleted = true;
            _infraProjectInstanceRoleRepository.Update(entity);
        }
        #endregion
    }
}


