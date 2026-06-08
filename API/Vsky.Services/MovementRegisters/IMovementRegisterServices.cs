using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.MovementRegisters
{
    public interface IMovementRegisterServices
    {
        #region GetAllMovementRegister
        IPagedList<Models.MovementRegister> GetAllMovementRegister(
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

        #region GetAllMovementRegistersForDashboard
        IPagedList<Models.MovementRegister> GetAllMovementRegistersForDashboard(
            string SiteId,
            string timeZone,
            string createdBy,
            string searchText,
            string employeeId,
            DateTime? fromDate,
            DateTime? toDate,
            bool isViewMore,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetMovementRegisterById
        Task<Models.MovementRegister> GetMovementRegisterById(string id);
        #endregion

        #region GetMovementRegisterDetailsById
        Task<Models.MovementRegister> GetMovementRegisterDetailsById(string id, string detailId);

        Task<Models.MovementRegister> GetMovementRegisterByDate(string SiteId, DateTime? Date, string id = null);

        Task<(DateTime? startDate, DateTime? endDate)> GetMovementRegisterDateRange(string siteId);

        #endregion

        #region InsertMovementRegister
        void InsertMovementRegister(Models.MovementRegister entity);
        #endregion

        #region UpdateMovementRegister
        void UpdateMovementRegister(Models.MovementRegister entity);
        #endregion

        #region DeleteMovementRegister
        void DeleteMovementRegister(Models.MovementRegister entity);
        #endregion

    }
}
