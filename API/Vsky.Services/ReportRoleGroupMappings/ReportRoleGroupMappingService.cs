using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ReportRoleGroupMappings
{
    public class ReportRoleGroupMappingService : IReportRoleGroupMappingService
    {
        #region Define Services
        private readonly IRepository<ReportRoleGroupMapping> _reportRoleGroupMappingRepository;
        #endregion

        #region Services Initializations
        public ReportRoleGroupMappingService(
             IRepository<ReportRoleGroupMapping> reportRoleGroupMappingRepository)
        {
            _reportRoleGroupMappingRepository = reportRoleGroupMappingRepository;
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

        #region GetAllReportGroupRoles
        // Title: GetAllReportGroupRoles
        // Description: This method retrieves a paginated list of report group-role mappings with optional filters and sorting.
        public async Task<IPagedList<ReportRoleGroupMapping>> GetAllReportGroupRoles(
            string SiteId,
            string SearchText,
            List<string> siteRoleIds,
            List<string> reportGroupIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {

            var query = _reportRoleGroupMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (siteRoleIds != null && siteRoleIds.Any())
                query = query.Where(x => siteRoleIds.Contains(x.SiteRoleId));

            if (reportGroupIds != null && reportGroupIds.Any())
                query = query.Where(x => reportGroupIds.Contains(x.ReportGroupId));

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
                m.SitesRoles.ApplicationRole.Name.ToLower().Contains(SearchText.ToLower()) ||
                m.ReportGroup.DropDownValue.ToLower().Contains(SearchText.ToLower()) 
                );
            }

            query = query.Select(x => new ReportRoleGroupMapping
            {
                Id = x.Id,
                SiteRoleId = x.SiteRoleId,
                ReportGroupId = x.ReportGroupId,
                Active = x.Active,
                SitesRoles = new SitesRoles
                {
                    Id = x.Id,
                    ApplicationRole = new ApplicationRole
                    {
                        Name = x.SitesRoles.ApplicationRole.Name
                    }
                },
                ReportGroup = new DropDown
                {
                    Id = x.ReportGroup.Id,
                    DropDownValue = x.ReportGroup.DropDownValue
                }

            });

            var list = new PagedList<ReportRoleGroupMapping>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetReportGroupRoleById
        // Title: GetReportGroupRoleById
        // Description: This method retrieves a specific report group-role mapping by its unique identifier.
        public async Task<ReportRoleGroupMapping> GetReportGroupRoleById(string id)
        {
            var query = _reportRoleGroupMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAssignedReportGroupNames
        // Title: GetAssignedReportGroupNames
        // Description: This method returns the report group names already mapped to a given site role.
        public async Task<List<string>> GetAssignedReportGroupNames(string siteId, string siteRoleId, List<string> reportGroupIds)
        {
            return await _reportRoleGroupMappingRepository.TableNoTracking
                   .Where(x => x.SiteId == siteId && x.SiteRoleId == siteRoleId && reportGroupIds
                   .Contains(x.ReportGroupId) && !x.Deleted)
                   .Select(x => x.ReportGroup.DropDownValue).ToListAsync();
        }
        #endregion

        #region GetReportGroupsByRoles
        // Title: GetReportGroupsByRoles
        // Description: This method retrieves all active, non-deleted report group mappings associated with the specified site and list of site role IDs.
        public async Task<List<ReportRoleGroupMapping>> GetReportGroupsByRoles(string siteId, string[] siteRoleIds)
        {
            return await _reportRoleGroupMappingRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Active && x.SiteId == siteId && siteRoleIds.Contains(x.SiteRoleId))
                .ToListAsync();
        }
        #endregion

        #region InsertReportGroupRole
        // Title: InsertReportGroupRole
        // Description: This method inserts a new ReportRoleGroupMapping entity into the repository.It takes a ReportRoleGroupMapping object as input and uses the _reportRoleGroupMappingRepository to handle the insertion operation.
        public void InsertReportGroupRole(ReportRoleGroupMapping entity)
        {
            _reportRoleGroupMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateReportGroupRole
        // Title: UpdateReportGroupRole
        // Description: This method updates the specified ReportRoleGroupMapping entity in the repository.It takes a ReportRoleGroupMapping object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateReportGroupRole(ReportRoleGroupMapping entity)
        {
            _reportRoleGroupMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteReportGroupRole
        // Title: DeleteReportGroupRole
        // Description: Marks the specified ReportRoleGroupMapping entity as deleted by setting its `Deleted` property to true.
        public void DeleteReportGroupRole(ReportRoleGroupMapping entity)
        {
            entity.Deleted = true;
            _reportRoleGroupMappingRepository.Update(entity);
        }
        #endregion
    }
}
