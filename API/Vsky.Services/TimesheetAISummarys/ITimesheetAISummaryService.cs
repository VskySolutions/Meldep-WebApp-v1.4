using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.TimesheetAISummarys
{
    public interface ITimesheetAISummaryService
    {
        #region GetAllTimesheetAISummary
        Task<IPagedList<TimesheetAISummary>> GetAllTimesheetAISummary(
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
        );
        #endregion

        #region GetTimesheetAISummaryById
        Task<TimesheetAISummary> GetTimesheetAISummaryById(string id);
        #endregion

        #region GetTimesheetAISummaryDetailsById
        Task<TimesheetAISummary> GetTimesheetAISummaryDetailsById(string id);
        #endregion

        #region InsertTimesheetAISummary
        void InsertTimesheetAISummary(TimesheetAISummary entity);
        #endregion

        #region UpdateTimesheetAISummary
        void UpdateTimesheetAISummary(TimesheetAISummary entity);
        #endregion

        #region DeleteTimesheetAISummary
        void DeleteTimesheetAISummary(TimesheetAISummary entity);
        #endregion
    }
}
