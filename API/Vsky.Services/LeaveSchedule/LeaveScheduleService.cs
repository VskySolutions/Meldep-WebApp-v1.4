using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.LeaveSchedule
{
    public class LeaveScheduleService : ILeaveScheduleService
    {
        #region Fields
        private readonly IRepository<LeaveSchedules> _leaveSchedulesRepository;
        private readonly ISiteService _siteService;
        #endregion

        #region Ctor
        public LeaveScheduleService(IRepository<LeaveSchedules> LeaveSchedulesRepository, ISiteService siteService)
        {
            _leaveSchedulesRepository = LeaveSchedulesRepository;
            _siteService = siteService;
        }
        #endregion

        #region public methods
        public async Task<IList<LeaveSchedules>> GetAllLeaveEvents(string SiteId)
        {
            var query = _leaveSchedulesRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            query = query.OrderBy(x => x.Date);

            query = query.Select(x => new LeaveSchedules
            {
                Id = x.Id,
                SiteId = x.SiteId,
                LeaveRuleId = x.LeaveRuleId,
                Title = x.Title,
                Description = x.Description,
                Date = x.Date,
                CreatedOnUtc = x.CreatedOnUtc,
                LeaveRules = new LeaveRules
                {
                    Id = x.LeaveRules.Id,
                    Year = x.LeaveRules.Year
                }
            });

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<LeaveSchedules> GetLeaveEventById(string SiteId, string id)
        {
            var query = await _leaveSchedulesRepository.TableNoTracking.Where(x => x.Id == id && x.SiteId == SiteId && !x.Deleted).FirstOrDefaultAsync();
            return query;
        }

        public async Task<LeaveSchedules> GetLeaveEventDetailsById(string id)
        {
            var query = _leaveSchedulesRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            query = query.Select(x => new LeaveSchedules
            {
                Id= x.Id,
                SiteId = x.SiteId,
                LeaveRuleId = x.LeaveRuleId,
                Title = x.Title,
                Description = x.Description,
                Date= x.Date,
                CreatedById = x.CreatedById,
                CreatedOnUtc= x.CreatedOnUtc,
                LeaveRules = new LeaveRules
                {
                    Id = x.LeaveRules.Id,
                    Year = x.LeaveRules.Year
                }
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<List<LeaveSchedules>> GetEmployeeLeaveListForDashboard(string SiteId, DateTime GetDateTime)
        {
            var nextTwoMonthsDate = GetDateTime.AddMonths(2);
            var query = _leaveSchedulesRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Date >= GetDateTime && x.Date <= nextTwoMonthsDate).OrderBy(x => x.Date).Take(7);

            query = query.Select(x => new LeaveSchedules
            {
                Id = x.Id,
                SiteId = x.SiteId,
                LeaveRuleId = x.LeaveRuleId,
                Title = x.Title,
                Description = x.Description,
                Date = x.Date
            });

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<LeaveSchedules> GetLeaveScheduleByDate(string SiteId, DateTime? Date)
        {
            var query = _leaveSchedulesRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Date == Date.Value.Date);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public void InsertLeaveEvent(LeaveSchedules entity)
        {
            _leaveSchedulesRepository.Insert(entity);
        }

        public void UpdateLeaveEvent(LeaveSchedules entity)
        {
            _leaveSchedulesRepository.Update(entity);
        }

        public void DeleteLeaveEvent(LeaveSchedules entity)
        {
            entity.Deleted = true;
            _leaveSchedulesRepository.Update(entity);
        }
        #endregion
    }
}
