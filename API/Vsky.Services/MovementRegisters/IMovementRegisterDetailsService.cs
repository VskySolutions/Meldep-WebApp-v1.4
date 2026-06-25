using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.MovementRegisters
{
    public interface IMovementRegisterDetailsService
    {
        #region GetAllMovementRegister
        Task<IPagedList<object>> GetMovementRegisterDetails(
            string SiteId,
            string createdBy,
            string searchText,
            string employeeId,
            string typeId,
            DateTime? fromDate,
            DateTime? toDate,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetMovementRegisterDetailByTypeId
        Task<Models.MovementRegisterDetails> GetMovementRegisterDetailByTypeId(string siteId, string employeeId, DateTime? date, string typeId);
        #endregion

        #region GetMovementRegisterDetailsById
        Task<Models.MovementRegisterDetails> GetMovementRegisterDetailsById(string id);
        #endregion

        #region InsertMovementRegisterDetails
        void InsertMovementRegisterDetails(Models.MovementRegisterDetails entity);
        #endregion

        #region UpdateMovementRegisterDetails
        void UpdateMovementRegisterDetails(Models.MovementRegisterDetails entity);
        #endregion

        #region DeleteMovementRegisterDetails
        void DeleteMovementRegisterDetails(Models.MovementRegisterDetails entity);
        #endregion
    }
}
