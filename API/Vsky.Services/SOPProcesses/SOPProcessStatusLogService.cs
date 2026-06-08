using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SOPProcesses
{
    public class SOPProcessStatusLogService : ISOPProcessStatusLogService
    {
        #region Define Services
        private readonly IRepository<SOPProcessStatusLog> _sOPProcessStatusLogRepository;
        #endregion

        #region Services Initializations

        public SOPProcessStatusLogService(IRepository<SOPProcessStatusLog> sOPProcessStatusLogRepository)
        {
            _sOPProcessStatusLogRepository = sOPProcessStatusLogRepository;
        }

        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region InsertSOPProcessStatusLog
        // Title: InsertSOPProcessStatusLog
        // Description: This method inserts a new InsertSOPProcessStatusLog entity into the repository. It takes a InsertSOPProcessStatusLog object as input and uses the _sOPProcessStatusLogRepository to handle the insertion operation.
        public void InsertSOPProcessStatusLog(SOPProcessStatusLog entity)
        {
            _sOPProcessStatusLogRepository.Insert(entity);
        }
        #endregion
    }
}
