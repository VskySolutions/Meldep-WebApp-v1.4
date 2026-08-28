using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectUserMappings
{
    public interface IProjectUserMappingService
    {
        #region GetAllProjectsForUserPermission
        Task<IPagedList<Project>> GetAllProjectsForUserPermission(
            string SiteId, 
            bool isTemplate,
            string userId,
            string employeeId, 
            string SearchText, 
            List<string> projectIds, 
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
        );
        #endregion
    }
}
