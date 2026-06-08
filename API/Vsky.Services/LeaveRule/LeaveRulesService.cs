using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.LeaveRule
{
    public class LeaveRulesService : ILeaveRulesService
    {
        #region Define Services
        private readonly IRepository<LeaveRules> _leaveRulesRepository;
        #endregion

        #region Services Initializations

        public LeaveRulesService(IRepository<LeaveRules> leaveRulesRepository)
        {
            _leaveRulesRepository = leaveRulesRepository;
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

        #region GetAllLeaveRules
        // Title: GetAllLeaveRules
        // Description: This method retrieves a paginated list of LeaveRules
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<LeaveRules> GetAllLeaveRules(string SiteId, string SearchText, List<int> years, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _leaveRulesRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (years != null && years.Any())
                query = query.Where(x => years.Contains(x.Year));

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
                     m.Year.ToString().ToLower().Contains(SearchText.ToLower())
                );
            }
               query = query.Select(x => new LeaveRules
            {
                Id = x.Id,
                Year = x.Year,
                IsGenerated = x.IsGenerated,
            });

            var list = new PagedList<LeaveRules>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetLeaveRulesById
        // Title: GetLeaveRulesById
        // Description: This method retrieves a LeaveRules from the database by its unique identifier (`id`). 
        public async Task<LeaveRules> GetLeaveRulesById(string id)
        {
            var query = _leaveRulesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLeaveRulesByYear
        // Title: GetLeaveRulesByYear
        // Description: This method retrieves a LeaveRules from the database by year. 
        public async Task<LeaveRules> GetLeaveRulesByYear(string SiteId, int year)
        {
            var query = _leaveRulesRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Year == year);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLeaveRulesDetailsById
        // Title: GetLeaveRulesDetailsById
        // Description: The method selects relevant fields from the LeaveRules entity.
        public async Task<LeaveRules> GetLeaveRulesDetailsById(string id)
        {
            var query = _leaveRulesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new LeaveRules
            {
                Id = x.Id,
                Year = x.Year,
                IsGenerated = x.IsGenerated,
                LeaveRuleLinesList = x.LeaveRuleLinesList.Where(p => !p.Deleted).Select(p => new LeaveRuleLines
                {
                    Id = p.Id,
                    EmploymentTypeId = p.EmploymentTypeId,
                    CasualLeaves = p.CasualLeaves,
                    SickLeaves = p.SickLeaves,
                    LeaveRuleId = p.LeaveRuleId,
                    EmploymentType = new DropDown
                    {
                        Id = p.EmploymentType.Id,
                        DropDownValue = p.EmploymentType.DropDownValue
                    }
                }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertLeaveRules
        // Title: InsertLeaveRules
        // Description: This method inserts a new LeaveRules entity into the repository. It takes a LeaveRules object as input and uses the _leaveRulesRepository to handle the insertion operation.
        public void InsertLeaveRules(LeaveRules entity)
        {
            _leaveRulesRepository.Insert(entity);
        }
        #endregion

        #region UpdateLeaveRules
        // Title: UpdateLeaveRules
        // Description: This method updates the specified LeaveRules entity in the repository. It takes a LeaveRules object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateLeaveRules(LeaveRules entity)
        {
            _leaveRulesRepository.Update(entity);
        }
        #endregion

        #region DeleteLeaveRules
        // Title: DeleteLeaveRules
        // Description: Marks the specified LeaveRules entity as deleted by setting its `Deleted` property to true. 
        public void DeleteLeaveRules(LeaveRules entity)
        {
            entity.Deleted = true;

            _leaveRulesRepository.Update(entity);
        }
        #endregion
    }
}

