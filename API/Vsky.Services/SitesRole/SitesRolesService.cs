using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.SitesRole
{
    public class SitesRolesService : ISitesRolesService
    {
        #region Define Services
        private readonly IRepository<SitesRoles> _SitesRolesRepository;
        #endregion

        #region Services Initializations
        public SitesRolesService(IRepository<SitesRoles> SitesRolesRepository)
        {
            _SitesRolesRepository = SitesRolesRepository;
        }
        #endregion

        #region GetAllSiteRoles
        // Title: GetAllSiteRoles
        // Description: This method retrieves a paginated list of siteroles based on various search criteria.
        public  IPagedList<SitesRoles> GetAllSiteRoles(string siteId, string searchText, List<string> siteRoleIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _SitesRolesRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            if (siteRoleIds != null && siteRoleIds.Any())
                query = query.Where(x => siteRoleIds.Contains(x.Id));

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(m =>
                   (m.ApplicationRole.Name.ToLower().Contains(searchText.ToLower()))
                );
            }

            query = query.Select(x => new SitesRoles
            {
                Id = x.Id,
                ApplicationRole = new ApplicationRole
                {
                    Name = x.ApplicationRole.Name,
                }
            }).OrderBy(x => x.ApplicationRole.Name);

            var list = new PagedList<SitesRoles>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetRolesBySiteId
        // Title: GetRolesBySiteId
        // Description: This method retrieves all site roles associated with a specific SiteId. 
        public async Task<IList<SitesRoles>> GetRolesBySiteId(string siteId)
        {
            return await _SitesRolesRepository.TableNoTracking.Include(x => x.ApplicationRole).Where(x => !x.Deleted && x.SiteId == siteId).OrderBy(x => x.ApplicationRole.Name).ToListAsync();
        }
        #endregion

        #region GetSitesRoles
        // Title: GetSitesRoles
        // Description: This method retrieves all site roles that are not marked as deleted. 
        public async Task<IList<SitesRoles>> GetSitesRoles()
        {
            return await _SitesRolesRepository.TableNoTracking.Where(x => !x.Deleted).ToListAsync();
        }
        #endregion

        #region GetRolesBySiteRoleIds
        public async Task<IList<SitesRoles>> GetRolesBySiteRoleIds(string[] siteRoleIds)
        {
            return await _SitesRolesRepository.TableNoTracking.Include(x => x.ApplicationRole).Where(x => siteRoleIds.Contains(x.Id) && !x.Deleted).ToListAsync();
        }
        #endregion

        #region GetRoleIdBySiteRoleId
        public async Task<string> GetRoleIdBySiteRoleId(string siteRoleId)
        {
            return await _SitesRolesRepository.TableNoTracking
                .Where(x => x.Id == siteRoleId && !x.Deleted)
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();
        }
        #endregion

        #region GetSiteRoleIdsByRoleIds
        public async Task<IList<string>> GetSiteRoleIdsByRoleIds(string siteId, string[] roleIds)
        {
            return await _SitesRolesRepository.TableNoTracking.Where(x => roleIds.Contains(x.RoleId) && x.SiteId == siteId && !x.Deleted).Select(x => x.Id).ToListAsync();

        }
        #endregion

        #region InsertSitesRoles
        // Title: InsertSitesRoles
        // Description: This method inserts a  SitesRoles entity into the repository. It takes a SitesRoles object as input and uses the _SitesRolesRepository to handle the insertion operation.
        public void InsertSitesRoles(SitesRoles entity)
        {
            _SitesRolesRepository.Insert(entity);
        }
        #endregion

        #region UpdateSitesRoles
        // Title : UpdateSitesRoles
        // Description: This method updates an existing SitesRoles entity in the repository.It receives a SitesRoles object and performs an update operation using the repository
        public void UpdateSitesRoles(SitesRoles entity)
        {
            _SitesRolesRepository.Update(entity);
        }
        #endregion

        #region DeleteSitesRoles
        // Title : DeleteSitesRoles
        // Description: This method performs a soft delete on a SitesRoles entity.It sets the Deleted flag to true and updates the entity in the repository.
        public void DeleteSitesRoles(SitesRoles entity)
        {
            entity.Deleted = true;
            _SitesRolesRepository.Update(entity);
        }
        #endregion
    }
}
