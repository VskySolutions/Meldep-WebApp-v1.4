using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmployeeStatuses
{
    public class EmployeeStatusService : IEmployeeStatusService
    {
        #region Define Services
        private readonly IRepository<EmployeeStatus> _employeeStatusRepository;
        #endregion

        #region Services Initializations
        public EmployeeStatusService(IRepository<EmployeeStatus> employeeStatusRepository)
        {
            _employeeStatusRepository = employeeStatusRepository;
        }
        #endregion

        #region GetEmployeeStatusById
        // Title: GetEmployeeStatusById
        // Description: This method retrieves a employee status from the database by its unique identifier (`id`). 
        public async Task<EmployeeStatus> GetEmployeeStatusById(string id)
        {
            var query = _employeeStatusRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertEmployeeStatusList
        // Title: InsertEmployeeStatusList
        // Description: This method inserts a new Employee statuses entity into the repository. It takes a Employee status object as input and uses the _employeeRepository to handle the insertion operation.
        public void InsertEmployeeStatusList(IList<EmployeeStatus> entities)
        {
            _employeeStatusRepository.Insert(entities);
        }
        #endregion

        #region UpdateEmployeeStatusList
        // Title: UpdateEmployeeStatusList
        // Description: This method updates the specified Employee status entity in the repository. It takes a Employee status object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployeeStatusList(IList<EmployeeStatus> entities)
        {
            _employeeStatusRepository.Update(entities);
        }
        #endregion

        #region DeleteEmployeeStatusList
        // Title: DeleteEmployeeStatusList
        // Description: Marks the specified employee status entity as deleted by setting its `Deleted` property to true. 
        public void DeleteEmployeeStatusList(List<EmployeeStatus> entities)
        {
            var list = new List<EmployeeStatus>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _employeeStatusRepository.Update(list);
        }
        #endregion
    }
}