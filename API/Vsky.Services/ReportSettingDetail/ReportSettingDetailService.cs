using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Sites;

namespace Vsky.Services.ReportSettingDetail
{
    public class ReportSettingDetailService : IReportSettingDetailService
    {
        #region Services Initializations
        private readonly IRepository<ReportSettingsDetails> _reportSettingsDetailsRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISiteService _siteService;
        private readonly IApplicationUserRoleService _applicationUserRoleService;

        public ReportSettingDetailService(
            IRepository<ReportSettingsDetails> reportSettingsDetailsRepository, 
            UserManager<ApplicationUser> userManager, 
            ISiteService siteService,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _reportSettingsDetailsRepository = reportSettingsDetailsRepository;
            _userManager = userManager;
            _siteService = siteService;
            _applicationUserRoleService = applicationUserRoleService;
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

        #region GetAllReportsDetails
        // Title: GetAllReportsDetails
        public async Task<IPagedList<ReportSettingsDetails>> GetAllReportsDetails(
            string SiteId,
            string userId,
            List<string> reportGroupIdsForRoles,
            string SearchText,
            List<string> reportIds,
            string reportGroupId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false,
            bool showAllReports = false
        )
        {

            //var query = _reportSettingsDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.ReportSetting.SiteId == SiteId);
            var query = _reportSettingsDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
       
            bool isAdmin = await IsCurrentUserAdmin(userId, SiteId);
            if (!isAdmin && !showAllReports)
                query = query.Where(p => reportGroupIdsForRoles.Contains(p.ReportGroupId) || p.ReportUserMapping.Any(m => !m.Deleted && m.AspNetUserId == userId && (m.FullAccess || m.ViewOnly)));

            if (!string.IsNullOrEmpty(reportGroupId))
                query = query.Where(x => x.ReportGroupId == reportGroupId);

            if (reportIds != null && reportIds.Any())
                query = query.Where(x => reportIds.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }


            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                m.ReportName.ToLower().Contains(SearchText.ToLower()) ||
                m.ReportGroup.DropDownValue.ToLower().Contains(SearchText.ToLower())||
                m.ReportDescription.ToLower().Contains(SearchText.ToLower()) ||
                m.Url.ToLower().Contains(SearchText.ToLower())
                );
            }

             query = query
            .OrderBy(x => x.ReportGroup.DropDownValue)
            .ThenBy(x => x.ReportName).Select(x => new ReportSettingsDetails
             {
                 Id = x.Id,
                 ReportId = x.ReportId,
                 ReportGroupId = x.ReportGroupId,
                 ReportName = x.ReportName,
                 ReportDescription = x.ReportDescription,
                 Url = x.Url,
                 ReportGroup = new DropDown { Id = x.ReportGroup.Id, DropDownValue = x.ReportGroup.DropDownValue },
                 ReportSetting = new ReportSettings
                 {
                    WorkspaceId = x.ReportSetting.WorkspaceId,
                    SiteId = x.ReportSetting.SiteId
                 },
                ReportUserMapping = x.ReportUserMapping.Where(m => !m.Deleted && m.ReportSettingsDetailId == x.Id && (showAllReports || isAdmin || m.AspNetUserId == userId)).Take(1).Select(m => new ReportUserMapping
                 {
                     Id = m.Id,
                     AspNetUserId = m.AspNetUserId,
                     FullAccess = m.FullAccess,
                     ViewOnly = m.ViewOnly
                 }).ToList(),
             });

            var list = new PagedList<ReportSettingsDetails>(query, page, pageSize);
            return await Task.FromResult(list);
        }
        #endregion

        #region GetReportSettingsDetailsById
        // Title: GetReportSettingsDetailsById
        public async Task<ReportSettingsDetails> GetById(string id)
        {
            var query = _reportSettingsDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetReportByName
        public async Task<ReportSettingsDetails> GetReportByName(string siteId, string reportName, string id = null)
        {
            var query = _reportSettingsDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.ReportName == reportName);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllReportListForDropdown
        public async Task<List<ReportSettingsDetails>> GetAllReportListForDropdown(string SiteId, List<string> reportGroupIdsForRoles, string LoggedUserId, bool showAllReports = false)
        {
            var query = _reportSettingsDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!IsAdmin && !showAllReports)
                query = query.Where(r => reportGroupIdsForRoles.Contains(r.ReportGroupId) || r.ReportUserMapping.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId));

            query = query.Select(x => new ReportSettingsDetails
            {
                Id = x.Id,
                ReportId = x.ReportId,
                ReportName = x.ReportName
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region AddUpdatReportDetails
        public async Task<bool> AddUpdatReportDetails(string reportSettingId, string reportId, string reportName, string description, DateTime GetDateTime)
        {
            var data = await _reportSettingsDetailsRepository.TableNoTracking.Where(m => m.ReportId == reportId).FirstOrDefaultAsync();
            if (data == null)
            {
                ReportSettingsDetails reportSettingsDetails = new ReportSettingsDetails();
                reportSettingsDetails.ReportSettingId = reportSettingId;
                reportSettingsDetails.ReportId = reportId;
                reportSettingsDetails.ReportName = reportName;
                reportSettingsDetails.CreatedOnUtc = GetDateTime;
                reportSettingsDetails.UpdatedOnUtc = GetDateTime;
                _reportSettingsDetailsRepository.Insert(reportSettingsDetails);
            }
            else
            {
                data.ReportId = reportId;
                data.ReportName = reportName;
                data.UpdatedOnUtc = GetDateTime;
                _reportSettingsDetailsRepository.Update(data);
            }
            return true;
        }
        #endregion

        #region IndsertReportSettingDetails
        // Title: IndsertReportSettingDetails
        public void IndsertReportSettingDetails(ReportSettingsDetails entity)
        {
            _reportSettingsDetailsRepository.Insert(entity);
        }
        #endregion

        #region UpdateReportSettingDetails
        // Title: UpdateReportSettingsDetails
        public void UpdateReportSettingDetails(ReportSettingsDetails entity)
        {
            _reportSettingsDetailsRepository.Update(entity);
        }
        #endregion

        #region DeleteReport
        public void DeleteReport(ReportSettingsDetails entity)
        {
            entity.Deleted = true;
            _reportSettingsDetailsRepository.Update(entity);
        }
        #endregion

        private async Task<bool> IsCurrentUserAdmin(string CId, string SiteId)
        {
            var userdata = await _userManager.FindByIdAsync(CId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            //var roles = await _userManager.GetRolesAsync(user);
            var roles = await _applicationUserRoleService.GetRoleNamesByUserAndSite(user.Id, SiteId);
            var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin");

            return isAdmin;
        }
    }
}
