using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.Departments
{
    public class DepartmentService : IDepartmentService
    {
        #region Define Services
        private readonly IRepository<Department> _departmentRepository;
        #endregion

        #region Services Initializations
        public DepartmentService(IRepository<Department> departmentRepository)
        {
            _departmentRepository = departmentRepository;
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

        #region Public GetAllDepartments
        // Title: GetAllDepartments
        // Description: This method retrieves a paginated list of departments based on various search criteria such as department name.
        public IPagedList<Department> GetAllDepartments(string SiteId, string SearchText, List<string> departmentIds, string sortBy,
             bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _departmentRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (departmentIds != null && departmentIds.Any())
                query = query.Where(x => departmentIds.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.Name);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(p =>
                    p.Name.ToLower().Contains(SearchText.ToLower()) );
            }

            if (lookup)
            {
                query = query.Select(x => new Department
                {
                    Id = x.Id,
                    Name = x.Name,
                });
            }
            else
            {
                query = query.Select(x => new Department
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Active = x.Active
                });
            }

            var list = new PagedList<Department>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetDepartById
        // Title: GetDepartById
        // Description: This method retrieves a department from the database by its unique identifier (`id`). 
        public async Task<Department> GetById(string id)
        {
            var query = _departmentRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllDepartmentListForDropdown
        public async Task<List<Department>> GetAllDepartmentListForDropdown(string SiteId)
        {
            var query = _departmentRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            var list = await query
                 .OrderBy(x => x.Name)
                 .Select(x => new Department
                    {
                        Id = x.Id,
                        Name = x.Name,
                    })
                     .ToListAsync();

            return list;
        }
        #endregion

        #region GetDepartmentnDetailsById
        // Title: GetDepartmentnDetailsById
        // Description: The method selects relevant fields from the department entity,and returns a `Department` object with these details. 
        public async Task<Department> GetDepartmentDetailsById(string id)
        {
            var query = _departmentRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new Department
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDepartmentById
        // Title: GetDepartmentById
        // Description: This method retrieves a department from the database by its unique identifier (`id`). 
        public async Task<Department> GetDepartmentById(string id)
        {
            var query = _departmentRepository.Table;
            query = query.Where(x => x.Id == id);
            query = query.Where(x => !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDepartmentByName
        // Title: GetDepartmentByName
        // Description: This method retrieves a department based on its name. It allows an optional exclusion of a department by its ID, which can be useful for scenarios like checking for duplicates.The method returns the first matching department or null if no match is found.
        public async Task<Department> GetDepartmentByName(string SiteId, string name, string id = null)
        {
            var query = _departmentRepository.TableNoTracking.Where(x => !x.Deleted && x.Name == name && x.SiteId == SiteId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertDepartment
        // Title: InsertDepartment
        // Description: This method inserts a new department entity into the repository. It takes a department object as input and uses the _departmentRepository to handle the insertion operation.
        public void InsertDepartment(Department entity)
        {
            _departmentRepository.Insert(entity);
        }
        #endregion

        #region UpdateDepartment
        // Title: UpdateDepartment
        // Description: This method updates the specified department entity in the repository. It takes a department object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateDepartment(Department entity)
        {
            _departmentRepository.Update(entity);
        }
        #endregion

        #region DeleteDepartment
        // Title: DeleteDepartment
        // Description: Marks the specified department entity as deleted by setting its `Deleted` property to true.
        public void DeleteDepartment(Department entity)
        {
            entity.Deleted = true;

            _departmentRepository.Update(entity);
        }
        #endregion
    }
}
