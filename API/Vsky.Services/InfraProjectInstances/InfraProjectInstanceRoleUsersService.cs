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
    public class InfraProjectInstanceRoleUsersService : IInfraProjectInstanceRoleUsersService
    {
        #region Define Services
        private readonly IRepository<InfraProjectInstanceRoleUsers> _infraProjectInstanceRoleUsersRepository;
        #endregion

        #region Services Initializations
        public InfraProjectInstanceRoleUsersService(
            IRepository<InfraProjectInstanceRoleUsers> infraProjectInstanceRoleUsersRepository)
        {
            _infraProjectInstanceRoleUsersRepository = infraProjectInstanceRoleUsersRepository;
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

        #region GetInfraProjectInstanceRoleUsersById
        // Title: GetInfraProjectInstanceRoleUsersById
        // Description: This method retrieves a Infra Project Instance Role Users from the database by its unique identifier (`id`). 
        public async Task<InfraProjectInstanceRoleUsers> GetInfraProjectInstanceRoleUsersById(string id)
        {
            var query = _infraProjectInstanceRoleUsersRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInfraProjectInstanceRoleUsersByUsername
        // Title: GetInfraProjectInstanceRoleUsersByUsername
        // Description: This method retrieves a InfraProjectInstanceRoleUser based on its name. It allows an optional exclusion of a InfraProjectInstanceRoleUser by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific Infra Project Instance Role User. The method returns the first matching Infra Project Instance Role User or null if no match is found.
        public async Task<InfraProjectInstanceRoleUsers> GetInfraProjectInstanceRoleUsersByUsername(string projectInstanceId, string username, string id = null)
        {
            var query = _infraProjectInstanceRoleUsersRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectInstanceRole.ProjectInstanceId == projectInstanceId && x.UserName.ToLower() == username.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        // Assign ftp to project instance
        #region InsertInfraProjectInstanceRoleUsers
        public void InsertInfraProjectInstanceRoleUsers(Models.InfraProjectInstanceRoleUsers entity)
        {
            _infraProjectInstanceRoleUsersRepository.Insert(entity);
        }
        #endregion

        #region UpdateInfraProjectInstanceRoleUsers
        public void UpdateInfraProjectInstanceRoleUsers(Models.InfraProjectInstanceRoleUsers entity)
        {
            _infraProjectInstanceRoleUsersRepository.Update(entity);
        }
        #endregion

        #region DeleteInfraProjectInstanceRoleUsers
        public void DeleteInfraProjectInstanceRoleUsers(Models.InfraProjectInstanceRoleUsers entity)
        {
            entity.Deleted = true;
            _infraProjectInstanceRoleUsersRepository.Update(entity);
        }
        #endregion
    }
}


