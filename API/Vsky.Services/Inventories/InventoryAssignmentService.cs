using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Inventories
{
    public class InventoryAssignmentService : IInventoryAssignmentService
    {
        #region Define Services
        private readonly IRepository<InventoryAssignment> _inventoryAssignmentRepository;
        #endregion

        #region Services Initializations
        public InventoryAssignmentService(IRepository<InventoryAssignment> inventoryAssignmentRepository)
        {
            _inventoryAssignmentRepository = inventoryAssignmentRepository;
        }
        #endregion

        #region GetInventoryAssignmentById
        // Title: GetInventoryAssignmentById
        // Description: This method retrieves a InventoryAssignment from the database by its unique identifier (`id`). 
        public async Task<InventoryAssignment> GetInventoryAssignmentById(string id)
        {
            var query = _inventoryAssignmentRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInventoryAssignmentsByInventoryId
        // Title: GetInventoryAssignmentsByInventoryId
        // Description: The method selects relevant fields from the InventoryAssignment entity, including related entities such as project module and project task, and returns a `InventoryAssignment` object with these details. 
        public async Task<List<InventoryAssignment>> GetInventoryAssignmentsByInventoryId(string inventoryId)
        {
            var query = _inventoryAssignmentRepository.TableNoTracking.Where(x => !x.Deleted && x.InventoryId == inventoryId).Select(x => new InventoryAssignment
            {
                Id = x.Id,
                AssignDate = x.AssignDate,
                ReturnDate = x.ReturnDate,
                ReturnReson = x.ReturnReson,
                CreatedOnUtc = x.CreatedOnUtc,
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                }
            });
            var item = await query.ToListAsync();
            return item;
        }
        #endregion

        #region GetInventoryAssignmentByInventoryId
        // Title: GetInventoryAssignmentByInventoryId
        // Description: This method retrieves a InventoryAssignment based on its name and Id. It allows an optional exclusion of a InventoryAssignment by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific InventoryAssignment. The method returns the first matching InventoryAssignment or null if no match is found.
        public async Task<InventoryAssignment> GetInventoryAssignmentByInventoryId(string employeeId, string inventoryId)
        {
            var query = _inventoryAssignmentRepository.TableNoTracking.Where(x => !x.Deleted && x.EmployeeId == employeeId && x.InventoryId == inventoryId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertInventoryAssignmentList
        public void InsertInventoryAssignmentList(IList<InventoryAssignment> entities)
        {
            _inventoryAssignmentRepository.Insert(entities);
        }
        #endregion

        #region UpdateInventoryAssignmentList
        // Title: UpdateInventoryAssignmentList
        // Description: This method updates the specified InventoryAssignment entity in the repository. It takes a InventoryAssignment object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateInventoryAssignmentList(IList<InventoryAssignment> entities)
        {
            _inventoryAssignmentRepository.Update(entities);
        }
        #endregion

        #region DeleteInventoryAssignmentList
        // Title: DeleteInventoryAssignmentList
        // Description: Marks the specified InventoryAssignment entity as deleted by setting its `Deleted` property to true. 
        public void DeleteInventoryAssignmentList(List<InventoryAssignment> entities)
        {
            var list = new List<InventoryAssignment>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _inventoryAssignmentRepository.Update(list);
        }
        #endregion
    }
}

