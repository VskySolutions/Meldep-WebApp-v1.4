using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmployeeDepartments
{
    public class EmployeeDepartmentService : IEmployeeDepartmentService
    {
        #region Define Services
        private readonly IRepository<EmployeeDepartment> _employeeDepartmentRepository;
        #endregion

        #region Services Initializations
        public EmployeeDepartmentService(IRepository<EmployeeDepartment> employeeDepartmentRepository)
        {
            _employeeDepartmentRepository = employeeDepartmentRepository;
        }
        #endregion

        #region GetDepartmentById
        // Title: GetDepartmentById
        // Description: This method retrieves a department based on its id. It allows an optional exclusion of a department by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific department. The method returns the first matching department or null if no match is found.
        public async Task<EmployeeDepartment> GetDepartmentById(string id, string employeeDepartmentId)
        {
            var query = _employeeDepartmentRepository.TableNoTracking.Where(x => !x.Deleted && x.EmployeeId == id && x.EmployeeDepartmentId == employeeDepartmentId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetEmployeeDepartmentById
        // Title: GetEmployeeDepartmentById
        // Description: This method retrieves a employee Department from the database by its unique identifier (`id`). 
        public async Task<EmployeeDepartment> GetEmployeeDepartmentById(string id)
        {
            var query = _employeeDepartmentRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertEmployeeDepartmentList
        // Title: InsertEmployeeDepartmentList
        // Description: This method inserts a new Employee department entity into the repository. It takes a Employee department object as input and uses the _employeeRepository to handle the insertion operation.
        public void InsertEmployeeDepartmentList(IList<EmployeeDepartment> entities)
        {
            _employeeDepartmentRepository.Insert(entities);
        }
        #endregion

        #region UpdateEmployeeDepartmentList
        // Title: UpdateEmployeeDepartmentList
        // Description: This method updates the specified Employee department entity in the repository. It takes a Employee department object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployeeDepartmentList(IList<EmployeeDepartment> entities)
        {
            _employeeDepartmentRepository.Update(entities);
        }
        #endregion

        #region DeleteEmployeeDepartmentList
        // Title: DeleteEmployeeDepartmentList
        // Description: Marks the specified employee department entity as deleted by setting its `Deleted` property to true. 
        public void DeleteEmployeeDepartmentList(List<EmployeeDepartment> entities)
        {
            var list = new List<EmployeeDepartment>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _employeeDepartmentRepository.Update(list);
        }
        #endregion
    }
}
