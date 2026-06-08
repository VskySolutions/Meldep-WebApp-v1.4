using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.HelpDesks
{
    public class HelpDeskReminderLogService : IHelpDeskReminderLogService
    {
        #region Define Services
        private readonly IRepository<HelpDeskReminderLog> _helpDeskReminderLogRepository;
        #endregion

        #region Services Initializations
        public HelpDeskReminderLogService(IRepository<HelpDeskReminderLog> helpDeskReminderLogRepository)
        {
            _helpDeskReminderLogRepository = helpDeskReminderLogRepository;
        }
        #endregion

        #region GetAllHelpDeskReminderLogs
        public async Task<IList<HelpDeskReminderLog>> GetAllHelpDeskReminderLogs(string HelpdeskId, string Title)
        {
            var query = _helpDeskReminderLogRepository.TableNoTracking
                .Where(x => !x.Deleted && x.HelpDeskId == HelpdeskId && !x.SitesEmailNotifications.Deleted && x.SitesEmailNotifications.Subject == Title);

            var list = await query.OrderByDescending(x => x.Date).ToListAsync();
            return list;
        }
        #endregion

        #region InsertHelpDeskReminderLogs
        public void InsertHelpDeskReminderLogs(HelpDeskReminderLog entity)
        {
            _helpDeskReminderLogRepository.Insert(entity);
        }
        #endregion

    }
}
