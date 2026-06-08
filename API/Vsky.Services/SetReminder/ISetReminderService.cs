using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SetReminders
{
    public interface ISetReminderService
    {
        IPagedList<SetReminder> GetAllSetReminder(string SiteId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        Task<SetReminder> GetById(string id);

        void InsertReminder(SetReminder entity);

        void UpdateReminder(SetReminder entity);

        void DeleteReminder(SetReminder entity);
    }
}
