using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmployeeClientLocations
{
    public class EmployeeClientLocationService : IEmployeeClientLocationService
    {
        #region Define Services
        private readonly IRepository<EmployeeClientLocation> _employeeClientLocationRepository;
        #endregion

        #region Services Initializations
        public EmployeeClientLocationService(IRepository<EmployeeClientLocation> employeeClientLocationRepository)
        {
            _employeeClientLocationRepository = employeeClientLocationRepository;
        }
        #endregion

        #region GetEmployeeClientLocationById
        // Title: GetEmployeeClientLocationById
        // Description: This method retrieves a employee Client Location from the database by its unique identifier (`id`). 
        public async Task<EmployeeClientLocation> GetEmployeeClientLocationById(string id)
        {
            var query = _employeeClientLocationRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertEmployeeClientLocationList
        // Title: InsertEmployeeClientLocationList
        // Description: This method inserts a new Employee Client Location entity into the repository. It takes a Employee Client Location object as input and uses the _employeeClientLocationRepository to handle the insertion operation.
        public void InsertEmployeeClientLocationList(IList<EmployeeClientLocation> entities)
        {
            _employeeClientLocationRepository.Insert(entities);
        }
        #endregion

        #region UpdateEmployeeClientLocationList
        // Title: UpdateEmployeeClientLocationList
        // Description: This method updates the specified Employee Client Location entity in the repository. It takes a Employee Client Location object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployeeClientLocationList(IList<EmployeeClientLocation> entities)
        {
            _employeeClientLocationRepository.Update(entities);
        }
        #endregion

        #region DeleteEmployeeClientLocationList
        // Title: DeleteEmployeeClientLocationList
        // Description: Marks the specified employee Client Location entity as deleted by setting its `Deleted` property to true. 
        public void DeleteEmployeeClientLocationList(List<EmployeeClientLocation> entities)
        {
            var list = new List<EmployeeClientLocation>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _employeeClientLocationRepository.Update(list);
        }
        #endregion
    }
}

