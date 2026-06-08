using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vsky.Services.ProjectSwimLanes
{
    public interface IMasterProjectSwimlaneListsServices
    {
        #region GetById
        Task<Models.MasterProjectSwimlaneLists> GetById(string id);
        #endregion

        #region Get List By SwimlaneTypeId
        Task<List<Models.MasterProjectSwimlaneLists>> GetMasterProjectSwimlaneBySwimlaneTypeId(string SwimlaneTypeId);
        #endregion

        #region Validate Master ListName
        Task<bool> ValidateListName(string siteId, string name);
        #endregion

        #region CRUD
        void InsertMasterProjectSwimlaneList(Models.MasterProjectSwimlaneLists entity);
        void UpdateMasterProjectSwimlaneList(Models.MasterProjectSwimlaneLists entity);
        void DeleteMasterProjectSwimlaneList(Models.MasterProjectSwimlaneLists entity);
        #endregion
    }
}
