using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.Inventories
{
    public class InventoryService : IInventoryService
    {
        #region Define Services
        private readonly IRepository<Inventory> _inventoryRepository;
        private readonly IRepository<Notes> _notesRepository;
        #endregion

        #region Services Initializations

        public InventoryService(IRepository<Inventory> inventoryRepository, IRepository<Notes> notesRepository)
        {
            _inventoryRepository = inventoryRepository;
            _notesRepository = notesRepository;
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

        #region GetAllInventory
        // Title: GetAllInventory
        // Description: This method retrieves a paginated list of Inventory based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<Inventory> GetAllInventory(string SiteId, string SearchText, List<string> itemTypeIds, string code, List<string> inventoryStatusIds, List<string> employeeIds, List<string> officeLocationIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _inventoryRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (itemTypeIds != null && itemTypeIds.Any())
                query = query.Where(x => itemTypeIds.Contains(x.ItemTypeId));

            if (employeeIds != null && employeeIds.Any())
                query = query.Where(x => x.InventoryAssignmentList.Any(m => !m.Deleted && employeeIds.Contains(m.EmployeeId)));

            if (inventoryStatusIds != null && inventoryStatusIds.Any())
                query = query.Where(x => inventoryStatusIds.Contains(x.InventoryStatusId));
            
            if (officeLocationIds != null && officeLocationIds.Any())
                query = query.Where(x => officeLocationIds.Contains(x.OfficeLocationId));

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(x => x.Inventorycode.Contains(code));

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
                    m.Inventorycode.ToLower().Contains(SearchText.ToLower()) ||
                    m.ItemType.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ModelNameORNumber.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProcessorType.ToLower().Contains(SearchText.ToLower()) ||
                    m.MemoryORRAM.ToLower().Contains(SearchText.ToLower()) ||
                    m.ServiceCode.ToLower().Contains(SearchText.ToLower()) ||
                    m.InventoryAssignmentList.Any(a => (a.Employee.Person.FirstName + " " + a.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.InventoryStatus.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.OfficeLocation.DropDownValue.ToLower().Contains(SearchText.ToLower()))
                    );
            }
             query = query.Select(x => new Inventory
            {
                Id = x.Id,
                Inventorycode = x.Inventorycode,
                InventoryStatusId = x.InventoryStatusId,
                InventoryAssignId = x.InventoryAssignId,
                ServiceCode = x.ServiceCode,
                ProcessorType = x.ProcessorType,
                MemoryORRAM = x.MemoryORRAM,
                ModelNameORNumber = x.ModelNameORNumber,
                WarrantyExpiryDate = x.WarrantyExpiryDate,
                OfficeLocationId = x.OfficeLocationId,
                ItemType = new InventoryItemType
                {
                    Id = x.ItemType.Id,
                    Name = x.ItemType.Name
                },
                InventoryStatus = new DropDown
                {
                    Id = x.InventoryStatus.Id,
                    DropDownValue = x.InventoryStatus.DropDownValue
                },
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                AssignmentType = new DropDown
                {
                    Id = x.AssignmentType.Id,
                    DropDownValue = x.AssignmentType.DropDownValue
                },
                InventoryAssign = new DropDown
                {
                    Id = x.InventoryAssign.Id,
                    DropDownValue = x.InventoryAssign.DropDownValue
                },
                 OfficeLocation = new DropDown
                 {
                     Id = x.OfficeLocation.Id,
                     DropDownValue = x.OfficeLocation.DropDownValue
                 },
                 InventoryAssignmentList = x.InventoryAssignmentList.Where(m => !m.Deleted).OrderBy(m => m.Employee.Person.FirstName).Select(m => new InventoryAssignment
                {
                    Id = m.Id,
                    EmployeeId = m.EmployeeId,
                    Employee = new Employee
                    {
                        Id = m.Employee.Id,
                        Person = new Person
                        {
                            Id = m.Employee.Person.Id,
                            FirstName = m.Employee.Person.FirstName,
                            FullName = m.Employee.Person.FirstName + " " + m.Employee.Person.LastName,
                        },
                    }
                }).ToList(),
                InventoryNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "inventory").Count(),
            });

            var list = new PagedList<Inventory>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetInventoryById
        // Title: GetInventoryById
        // Description: This method retrieves a Inventory from the database by its unique identifier (`id`). 
        public async Task<Inventory> GetInventoryById(string id)
        {
            var query = _inventoryRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInventoryDetailsById
        // Title: GetInventoryDetailsById
        // Description: The method selects relevant fields from the Inventory entity.
        public async Task<Inventory> GetInventoryDetailsById(string id)
        {
            var query = _inventoryRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new Inventory
            {
                Id = x.Id,
                Inventorycode = x.Inventorycode,
                ItemTypeId = x.ItemTypeId,
                InventoryStatusId = x.InventoryStatusId,
                DateofPurchase = x.DateofPurchase,
                Warranty = x.Warranty,
                Guaranty = x.Guaranty,
                InventoryAssignId = x.InventoryAssignId,
                ServiceCode = x.ServiceCode,
                Notes = x.Notes,
                OperatingSystem = x.OperatingSystem,
                ProcessorType = x.ProcessorType,
                MemoryORRAM = x.MemoryORRAM,
                HardDriveORStorageCapacity = x.HardDriveORStorageCapacity,
                GraphicsCard = x.GraphicsCard,
                VirusProtection = x.VirusProtection,
                ModelNameORNumber = x.ModelNameORNumber,
                WarrantyExpiryDate = x.WarrantyExpiryDate,
                OfficeLocationId = x.OfficeLocationId,
                AssignDate = x.AssignDate,
                ReturnDate = x.ReturnDate,
                ReturnReson = x.ReturnReson,
                Description = x.Description,
                CreatedById = x.CreatedById,
                UpdatedById = x.UpdatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                ItemType = new InventoryItemType
                {
                    Id = x.ItemType.Id,
                    Name = x.ItemType.Name
                },
                InventoryStatus = new DropDown
                {
                    Id = x.InventoryStatus.Id,
                    DropDownValue = x.InventoryStatus.DropDownValue
                },
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                AssignmentType = new DropDown
                {
                    Id = x.AssignmentType.Id,
                    DropDownValue = x.AssignmentType.DropDownValue
                },
                InventoryAssign = new DropDown
                {
                    Id = x.InventoryAssign.Id,
                    DropDownValue = x.InventoryAssign.DropDownValue
                },
                OfficeLocation = new DropDown
                {
                    Id = x.OfficeLocation.Id,
                    DropDownValue = x.OfficeLocation.DropDownValue
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    UserName = x.CreatedBy.UserName,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName,
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    UserName = x.UpdatedBy.UserName,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FirstName = x.UpdatedBy.Person.FirstName,
                        LastName = x.UpdatedBy.Person.LastName,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName,
                    }
                },
                InventoryAssignmentList = x.InventoryAssignmentList.Where(m => !m.Deleted).Select(status => new InventoryAssignment
                {
                    Id = status.Id,
                    EmployeeId = status.EmployeeId,
                    AssignDate = status.AssignDate,
                    ReturnDate = status.ReturnDate,
                    ReturnReson = status.ReturnReson,
                    CreatedOnUtc = status.CreatedOnUtc,
                    Employee = new Employee
                    {
                        Id = status.Employee.Id,
                        Person = new Person
                        {
                            Id = status.Employee.Person.Id,
                            FullName = status.Employee.Person.FirstName + " " + status.Employee.Person.LastName,
                        },
                    },
                }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region
        public async Task<Inventory> GetInventoryCode(string siteId,string itemTypeId)
        {
            return await _inventoryRepository.TableNoTracking.Where(x => x.SiteId == siteId &&  x.ItemTypeId == itemTypeId).OrderByDescending(m => m.Inventorycode).FirstOrDefaultAsync();
        }

        #endregion

        #region InsertInventory
        // Title: InsertInventory
        // Description: This method inserts a new Inventory entity into the repository. It takes a Inventory object as input and uses the _adPostRepository to handle the insertion operation.
        public void InsertInventory(Inventory entity)
        {
            _inventoryRepository.Insert(entity);
        }
        #endregion

        #region UpdateInventory
        // Title: UpdateInventory
        // Description: This method updates the specified Inventory entity in the repository. It takes a Inventory object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateInventory(Inventory entity)
        {
            _inventoryRepository.Update(entity);
        }
        #endregion

        #region DeleteInventory
        // Title: DeleteInventory
        // Description: Marks the specified Inventory entity as deleted by setting its `Deleted` property to true. 
        public void DeleteInventory(Inventory entity)
        {
            entity.Deleted = true;

            _inventoryRepository.Update(entity);
        }
        #endregion
    }
}
