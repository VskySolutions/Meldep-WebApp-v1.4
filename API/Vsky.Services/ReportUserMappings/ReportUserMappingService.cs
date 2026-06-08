using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ReportUserMappings
{
    public class ReportUserMappingService : IReportUserMappingService
    {
        #region Define Service
        private readonly IRepository<ReportUserMapping> _reportUserMappingRepository;
        private readonly IRepository<ReportSettingsDetails> _reportSettingsDetailsRepository;

        #endregion

        #region Service Initializations
        public ReportUserMappingService(IRepository<ReportUserMapping> reportUserMappingRepository,
            IRepository<ReportSettingsDetails> reportSettingsDetailsRepository)
        {
            _reportUserMappingRepository = reportUserMappingRepository;
            _reportSettingsDetailsRepository = reportSettingsDetailsRepository;
        }

        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllReportsForUserPermission
        // Title: GetAllReportsForUserPermission
        // Description: This method retrieves a paginated list of reports based on various search criteria.
        public IPagedList<ReportSettingsDetails> GetAllReportsForUserPermission(
            string SiteId,
            string SearchText,
            List<string> reportIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _reportSettingsDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.ReportSetting.SiteId == SiteId);

            if (reportIds != null && reportIds.Any())
                query = query.Where(x => reportIds.Contains(x.ReportId));

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                   m.ReportName.ToLower().Contains(SearchText.ToLower())
                );
            }

            query = query.Select(x => new ReportSettingsDetails
            {
                Id = x.Id,
                ReportName = x.ReportName,
                ReportUserMapping = x.ReportUserMapping.Where(m => !m.Deleted && m.ReportSettingsDetailId == x.Id).Select(mapping => new ReportUserMapping
                {
                    Id = mapping.Id,
                    FullAccess = mapping.FullAccess,
                    ViewOnly = mapping.ViewOnly,
                    User = new ApplicationUser
                    {
                        Id = mapping.User.Id,
                        Person = new Person
                        {
                            Id = mapping.User.Person.Id,
                            FullName = mapping.User.Person.FirstName + " " + mapping.User.Person.LastName,
                        },
                    }
                }).ToList(),
            });

            var list = new PagedList<ReportSettingsDetails>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetReportUserByReportSettingsDetailId
        // Title: GetReportUserByReportSettingsDetailId
        // Description: The method selects relevant fields from the report user entity.
        public async Task<List<ReportUserMapping>> GetReportUserByReportSettingsDetailId(string SiteId, string reportSettingsDetailId)
        {
            var query = _reportUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ReportSettingsDetail.ReportSetting.SiteId == SiteId && x.ReportSettingsDetailId == reportSettingsDetailId).Select(x => new ReportUserMapping
            {
                Id = x.Id,
                ReportSettingsDetailId = x.ReportSettingsDetailId,
                AspNetUserId = x.AspNetUserId,
                FullAccess = x.FullAccess,
                ViewOnly = x.ViewOnly,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetReportUserById
        // Title: GetReportUserById
        // Description: This method retrieves a report user from the database by its unique identifier (`id`). 
        public async Task<ReportUserMapping> GetReportUserById(string id)
        {
            var query = _reportUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetReportByUserIdandReportSettingsDetailId
        // Title: GetReportByUserIdandReportSettingsDetailId
        // Description: This method retrieves a report user from the database by userId and reportSettingsDetailId. 
        public async Task<ReportUserMapping> GetReportByUserIdandReportSettingsDetailId(string SiteId, string userId, string reportSettingsDetailId)
        {
            var query = _reportUserMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ReportSettingsDetail.ReportSetting.SiteId == SiteId && x.AspNetUserId == userId && x.ReportSettingsDetailId == reportSettingsDetailId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertReportUser
        // Title : InsertReportUser
        // Description: Inserts a new ReportUserMapping entity into the repository.
        public void InsertReportUser(ReportUserMapping entity)
        {
            _reportUserMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateReportUser
        // Title : UpdateReportUser
        // Description: Updates an existing ReportUserMapping entity in the repository.
        public void UpdateReportUser(ReportUserMapping entity)
        {
            _reportUserMappingRepository.Update(entity);
        }
        #endregion

    }
}
