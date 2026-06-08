using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.EmployeeOrgStructures
{
    public class EmployeeOrgStructureService : IEmployeeOrgStructureService
    {
        #region Define Services
        private readonly IRepository<EmployeeOrgStructure> _employeeOrgStructureRepository;
        #endregion

        #region Services Initializations
        public EmployeeOrgStructureService(IRepository<EmployeeOrgStructure> employeeOrgStructureRepository)
        {
            _employeeOrgStructureRepository = employeeOrgStructureRepository;
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

        #region GetAllEmployeeOrgStructures
        public List<OrgChartPreview> GetOrgStructurePreview(string siteId, int year)
        {
            var list = _employeeOrgStructureRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.Employee.Active &&
                    x.SiteId == siteId &&
                    x.Year == year
                )
                .Select(x => new OrgChartPreview
                {
                    Id = x.EmployeeId,
                    ParentId = x.ManagerId ?? null,
                    Name = string.Concat(x.Employee.Person.FirstName, ' ', x.Employee.Person.LastName) ?? "",
                    //Position = x.Role != null ? x.Role.DropDownValue : "",
                    Position = x.EmployeeOrgStructureDesignationMapping != null &&
                                       x.EmployeeOrgStructureDesignationMapping.Any()
                                ? string.Join(", ",
                                    x.EmployeeOrgStructureDesignationMapping
                                        .Select(m => m.EmployeeDesignation.DropDownValue))
                                : "",
                    Department = x.Department != null ? x.Department.Name : "",
                    Responsibilities = x.Responsibilities ?? "",
                    Color = !string.IsNullOrEmpty(x.Color) ? x.Color : "#999",
                    Image = !string.IsNullOrEmpty(x.Employee.Person.Picture.VirtualPath) ? x.Employee.Person.Picture.VirtualPath : $"https://ui-avatars.com/api/?name={x.Employee.Person.FirstName[0] + " " + x.Employee.Person.LastName[0]}&background=E8EEF9&color=1F3B5B",
                    SortOrder = x.SortOrder ?? 0
                })
                .OrderBy(x => x.SortOrder) // 🔥 important for UI stability
                .ToList();

            return list;
        }

        // Title: GetAllEmployeeOrgStructureList
        // Description: This method retrieves a paginated list of Employee Org Structure based on various search criteria such as employee
        public IPagedList<EmployeeOrgStructure> GetAllEmployeeOrgStructureList(
            string SiteId,
            string SearchText,
            int years,
            string level,
            List<string> departmentIds,
            List<string> employeeDesignationIds,
            List<string> managerIds,
            List<string> employeeIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
            )
        {

            var query = _employeeOrgStructureRepository.TableNoTracking.Where(x => !x.Deleted && x.Employee.Active && x.SiteId == SiteId);

            if (years != 0)
                query = query.Where(x => x.Year == years);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(x => x.Level == int.Parse(level));

            if (departmentIds != null && departmentIds.Any())
                query = query.Where(x => departmentIds.Contains(x.DepartmentId));

            if (employeeDesignationIds != null && employeeDesignationIds.Any())
                query = query.Where(x => x.EmployeeOrgStructureDesignationMapping.Any(m => employeeDesignationIds.Contains(m.EmployeeDesignationId)));

            if (managerIds != null && managerIds.Any())
                query = query.Where(x => managerIds.Contains(x.ManagerId));

            if (employeeIds != null && employeeIds.Any())
                query = query.Where(x => employeeIds.Contains(x.EmployeeId));

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
                int.TryParse(SearchText, out int number);
                query = query.Where(m =>
                   (m.Manager.Person.FirstName + " " + m.Manager.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                   (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.Department.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.EmployeeOrgStructureDesignationMapping.Any(des =>
                    des.EmployeeDesignation.DropDownValue.ToLower().Contains(SearchText.ToLower())) ||
                   m.Level == number ||
                   m.SortOrder == number
                );
            }

             query = query.Select(x => new EmployeeOrgStructure
             {
                Id = x.Id,
                Year = x.Year,
                Level = x.Level,
                SortOrder = x.SortOrder,
                Employee = new Employee
                {
                    Id = x.EmployeeId,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }

                },
                Manager = new Employee
                {
                    Id = x.ManagerId,
                    Person = new Person
                    {
                        Id = x.Manager.Person.Id,
                        FullName = x.Manager.Person.FirstName + " " + x.Manager.Person.LastName,
                    }

                },
                Department = new Department
                {
                    Id = x.Department.Id,
                    Name = x.Department.Name
                },
                EmployeeOrgStructureDesignationMapping = x.EmployeeOrgStructureDesignationMapping.Select(mapping => new EmployeeOrgStructureDesignationMapping
                {
                     Id = mapping.Id,
                     EmployeeDesignationId = mapping.EmployeeDesignationId,
                     EmployeeDesignation = new DropDown
                     {
                         Id = mapping.EmployeeDesignation.Id,
                         DropDownValue = mapping.EmployeeDesignation.DropDownValue
                     }
                }).ToList()
             });

            var list = new PagedList<EmployeeOrgStructure>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetEmployeeOrgStructureById
        // Title: GetEmployeeOrgStructureById
        // Description: This method retrieves a employee org structure from the database by its unique identifier (`id`). 
        public async Task<EmployeeOrgStructure> GetById(string id)
        {
            return await _employeeOrgStructureRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region GetEmployeeOrgStructureDetailsById
        public async Task<EmployeeOrgStructure> GetEmployeeOrgStructureDetailsById(string id)
        {
            var query = _employeeOrgStructureRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new EmployeeOrgStructure
            {
                Id = x.Id,
                DepartmentId = x.DepartmentId,
                ManagerId = x.ManagerId,
                EmployeeId = x.EmployeeId,
                Year = x.Year,
                Level = x.Level,
                Responsibilities = x.Responsibilities,
                Color = x.Color,
                SortOrder = x.SortOrder,
                Employee = new Employee
                {
                    Id = x.EmployeeId,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }

                },
                Manager = new Employee
                {
                    Id = x.ManagerId,
                    Person = new Person
                    {
                        Id = x.Manager.Person.Id,
                        FullName = x.Manager.Person.FirstName + " " + x.Manager.Person.LastName,
                    }

                },
                Department = new Department
                {
                    Id = x.Department.Id,
                    Name = x.Department.Name
                },
                EmployeeOrgStructureDesignationMapping = x.EmployeeOrgStructureDesignationMapping.Select(mapping => new EmployeeOrgStructureDesignationMapping
                {
                    Id = mapping.Id,
                    EmployeeDesignationId = mapping.EmployeeDesignationId,
                    EmployeeDesignation = new DropDown
                    {
                        Id = mapping.EmployeeDesignation.Id,
                        DropDownValue = mapping.EmployeeDesignation.DropDownValue
                    }
                }).ToList()
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetEmployeeOrgStructureByManagerAndEmployee
        // Title: GetEmployeeOrgStructureByManagerAndEmployee
        // Description: This method retrieves a employee org structure based on its name. It allows an optional exclusion of a employee org structure by its ID, which can be useful for scenarios like checking for duplicates.The method returns the first matching employee org structure or null if no match is found.
        public async Task<EmployeeOrgStructure> GetEmployeeOrgStructureByManagerAndEmployee(string SiteId, string managerId, string employeeId, string id = null)
        {
            var query = _employeeOrgStructureRepository.TableNoTracking.Where(x => !x.Deleted && x.ManagerId == managerId && x.EmployeeId == employeeId && x.SiteId == SiteId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetEmployeeOrgStructureByYearAndLevel
        //Title: GetEmployeeOrgStructureByYearAndLevel
        // Description: This method retrieves an employee org structure record based on the specified year and level for a given site.
        // It optionally excludes a specific record by its ID, which is useful during update operations to avoid duplicate validation conflicts.
        // The method returns the first matching record if found; otherwise, it returns null.
        public async Task<EmployeeOrgStructure> GetEmployeeOrgStructureByYearAndLevel(string SiteId, int year, int? level, string id = null)
        {
            var query = _employeeOrgStructureRepository.TableNoTracking.Where(x => !x.Deleted && x.Year == year && x.Level == level && x.SiteId == SiteId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertEmployeeOrgStructure
        // Title: InsertEmployeeOrgStructure
        // Description: This method inserts a new employee org structure entity into the repository. It takes a employee org structure object as input and uses the _employeeOrgStructureRepository to handle the insertion operation.
        public void InsertEmployeeOrgStructure(EmployeeOrgStructure entity)
        {
            _employeeOrgStructureRepository.Insert(entity);
        }
        #endregion

        #region UpdateEmployeeOrgStructure
        // Title: UpdateEmployeeOrgStructure
        // Description: This method updates the specified employee org structure entity in the repository. It takes a employee org structure object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployeeOrgStructure(EmployeeOrgStructure entity)
        {
            _employeeOrgStructureRepository.Update(entity);
        }
        #endregion

        #region DeleteEmployeeOrgStructure
        // Title: DeleteEmployeeOrgStructure
        // Description: Marks the specified employee org structure entity as deleted by setting its `Deleted` property to true.
        public void DeleteEmployeeOrgStructure(EmployeeOrgStructure entity)
        {
            entity.Deleted = true;

            _employeeOrgStructureRepository.Update(entity);
        }
        #endregion
    }
}
