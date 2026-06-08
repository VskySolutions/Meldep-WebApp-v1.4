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
using Vsky.Services.Sites;

namespace Vsky.Services.LeadActivityLogss
{
    public class LeadActivityLogsService : ILeadActivityLogsService
    {
        #region Fields
        private readonly IRepository<LeadActivityLogs> _leadActivityLogsRepository;
        #endregion

        #region Ctor

        public LeadActivityLogsService(IRepository<LeadActivityLogs> leadActivityLogsRepository)
        {
            _leadActivityLogsRepository = leadActivityLogsRepository;
        }

        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region Public Methods

        public IPagedList<LeadActivityLogs> GetAllLeadActivityLogs(string SiteId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _leadActivityLogsRepository.TableNoTracking.Where(x => !x.Deleted && x.Leads.SiteId == SiteId);

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
                query = query.Select(x => new LeadActivityLogs
                {
                    Id = x.Id,
                });
            }
            else
            {
                // projection
                query = query.Select(x => new LeadActivityLogs

                {
                    Id = x.Id,
                    LeadsId = x.LeadsId,
                    LeadStageId = x.LeadStageId,
                    LeadActivityId = x.LeadActivityId,
                    ActivityDate = x.ActivityDate,
                    ActivityNote = x.ActivityNote,
                    IsFutureActivity = x.IsFutureActivity,
                    User = new ApplicationUser
                    {
                        Id = x.User.Id,
                        UserName = x.User.UserName,
                        Person = new Person
                        {
                            Id = x.User.PersonId,
                            FirstName = x.User.Person.FirstName,
                            LastName = x.User.Person.LastName,
                        }
                    },
                });
            }

            var list = new PagedList<LeadActivityLogs>(query, page, pageSize);
            return list;
        }

        public async Task<LeadActivityLogs> GetById(string id)
        {
            var query = _leadActivityLogsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Include(m => m.LeadActivity).Include(m => m.LeadStage);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public List<LeadActivityLogs> GetByLeadId(string id)
        {
            var query = _leadActivityLogsRepository.Table;
            query = query.Where(x => !x.Deleted && x.LeadsId == id).Include(m => m.LeadActivity).Include(m => m.LeadStage).Include(m=>m.User);
            var item = query.ToList();
            return item;
        }

        public void InsertLeadActivityLogs(LeadActivityLogs entity)
        {
            _leadActivityLogsRepository.Insert(entity);
        }

        public void UpdateLeadActivityLogs(LeadActivityLogs entity)
        {
            _leadActivityLogsRepository.Update(entity);
        }

        public void DeleteLeadActivityLogs(LeadActivityLogs entity)
        {
            entity.Deleted = true;

            _leadActivityLogsRepository.Update(entity);
        }

        #endregion
    }
}
