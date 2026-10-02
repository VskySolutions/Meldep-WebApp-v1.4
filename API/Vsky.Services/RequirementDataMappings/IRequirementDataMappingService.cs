using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.RequirementDataMappings
{
    public interface IRequirementDataMappingService
    {
        #region GetAllRequirementDataMappings
        Task<IPagedList<Vsky.Models.RequirementDataMapping>> GetAllRequirementDataMappings(
            string SiteId,
            string loggedUserId,
            string SearchText,
            List<string> requirementIds,
            string source,
            string target,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetAllRequirementDataMappingGroups
        //Task<List<RequirementDataMappingGroup>> GetAllRequirementDataMappingGroups(string siteId);
        Task<IPagedList<RequirementDataMappingGroup>> GetAllRequirementDataMappingGroups(
            string SiteId,
            string loggedUserId,
            string SearchText,
            List<string> projectIds,
            List<string> projectModuleIds,
            List<string> requirementIds,
            string source,
            string target,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetById
        Task<RequirementDataMapping> GetRequirementDataMappingById(string id);
        #endregion

        #region GetRequirementDataMappingsByRequirementId
        Task<List<RequirementDataMapping>> GetRequirementDataMappingsByRequirementId(string requirementId);
        #endregion

        #region InsertRequirementDataMapping
        void InsertRequirementDataMapping(Vsky.Models.RequirementDataMapping entity);
        #endregion

        #region UpdateRequirementDataMapping
        void UpdateRequirementDataMapping(Vsky.Models.RequirementDataMapping entity);
        #endregion

        #region DeleteRequirementDataMapping
        void DeleteRequirementDataMapping(Vsky.Models.RequirementDataMapping entity);
        #endregion
    }
}
