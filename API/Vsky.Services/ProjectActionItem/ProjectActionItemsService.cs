using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Linq.Dynamic.Core;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Services.ProjectActionItem
{
    public class ProjectActionItemsService : IProjectActionItemsService
    {

        #region Define Services
        private readonly IRepository<ProjectActionItems> _projectActionItemsRepository;
        private readonly IRepository<Notes> _notesRepository;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations

        public ProjectActionItemsService(
            IRepository<ProjectActionItems> projectActionItemsRepository,
            IRepository<Notes> notesRepository,
            ICommonService commonService
        )
        {
            _projectActionItemsRepository = projectActionItemsRepository;
            _notesRepository = notesRepository;
            _commonService = commonService;
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

        #region GetAllProjectActionItems
        public async Task<IPagedList<ProjectActionItems>> GetAllProjectActionItems(
            string SiteId,
            string LoggedUserId,
            string SearchText,
            List<string> projectIds,
            List<string> requirementIds,
            List<string> priorityIds,
            string title,
            List<string> customerIds,
            List<string> employeeIds,
            DateTime? dueDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _projectActionItemsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (requirementIds != null && requirementIds.Any())
                query = query.Where(x => requirementIds.Contains(x.RequirementId));

            if (priorityIds != null && priorityIds.Any())
                query = query.Where(x => priorityIds.Contains(x.PriorityId));

            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(title));
            }

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.CustomerId));

            if (employeeIds != null && employeeIds.Any())
                query = query.Where(x => employeeIds.Contains(x.EmployeeId));

            if (dueDate != null)
                query = query.Where(a => a.DueDate == dueDate);

            //if (!string.IsNullOrWhiteSpace(sortBy))
            //{
            //    var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
            //    query = query.OrderBy(orderBy);
            //}
            //else
            //{
            //    query = query.OrderByDescending(x => x.CreatedOnUtc);
            //}


            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                string orderBy;
                if (sortBy == "employee.person.fullName")
                {
                    orderBy = 
                        $"{GetOrderBy("Employee.Person.FirstName")} {(descending ? "desc" : "asc")}," +
                        $"{GetOrderBy("Employee.Person.LastName")} {(descending ? "desc" : "asc")}";
                }
                else if (sortBy == "customer.name")
                {
                    orderBy = 
                        $"{GetOrderBy("Customer.Company.Name")} {(descending ? "desc" : "asc")}," +
                        $"{GetOrderBy("Customer.Person.FirstName")} {(descending ? "desc" : "asc")}," +
                        $"{GetOrderBy("Customer.Person.LastName")} {(descending ? "desc" : "asc")}";
                }
                else
                {
                    orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                }
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                       m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                       m.Requirement.Title.ToLower().Contains(SearchText.ToLower()) ||
                       m.Priority.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                       m.Title.ToLower().Contains(SearchText.ToLower()) ||
                        (m.Customer.Company.Employee.Person.FirstName.Contains(SearchText.ToLower()) || m.Customer.Company.Employee.Person.LastName.Contains(SearchText.ToLower())) ||
                        m.Customer.Company.Name.Contains(SearchText.ToLower()) ||
                        (m.Employee.Person.FirstName.Contains(SearchText.ToLower()) || m.Employee.Person.LastName.Contains(SearchText.ToLower())) ||
                       m.Description.ToLower().Contains(SearchText.ToLower()) ||
                       m.DueDate == parsedDate.Date
                );
            }

            // Apply multi-level dictionary sorting
            //if (sorts != null && sorts.Count > 0)
            //{
            //    query = _commonService.ApplySorting(query, sorts);
            //}
            if (sorts != null && sorts.Count > 0)
            {
                // Customer sorting
                if (sorts.TryGetValue("customer.name", out var customerDirection))
                {
                    bool customerDesc = customerDirection == "desc";

                    query = customerDesc
                        ? query.OrderByDescending(x =>
                            x.Customer.Company != null
                                ? x.Customer.Company.Name
                                : x.Customer.Person.FirstName + " " +
                                  x.Customer.Person.LastName)
                        : query.OrderBy(x =>
                            x.Customer.Company != null
                                ? x.Customer.Company.Name
                                : x.Customer.Person.FirstName + " " +
                                  x.Customer.Person.LastName);

                    // Remove from generic sorting
                    sorts.Remove("customer.name");
                }

                // Employee sorting
                if (sorts.TryGetValue("employee.person.fullName", out var employeeDirection))
                {
                    bool employeeDesc = employeeDirection == "desc";

                    query = employeeDesc
                             ? query.OrderByDescending(x => x.Employee.Person.FirstName)
                                 .ThenByDescending(x => x.Employee.Person.LastName)
                             : query.OrderBy(x => x.Employee.Person.FirstName)
                                 .ThenBy(x => x.Employee.Person.LastName);

                    // Remove from generic sorting
                    sorts.Remove("employee.person.fullName");
                }

                if (sorts.Any())
                {
                    query = _commonService.ApplySorting(query, sorts);
                }
            }

            var notesQuery = _notesRepository.TableNoTracking.Where(n => !n.Deleted && n.Type == "ProjectActionItems");
            query = query.Select(x => new ProjectActionItems
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                RequirementId = x.RequirementId,
                PriorityId = x.PriorityId,
                Title = x.Title,
                Description = x.Description,
                CustomerId = x.CustomerId,
                EmployeeId = x.EmployeeId,
                DueDate = x.DueDate,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                Requirement = new Requirement
                {
                    Id = x.Requirement.Id,
                    Title = x.Requirement.Title
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName
                    }
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Name = x.Customer.Company != null ? x.Customer.Company.Name : string.Join(" ", x.Customer.Person.FirstName, x.Customer.Person.LastName).Trim(),
                    PersonId = x.Customer.PersonId,
                    CompanyId = x.Customer.CompanyId
                },
                CreatedBy = new ApplicationUser
                {
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName,
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName,
                    }
                },
                ProjectActionItemNotesCount = notesQuery.Where(m => m.SubModuleId == x.Id).Count()
            });

            var list = new PagedList<ProjectActionItems>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetProjectActionItemById
        public async Task<ProjectActionItems> GetProjectActionItemById(string id)
        {
            var query = _projectActionItemsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectActionItemDetailsById
        public async Task<ProjectActionItems> GetProjectActionItemDetailsById(string id)
        {
            var query = _projectActionItemsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new ProjectActionItems
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                RequirementId = x.RequirementId,
                PriorityId = x.PriorityId,
                Title = x.Title,
                Description = x.Description,
                EmployeeId = x.EmployeeId,
                CustomerId = x.CustomerId,
                DueDate = x.DueDate,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                Requirement = new Requirement
                {
                    Id = x.Requirement.Id,
                    Title = x.Requirement.Title
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName
                    }
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Name = x.Customer.Company != null ? x.Customer.Company.Name : string.Join(" ", x.Customer.Person.FirstName, x.Customer.Person.LastName).Trim(),
                    PersonId = x.Customer.PersonId,
                    CompanyId = x.Customer.CompanyId
                },
                CreatedBy = new ApplicationUser
                {
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName,
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName,
                    }
                },
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertProjectActionItems
        public void InsertProjectActionItems(ProjectActionItems entity)
        {
            _projectActionItemsRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectActionItems
        public void UpdateProjectActionItems(ProjectActionItems entity)
        {
            _projectActionItemsRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectActionItems
        public void DeleteProjectActionItems(ProjectActionItems entity)
        {
            entity.Deleted = true;

            _projectActionItemsRepository.Update(entity);
        }
        #endregion
    }
}
