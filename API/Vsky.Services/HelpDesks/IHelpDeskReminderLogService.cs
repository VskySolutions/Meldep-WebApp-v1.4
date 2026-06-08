using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.HelpDesks
{
    public interface IHelpDeskReminderLogService
    {
        #region GetAllHelpDeskReminderLogs
        Task<IList<HelpDeskReminderLog>> GetAllHelpDeskReminderLogs(string HelpdeskId, string Title);
        #endregion

        #region InsertHelpDeskReminderLogs
        void InsertHelpDeskReminderLogs(HelpDeskReminderLog entity);
        #endregion
    }
}
