using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;


namespace Vsky.Services.ProjectModulesUserMappings
{
    public interface IProjectModulesUserMappingService
    {
        #region GetUsersByProjectModuleId
        Task<List<Models.ProjectModulesUserMapping>> GetUsersByProjectModuleId(string SiteId, string projectModuleId);
        Task<List<Models.ProjectModulesUserMapping>> GetUsersByProjectModuleIds(string SiteId, List<string> projectModuleIds);
        #endregion

        #region GetProjectModuleUserById
        Task<Models.ProjectModulesUserMapping> GetProjectModuleUserById(string id);
        #endregion

        #region GetRecordByUserIdandProjectModuleId
        Task<Models.ProjectModulesUserMapping> GetRecordByUserIdandProjectModuleId(string SiteId, string userId, string projectModuleId);
        #endregion

        #region InsertProjectModuleUser
        void InsertProjectModuleUser(Models.ProjectModulesUserMapping entity);
        #endregion

        #region UpdateProjectModuleUser
        void UpdateProjectModuleUser(Models.ProjectModulesUserMapping entity);
        #endregion

        #region DeleteProjectModuleUser
        void DeleteProjectModuleUser(Models.ProjectModulesUserMapping entity);
        #endregion
    }
}
