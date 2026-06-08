using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.LeaveRuleLine
{
    public class LeaveRuleLinesService : ILeaveRuleLinesService
    {
        #region Define Services
        private readonly IRepository<LeaveRuleLines> _leaveRuleLinesRepository;
        #endregion

        #region Services Initializations
        public LeaveRuleLinesService(IRepository<LeaveRuleLines> leaveRuleLinesRepository)
        {
            _leaveRuleLinesRepository = leaveRuleLinesRepository;
        }
        #endregion

        #region GetLeaveRuleLinesByEmployeeTypeId
        // Title: GetLeaveRuleLinesByEmployeeTypeId
        // Description: This method retrieves a LeaveRuleLines based on its name.The method returns the first matching LeaveRuleLines or null if no match is found.
        public async Task<LeaveRuleLines> GetLeaveRuleLinesByEmployeeTypeId(string EmployeeTypeId, string id = null)
        {
            var query = _leaveRuleLinesRepository.TableNoTracking.Where(x => !x.Deleted && x.EmploymentTypeId == EmployeeTypeId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLeaveRuleLinesByLeaveRuleId
        // Title: GetLeaveRuleLinesByLeaveRuleId
        // Description: This method retrieves a LeaveRuleLines based on its LeaveRuleId.The method returns the first matching LeaveRuleLines or null if no match is found.
        public async Task<List<LeaveRuleLines>> GetLeaveRuleLinesByLeaveRuleId(string LeaveRuleId, string employeeType = null)
        {
            var query = _leaveRuleLinesRepository.TableNoTracking.Where(m => !m.Deleted && m.LeaveRuleId == LeaveRuleId);

            if (!string.IsNullOrEmpty(employeeType))
                query = query.Where(m => m.EmploymentTypeId == employeeType);

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetLeaveRuleLineById
        // Title: GetLeaveRuleLineById
        // Description: This method retrieves a LeaveRuleLines from the database by its unique identifier (`id`). 
        public async Task<LeaveRuleLines> GetLeaveRuleLineById(string id)
        {
            var query = _leaveRuleLinesRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertLeaveRuleLine
        // Title: InsertLeaveRuleLine
        // Description: This method inserts a new LeaveRuleLine entity into the repository. It takes a LeaveRuleLine object as input and uses the _leaveRuleLinesRepository to handle the insertion operation.
        public void InsertLeaveRuleLine(LeaveRuleLines entity)
        {
            _leaveRuleLinesRepository.Insert(entity);
        }
        #endregion

        #region InsertLeaveRuleLinesList
        public void InsertLeaveRuleLinesList(IList<LeaveRuleLines> entities)
        {
            _leaveRuleLinesRepository.Insert(entities);
        }
        #endregion

        #region UpdateLeaveRuleLinesList
        // Title: UpdateLeaveRuleLinesList
        // Description: This method updates the specified LeaveRuleLine entity in the repository. It takes a LeaveRuleLines object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateLeaveRuleLinesList(IList<LeaveRuleLines> entities)
        {
            _leaveRuleLinesRepository.Update(entities);
        }
        #endregion

        #region DeleteLeaveRuleLinesList
        // Title: DeleteLeaveRuleLinesList
        // Description: Marks the specified LeaveRuleLines entity as deleted by setting its `Deleted` property to true. 
        public void DeleteLeaveRuleLinesList(List<LeaveRuleLines> entities)
        {
            var list = new List<LeaveRuleLines>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _leaveRuleLinesRepository.Update(list);
        }
        #endregion
    }
}
