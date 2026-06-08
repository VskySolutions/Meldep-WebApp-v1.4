using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmployeeOrgLocations
{
    public class EmployeeOrgLocationService : IEmployeeOrgLocationService
    {
        #region Define Services
        private readonly IRepository<EmployeeOrgLocation> _employeeOrgLocationRepository;
        #endregion

        #region Services Initializations
        public EmployeeOrgLocationService(IRepository<EmployeeOrgLocation> employeeOrgLocationRepository)
        {
            _employeeOrgLocationRepository = employeeOrgLocationRepository;
        }
        #endregion

        #region GetEmployeeOrgLocationById
        // Title: GetEmployeeOrgLocationById
        // Description: This method retrieves a employee Org Location from the database by its unique identifier (`id`). 
        public async Task<EmployeeOrgLocation> GetEmployeeOrgLocationById(string id)
        {
            var query = _employeeOrgLocationRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertEmployeeOrgLocationList
        // Title: InsertEmployeeOrgLocationList
        // Description: This method inserts a new Employee Org Location entity into the repository. It takes a Employee Org Location object as input and uses the _employeeOrgLocationRepository to handle the insertion operation.
        public void InsertEmployeeOrgLocationList(IList<EmployeeOrgLocation> entities)
        {
            _employeeOrgLocationRepository.Insert(entities);
        }
        #endregion

        #region UpdateEmployeeOrgLocationList
        // Title: UpdateEmployeeOrgLocationList
        // Description: This method updates the specified Employee Org Location entity in the repository. It takes a Employee Org Location object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployeeOrgLocationList(IList<EmployeeOrgLocation> entities)
        {
            _employeeOrgLocationRepository.Update(entities);
        }
        #endregion

        #region DeleteEmployeeOrgLocationList
        // Title: DeleteEmployeeOrgLocationList
        // Description: Marks the specified employee Org Location entity as deleted by setting its `Deleted` property to true. 
        public void DeleteEmployeeOrgLocationList(List<EmployeeOrgLocation> entities)
        {
            var list = new List<EmployeeOrgLocation>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _employeeOrgLocationRepository.Update(list);
        }
        #endregion
    }
}

