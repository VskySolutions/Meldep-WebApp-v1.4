using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Requirements
{
    public interface IRequirementChangeLogService
    {
        #region GetRequirementChangeLogById
        Task<RequirementChangeLog> GetRequirementChangeLogById(string id);
        #endregion

        //#region GetRequirementChangeLogDetailsById
        //Task<RequirementChangeLog> GetRequirementChangeLogDetailsById(string id);
        //#endregion

        #region InsertRequirementChangeLogList
        void InsertRequirementChangeLogList(IList<RequirementChangeLog> entity);
        #endregion

        #region UpdateRequirementChangeLogList
        void UpdateRequirementChangeLogList(IList<RequirementChangeLog> entity);
        #endregion

        #region DeleteRequirementChangeLogList
        void DeleteRequirementChangeLogList(List<RequirementChangeLog> entity);
        #endregion
    }
}
