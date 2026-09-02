using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Projects
{
    public class SitesProjectRolesService : ISitesProjectRolesService
    {
        #region Define Services
        private readonly IRepository<SitesProjectRoles> _sitesProjectRolesRepository;
        #endregion

        #region Services Initializations
        public SitesProjectRolesService(IRepository<SitesProjectRoles> sitesProjectRolesRepository
        )
        {
            _sitesProjectRolesRepository = sitesProjectRolesRepository;
        }
        #endregion

        #region GetAllSiteProjectRolesListForDropdown
        public async Task<List<SitesProjectRoles>> GetAllSiteProjectRolesListForDropdown(string siteId)
        {
            var list = await _sitesProjectRolesRepository.TableNoTracking
                .Where(x => x.SiteId == siteId && !x.Deleted)
                .OrderBy(x => x.MasterProjectRoles.Name.Replace(" ", ""))
                .Select(x => new SitesProjectRoles
                {
                    Id = x.Id,
                    MasterProjectRoleId = x.MasterProjectRoleId,
                    MasterProjectRoles = new MasterProjectRoles
                    {
                        Name = x.MasterProjectRoles.Name
                    },
                    SitesProjectRolesPermissions = x.SitesProjectRolesPermissions
                        .Where(p => !p.Deleted)
                        .Select(p => new SitesProjectRolesPermissions
                        {
                            Id = p.Id,
                            FullAccess = p.FullAccess,
                            ViewOnly = p.ViewOnly,
                            Notes = p.Notes
                        })
                        .ToList()
                })
                .ToListAsync();

            return list;
        }
        #endregion

        #region GetSiteProjectRoleByName
        public async Task<SitesProjectRoles> GetSiteProjectRoleByName(string siteId, string name)
        {
            return await _sitesProjectRolesRepository.TableNoTracking
                .FirstOrDefaultAsync(x =>
                    x.SiteId == siteId &&
                    x.MasterProjectRoles.Name == name &&
                    !x.Deleted);
        }
        #endregion
    }
}
