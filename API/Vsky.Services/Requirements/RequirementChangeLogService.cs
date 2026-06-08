using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Requirements
{
    public class RequirementChangeLogService : IRequirementChangeLogService
    {
        #region Define Services
        private readonly IRepository<RequirementChangeLog> _requirementChangeLogRepository;
        #endregion

        #region Services Initializations

        public RequirementChangeLogService(IRepository<RequirementChangeLog> requirementChangeLogRepository)
        {
            _requirementChangeLogRepository = requirementChangeLogRepository;
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

        #region GetRequirementChangeLogById
        // Title: GetRequirementChangeLogById
        // Description: This method retrieves a RequirementChangeLog from the database by its unique identifier (`id`). 
        public async Task<RequirementChangeLog> GetRequirementChangeLogById(string id)
        {
            var query = _requirementChangeLogRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertRequirementChangeLogList
        // Title: InsertRequirementChangeLogList
        // Description: This method inserts a new RequirementChangeLog entity into the repository. It takes a RequirementChangeLog object as input and uses the _requirementChangeLogRepository to handle the insertion operation.
        public void InsertRequirementChangeLogList(IList<RequirementChangeLog> entities)
        {
            _requirementChangeLogRepository.Insert(entities);
        }
        #endregion

        #region UpdateRequirementChangeLogList
        // Title: UpdateRequirementChangeLogList
        // Description: This method updates the specified RequirementChangeLog entity in the repository. It takes a RequirementChangeLog object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateRequirementChangeLogList(IList<RequirementChangeLog> entities)
        {
            _requirementChangeLogRepository.Update(entities);
        }
        #endregion

        #region DeleteRequirementChangeLogList
        // Title: DeleteRequirementChangeLogList
        // Description: Marks the specified RequirementChangeLog entity as deleted by setting its `Deleted` property to true. 
        public void DeleteRequirementChangeLogList(List<RequirementChangeLog> entities)
        {
            var list = new List<RequirementChangeLog>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _requirementChangeLogRepository.Update(list);
        }
        #endregion
    }
}

