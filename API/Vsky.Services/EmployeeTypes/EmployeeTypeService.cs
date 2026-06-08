using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmployeeTypes
{
    public class EmployeeTypeService : IEmployeeTypeService
    {
        #region Define Services
        private readonly IRepository<EmployeeType> _employeeTypeRepository;
        #endregion

        #region Services Initializations
        public EmployeeTypeService(IRepository<EmployeeType> employeeTypeRepository)
        {
            _employeeTypeRepository = employeeTypeRepository;
        }
        #endregion

        #region GetEmployeeTypeById
        // Title: GetEmployeeTypeById
        // Description: This method retrieves a employee type from the database by its unique identifier (`id`). 
        public async Task<EmployeeType> GetEmployeeTypeById(string id)
        {
            var query = _employeeTypeRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertEmployeeTypeList
        public void InsertEmployeeTypeList(IList<EmployeeType> entities)
        {
            _employeeTypeRepository.Insert(entities);
        }
        #endregion

        #region UpdateEmployeeTypeList
        // Title: UpdateEmployeeTypeList
        // Description: This method updates the specified Employee type entity in the repository. It takes a Employee type object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployeeTypeList(IList<EmployeeType> entities)
        {
            _employeeTypeRepository.Update(entities);
        }
        #endregion

        #region DeleteEmployeeTypeList
        // Title: DeleteEmployeeTypeList
        // Description: Marks the specified employee type entity as deleted by setting its `Deleted` property to true. 
        public void DeleteEmployeeTypeList(List<EmployeeType> entities)
        {
            var list = new List<EmployeeType>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _employeeTypeRepository.Update(list);
        }
        #endregion
    }
}
