using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using Vsky.Services.SetReminders;
using Vsky.Services.Sites;

namespace Vsky.Services.SetReminders
{
    public class SetReminderService : ISetReminderService
    {
        #region Fields
        private readonly IRepository<SetReminder> _setReminderRepository;
        #endregion

        #region Ctor

        public SetReminderService(IRepository<SetReminder> setReminderRepository)
        {
            _setReminderRepository = setReminderRepository;
        }

        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region Public Methods
        public IPagedList<SetReminder> GetAllSetReminder(string SiteId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _setReminderRepository.TableNoTracking.Where(x => !x.Deleted && x.LeadActivityLogs.Leads.SiteId == SiteId);
            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedById);
            }

            if (lookup)
            {
                query = query.Select(x => new SetReminder
                {
                    Id = x.Id,
                });
            }
            else
            {
                // projection
                query = query.Select(x => new SetReminder

                {
                    Id = x.Id,
                    LeadActivityLogId = x.LeadActivityLogId,
                    ReminderAfterDays = x.ReminderAfterDays,
                    Time = x.Time,
                    Note = x.Note,
                    ReminderDateTime = x.ReminderDateTime,
                    IsMailStatus = x.IsMailStatus,
                });
            }

            var list = new PagedList<SetReminder>(query, page, pageSize);
            return list;
        }

        public async Task<SetReminder> GetById(string id)
        {
            var query = _setReminderRepository.Table;
            query = query.Where(x => !x.Deleted && x.Id == id).Include(m => m.LeadActivityLogs);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public void InsertReminder(SetReminder entity)
        {
            _setReminderRepository.Insert(entity);
        }

        public void UpdateReminder(SetReminder entity)
        {
            _setReminderRepository.Update(entity);
        }

        public void DeleteReminder(SetReminder entity)
        {
            entity.Deleted = true;

            _setReminderRepository.Update(entity);
        }
        #endregion
    }
}
