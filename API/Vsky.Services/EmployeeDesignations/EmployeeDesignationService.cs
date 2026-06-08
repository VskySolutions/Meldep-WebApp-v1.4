using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmployeeDesignations
{
    public class EmployeeDesignationService : IEmployeeDesignationService
    {
        #region Define Services
        private readonly IRepository<EmployeeDesignation> _employeeDesignationRepository;
        #endregion

        #region Services Initializations
        public EmployeeDesignationService(IRepository<EmployeeDesignation> employeeDesignationRepository)
        {
            _employeeDesignationRepository = employeeDesignationRepository;
        }
        #endregion

        #region GetEmployeeDesignationById
        // Title: GetEmployeeDesignationById
        // Description: This method retrieves a employee Designation from the database by its unique identifier (`id`). 
        public async Task<EmployeeDesignation> GetEmployeeDesignationById(string id)
        {
            var query = _employeeDesignationRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetEmployeeDesignationByEmployeeId
        // Title: GetEmployeeDesignationByEmployeeId
        // Description: This method retrieves a employee Designation from the database by employeeId. 
        public async Task<EmployeeDesignation> GetEmployeeDesignationByEmployeeId(string employeeId)
        {
            var query = await _employeeDesignationRepository.TableNoTracking.Where(x => !x.Deleted && x.EmployeeId == employeeId).OrderByDescending(x => x.StartDate)
            .Select(x => new EmployeeDesignation
            {
                Id = x.Id,
                LeaveApproverId = x.LeaveApproverId,
                LeaveApprover = x.LeaveApprover == null ? null : new Employee
                {
                    Id = x.LeaveApprover.Id,
                    Person = x.LeaveApprover.Person == null ? null : new Person
                    {
                        Id = x.LeaveApprover.Person.Id,
                        FullName = x.LeaveApprover.Person.FirstName + " " + x.LeaveApprover.Person.LastName,
                    }
                }
            }).FirstOrDefaultAsync();

            return query;
        }
        #endregion

        #region InsertEmployeeDesignationList
        // Title: InsertEmployeeDesignationList
        // Description: This method inserts a new Employee Designation entity into the repository. It takes a Employee Designation object as input and uses the _employeeRepository to handle the insertion operation.
        public void InsertEmployeeDesignationList(IList<EmployeeDesignation> entities)
        {
            _employeeDesignationRepository.Insert(entities);
        }
        #endregion

        #region UpdateEmployeeDesignationList
        // Title: UpdateEmployeeDesignationList
        // Description: This method updates the specified Employee designation entity in the repository. It takes a Employee designation object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployeeDesignationList(IList<EmployeeDesignation> entities)
        {
            _employeeDesignationRepository.Update(entities);
        }
        #endregion

        #region DeleteEmployeeDesignationList
        // Title: DeleteEmployeeDesignationList
        // Description: Marks the specified employee designation entity as deleted by setting its `Deleted` property to true. 
        public void DeleteEmployeeDesignationList(List<EmployeeDesignation> entities)
        {
            var list = new List<EmployeeDesignation>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _employeeDesignationRepository.Update(list);
        }
        #endregion
    }
}

