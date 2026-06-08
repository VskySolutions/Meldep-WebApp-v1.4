using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.InfraProjectInstances
{
    public interface IInfraProjectInstanceService
    {
        #region GetAllInfraProjectInstanceForList
        public IPagedList<InfraProjectInstance> GetAllInfraProjectInstanceForList(
            string siteId,
            List<string> infraProjectIds,
            List<string> platformIds,
            List<string> instanceTypeIds,
            string searchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetAllInfraProjectInstanceListForDropdown
        Task<List<InfraProjectInstance>> GetAllInfraProjectInstanceListForDropdown(string SiteId, string projectId = null);
        #endregion

        #region GetInfraProjectInstanceById
        Task<InfraProjectInstance> GetInfraProjectInstanceById(string id);
        #endregion

        #region GetInfraProjectInstanceInDetailById
        Task<Models.InfraProjectInstance> GetInfraProjectInstanceInDetailById(string id);
        #endregion

        #region InsertInfraProjectInstance
        void InsertInfraProjectInstance(Models.InfraProjectInstance entity);
        #endregion

        #region UpdateInfraProjectInstance
        void UpdateInfraProjectInstance(Models.InfraProjectInstance entity);
        #endregion

        #region DeleteInfraProjectInstance
        void DeleteInfraProjectInstance(Models.InfraProjectInstance entity);
        #endregion
    }
}
