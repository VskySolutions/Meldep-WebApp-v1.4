using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.LeadUserGroupMappings
{
    public class LeadUserGroupMappingService : ILeadUserGroupMappingService
    {
        #region Define Services
        private readonly IRepository<LeadUserGroupMapping> _leadUserGroupMappingRepository;
        #endregion

        #region Services Initializations
        public LeadUserGroupMappingService(
             IRepository<LeadUserGroupMapping> leadUserGroupMappingRepository)
        {
            _leadUserGroupMappingRepository = leadUserGroupMappingRepository;
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

        #region GetAllLeadUserGroups
        // Title: GetAllLeadUserGroups
        // Description: This method retrieves a paginated list of Lead group-user mappings with optional filters and sorting.
        public async Task<IPagedList<LeadUserGroupMapping>> GetAllLeadUserGroups(
            string SiteId,
            string SearchText,
            List<string> userIds, 
            List<string> leadGroupIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {

            var query = _leadUserGroupMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (userIds != null && userIds.Any())
                query = query.Where(x => userIds.Contains(x.UserId));

            if (leadGroupIds != null && leadGroupIds.Any())
                query = query.Where(x => leadGroupIds.Contains(x.LeadGroupId));

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
                m.User.Person.FirstName.ToLower().Contains(SearchText.ToLower()) ||
                m.LeadGroup.DropDownValue.ToLower().Contains(SearchText.ToLower())
                );
            }

            query = query.Select(x => new LeadUserGroupMapping
            {
                Id = x.Id,
                UserId = x.UserId,
                LeadGroupId = x.LeadGroupId,
                Active = x.Active,
                User = new ApplicationUser
                {
                    Id = x.Id,
                    Person = new Person
                    {
                        FullName = x.User.Person.FirstName + " " + x.User.Person.LastName,
                        FirstName = x.User.Person.FirstName
                    }
                },
                LeadGroup = new DropDown
                {
                    Id = x.LeadGroup.Id,
                    DropDownValue = x.LeadGroup.DropDownValue
                }

            });

            var list = new PagedList<LeadUserGroupMapping>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetLeadUserGroupUserById
        // Title: GetLeadUserGroupUserById
        // Description: This method retrieves a specific Lead group-user mapping by its unique identifier.
        public async Task<LeadUserGroupMapping> GetLeadUserGroupUserById(string id)
        {
            var query = _leadUserGroupMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAssignedLeadGroupNames
        // Title: GetAssignedLeadGroupNames
        // Description: This method returns the Lead group names already mapped to a given user.
        public async Task<List<string>> GetAssignedLeadGroupNames(string siteId, string userId, List<string> leadGroupIds)
        {
            return await _leadUserGroupMappingRepository.TableNoTracking
                   .Where(x => x.SiteId == siteId && x.UserId == userId && leadGroupIds
                   .Contains(x.LeadGroupId) && !x.Deleted)
                   .Select(x => x.LeadGroup.DropDownValue).ToListAsync();
        }
        #endregion

        #region GetLeadGroupsByUsers
        // Title: GetLeadGroupsByUsers
        // Description: This method retrieves all active, non-deleted Lead group mappings associated with the specified site and list of user IDs.
        public async Task<List<LeadUserGroupMapping>> GetLeadGroupsByUsers(string siteId, string userId)
        {
            return await _leadUserGroupMappingRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Active && x.SiteId == siteId && x.UserId == userId)
                .ToListAsync();
        }
        #endregion

        #region InsertLeadUserGroup
        // Title: InsertLeadUserGroup
        // Description: This method inserts a new LeadUserGroupMapping entity into the repository.It takes a LeadUserGroupMapping object as input and uses the _leadUserGroupMappingRepository to handle the insertion operation.
        public void InsertLeadUserGroup(LeadUserGroupMapping entity)
        {
            _leadUserGroupMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateLeadUserGroup
        // Title: UpdateLeadUserGroup
        // Description: This method updates the specified LeadUserGroupMapping entity in the repository.It takes a LeadUserGroupMapping object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateLeadUserGroup(LeadUserGroupMapping entity)
        {
            _leadUserGroupMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteLeadUserGroup
        // Title: DeleteLeadUserGroup
        // Description: Marks the specified LeadUserGroupMapping entity as deleted by setting its `Deleted` property to true.
        public void DeleteLeadUserGroup(LeadUserGroupMapping entity)
        {
            entity.Deleted = true;
            _leadUserGroupMappingRepository.Update(entity);
        }
        #endregion
    }
}
