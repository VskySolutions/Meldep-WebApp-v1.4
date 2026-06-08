using Vsky.Models;

namespace Vsky.Services.SOPProcesses
{
    public interface ISOPProcessStatusLogService
    {
        #region InsertSOPProcessStatusLog
        void InsertSOPProcessStatusLog(SOPProcessStatusLog entity);
        #endregion
    }
}
