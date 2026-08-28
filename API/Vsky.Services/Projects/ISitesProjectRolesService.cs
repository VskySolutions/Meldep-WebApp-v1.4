using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Projects
{
    public interface ISitesProjectRolesService
    {
        #region GetAllSiteProjectRolesListForDropdown
        Task<List<SitesProjectRoles>> GetAllSiteProjectRolesListForDropdown(string siteId);
        #endregion
    }
}


