using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.RequirementStatusTrackings
{
    public interface IRequirementStatusTrackingService
    {
        #region GetAllRequirementStatusTrackingRecords
        Task<RequirementStatusTrackingResult> GetAllRecords(string siteId, string loggedUserId, string employeeId, RequirementStatusTrackingFilter filter);
        #endregion

        #region GetRequirementStatusTrackingRecordById
        Task<RequirementStatusTrackingRecord> GetRecordById(string siteId, string loggedUserId, string employeeId, string recordId);
        #endregion

        #region SaveRequirementStatusTrackingRecord
        // Creates (isCreate) or updates one complete record; validates the whole batch and commits atomically.
        Task<RequirementStatusTrackingResult> SaveRecord(string siteId, string loggedUserId, string employeeId, RequirementStatusTrackingSaveModel model, bool isCreate, DateTime now);
        #endregion

        #region DeleteRequirementStatusTrackingRecords
        // Soft-deletes the selected complete records; all ids must be found and writable or nothing is deleted.
        Task<RequirementStatusTrackingResult> DeleteRecords(string siteId, string loggedUserId, string employeeId, List<string> recordIds, DateTime now);
        #endregion
    }
}
