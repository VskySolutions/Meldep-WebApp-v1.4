using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using Org.BouncyCastle.Bcpg.Sig;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.Timesheets
{
    public class TimesheetService : ITimesheetService
    {
        #region Define Services
        private readonly IRepository<Timesheet> _timesheetRepository;
        private readonly IRepository<TimesheetLines> _timesheetLinerepository;
        #endregion

        #region Services Initializations
        public TimesheetService(IRepository<Timesheet> timesheetRepository, IRepository<TimesheetLines> timesheetLinerepository)
        {
            _timesheetRepository = timesheetRepository;
            _timesheetLinerepository = timesheetLinerepository;
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

        #region GetAllTimesheets
        // Title: GetAllTimesheets
        // Description: This method retrieves a paginated list of timesheets based on various search criteria such as project name, 
        // project module,task,employee.The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<Timesheet> GetAllTimesheets(
            string SiteId, 
            string createdBy, 
            string SearchText, 
            string employeeId, 
            string projectId, 
            string projectModuleId, 
            string projectTaskId, 
            string projectActivityId, 
            DateTime? activityDate, 
            DateTime? fromDate, 
            DateTime? toDate,
            bool thisWeek,
            int lastNumberOfWeeks,
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
        )
        {
            var query = _timesheetRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(x => x.CreatedById == createdBy);
            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.EmployeeId == employeeId);

            // Filter to only include Timesheets with associated TimesheetLines, and optionally filter by projectId if provided
            query = query.Where(x => x.TimesheetLines.Any(line => !line.Deleted &&
                (string.IsNullOrWhiteSpace(projectId) || line.ProjectId == projectId)));

            //Search by Date 
            if (activityDate != null)
                query = query.Where(x => x.TimesheetDate == activityDate);
            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.TimesheetDate >= fromDate);
            if (toDate != null)
                query = query.Where(a => a.TimesheetDate <= toDate);

            // Search by projectId in TimesheetLines if provided
            if (!string.IsNullOrWhiteSpace(projectId))
            {
                query = query.Where(x => x.TimesheetLines.Any(m => m.ProjectId == projectId));
            }

            if (!string.IsNullOrWhiteSpace(projectModuleId))
            {
                query = query.Where(x => x.TimesheetLines.Any(m => m.ProjectModuleId == projectModuleId));
            }

            if (!string.IsNullOrWhiteSpace(projectTaskId))
            {
                query = query.Where(x => x.TimesheetLines.Any(m => m.ProjectTaskId == projectTaskId));
            }

            if (!string.IsNullOrWhiteSpace(projectActivityId))
            {
                query = query.Where(x => x.TimesheetLines.Any(m => m.ProjectActivityId == projectActivityId));
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.TimesheetLines.Any(line => line.Project.Name.ToLower().Contains(SearchText.ToLower())) ||
                    m.TimesheetLines.Any(line => line.Task.Name.ToLower().Contains(SearchText.ToLower())) ||
                    m.TimesheetLines.Any(line => line.ProjectModule.Name.ToLower().Contains(SearchText.ToLower())) ||
                    m.TimesheetLines.Any(line => line.ProjectActivity.Name.ToLower().Contains(SearchText.ToLower())) ||
                    m.TimesheetLines.Any(line => line.Description.ToLower().Contains(SearchText.ToLower())) ||
                    (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.TimesheetLines.Any(line => line.Hours.ToString().Contains(SearchText.ToLower())) ||
                    m.TimesheetDate.Value.Date == parsedDate.Date
                );
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                //var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                //query = query.OrderBy(orderBy);
                //query = query.Where(x => x.TimesheetLines.Any()).OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.TimesheetDate);
            }

            query = query.OrderByDescending(x => x.TimesheetDate).Select(x => new Timesheet
            {

                Id = x.Id,
                TimesheetDate = x.TimesheetDate,
                IsActionVisible = thisWeek && x.TimesheetDate >= DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.Date.DayOfWeek).AddDays(-(7 * lastNumberOfWeeks)),
                Sites = new Site
                {
                    Id = x.Sites.Id,
                    Name = x.Sites.Name
                },
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    UserName = x.User.UserName,
                    Person = new Person
                    {
                        Id = x.User.PersonId,
                        FirstName = x.User.Person.FirstName,
                        LastName = x.User.Person.LastName,
                        FullName = x.User.Person.FirstName + " " + x.User.Person.LastName,
                    }
                },
                TimesheetLines = x.TimesheetLines.Where(m => !m.Deleted && (string.IsNullOrWhiteSpace(projectId) || m.ProjectId == projectId) && (string.IsNullOrWhiteSpace(projectModuleId) || m.ProjectModuleId == projectModuleId) && (string.IsNullOrWhiteSpace(projectTaskId) || m.ProjectTaskId == projectTaskId)).Select(mapping => new TimesheetLines
                {
                    Id = mapping.Id,
                    Hours = mapping.Hours,
                    Description = mapping.Description,
                    Project = new Project
                    {
                        Id = mapping.Project.Id,
                        Name = mapping.Project.Name,
                        
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = mapping.ProjectModule.Id,
                        Name = mapping.ProjectModule.Name,
                    },
                    Task = new ProjectTask
                    {
                        Id = mapping.Task.Id,
                        Name = mapping.Task.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name,
                        Active = mapping.ProjectActivity != null ? mapping.ProjectActivity.Active : false,
                        ActivityStatus = new DropDown
                        {
                            DropDownValue = mapping.ProjectActivity.ActivityStatus.DropDownValue,
                        }
                    }
                }).ToList(),
            });

            var list = new PagedList<Timesheet>(query, page, pageSize);
            return list;
        }

        public IPagedList<TimesheetLines> GetAllBillingTimesheets(
            string SiteId, 
            string SearchText, 
            DateTime? fromDate, 
            DateTime? toDate, 
            string projectId, 
            List<string> projectModuleIds, 
            List<string> projectTaskIds, 
            List<string> customerIds, 
            List<string> companyContactIds, 
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = 
            int.MaxValue, 
            bool lookup = false
        )
        {
            var query = _timesheetLinerepository.TableNoTracking.Where(x => !x.Deleted && !x.Timesheet.Deleted && x.Timesheet.SiteId == SiteId);

            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.Timesheet.TimesheetDate >= fromDate);

            if (toDate != null)
                query = query.Where(x => x.Timesheet.TimesheetDate <= toDate);

            // Search by projectId in TimesheetLines if provided
            if (!string.IsNullOrWhiteSpace(projectId))
            {
                query = query.Where(x => x.ProjectId == projectId);
            }

            if (projectModuleIds != null && projectModuleIds.Any())
            {
                query = query.Where(x => projectModuleIds.Contains(x.ProjectModuleId));
            }

            if (projectTaskIds != null && projectTaskIds.Any())
            {
                query = query.Where(x => projectTaskIds.Contains(x.ProjectTaskId));
            }

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.Project.CustomerId));
                //query = query.Where(x => customerIds.Contains(x.Project.CustomerId));

            if (companyContactIds != null && companyContactIds.Any())
                query = query.Where(x => companyContactIds.Contains(x.Project.CompanyContactId));
            
            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Task.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProjectModule.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProjectActivity.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Description.ToLower().Contains(SearchText.ToLower()) ||
                    m.Hours.ToString().Contains(SearchText.ToLower()) ||
                    m.BillableHours.ToString().Contains(SearchText.ToLower()) ||
                    (m.Timesheet.Employee.Person.FirstName + " " + m.Timesheet.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.Timesheet.TimesheetDate.Value.Date == parsedDate.Date
                );
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                //var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                //query = query.OrderBy(orderBy);
                //query = query.Where(x => x.TimesheetLines.Any()).OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.Timesheet.TimesheetDate);
            }

            // Select required fields
            query = query.Select(x => new TimesheetLines
            {
                Id = x.Id,
                Hours = x.Hours,
                BillableHours = x.BillableHours,
                Description = string.IsNullOrEmpty(x.Description) ? string.Empty : x.Description,
                BillableCreatedOnUtc = x.BillableCreatedOnUtc,
                Timesheet = new Timesheet
                {
                    Id = x.Timesheet.Id,
                    TimesheetDate = x.Timesheet.TimesheetDate,
                    Sites = new Site
                    {
                        Id = x.Timesheet.Sites.Id,
                        Name = x.Timesheet.Sites.Name
                    },
                    Employee = new Employee
                    {
                        Person = new Person
                        {
                            Id = x.Timesheet.Employee.Person.Id,
                            FullName = x.Timesheet.Employee.Person.FirstName + " " + x.Timesheet.Employee.Person.LastName,
                        }
                    },
                    User = new ApplicationUser
                    {
                        Id = x.Timesheet.User.Id,
                        UserName = x.Timesheet.User.UserName,
                        Person = new Person
                        {
                            Id = x.Timesheet.User.PersonId,
                            FirstName = x.Timesheet.User.Person.FirstName,
                            LastName = x.Timesheet.User.Person.LastName,
                            FullName = x.Timesheet.User.Person.FirstName + " " + x.Timesheet.User.Person.LastName,
                        }
                    }
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    CompanyContact = new CompanyContacts
                    {
                        Id = x.Project.CompanyContact.Id,
                        Person = new Person
                        {
                            Id = x.Project.CompanyContact.Person.Id,
                            FullName = x.Project.CompanyContact.Person.FirstName + " " + x.Project.CompanyContact.Person.LastName
                        }
                    },
                    Customer = new CompanyClients
                    {
                        Id = x.Project.Customer.Id,
                        Company = new Company
                        {
                            Id = x.Project.Customer.Company.Id,
                            Name = x.Project.Customer.Company.Name,
                        }
                    },
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name,
                },
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                },
                ProjectActivity = new ProjectActivity
                {
                    Id = x.ProjectActivity.Id,
                    Name = x.ProjectActivity.Name
                },
                BillableCreatedBy = new ApplicationUser
                {
                    Id = x.BillableCreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.BillableCreatedBy.PersonId,
                        FirstName = x.BillableCreatedBy.Person.FirstName,
                        LastName = x.BillableCreatedBy.Person.LastName,
                        FullName = x.BillableCreatedBy.Person.FirstName + " " + x.BillableCreatedBy.Person.LastName
                    }
                },
            });

            var list = new PagedList<TimesheetLines>(query, page, pageSize);
            return list;
        }

        public IPagedList<TimesheetLines> GetGroupedBillingTimesheetsByEmployee(
            string SiteId, 
            string SearchText, 
            DateTime? fromDate, 
            DateTime? toDate, 
            string projectId, 
            List<string> projectModuleIds, 
            List<string> projectTaskIds, 
            List<string> customerIds, 
            List<string> companyContactIds, 
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
        )
        {
            var query = _timesheetLinerepository.TableNoTracking.Where(x => !x.Deleted && !x.Timesheet.Deleted && x.Timesheet.SiteId == SiteId);

            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.Timesheet.TimesheetDate >= fromDate);

            if (toDate != null)
                query = query.Where(x => x.Timesheet.TimesheetDate <= toDate);

            // Search by projectId in TimesheetLines if provided
            if (!string.IsNullOrWhiteSpace(projectId))
            {
                query = query.Where(x => x.ProjectId == projectId);
            }

            if (projectModuleIds != null && projectModuleIds.Any())
            {
                query = query.Where(x => projectModuleIds.Contains(x.ProjectModuleId));
            }

            if (projectTaskIds != null && projectTaskIds.Any())
            {
                query = query.Where(x => projectTaskIds.Contains(x.ProjectTaskId));
            }

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.Project.CustomerId));

            if (companyContactIds != null && companyContactIds.Any())
                query = query.Where(x => companyContactIds.Contains(x.Project.CompanyContactId));

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Task.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProjectModule.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProjectActivity.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Description.ToLower().Contains(SearchText.ToLower()) ||
                    m.Hours.ToString().Contains(SearchText.ToLower()) ||
                    m.BillableHours.ToString().Contains(SearchText.ToLower()) ||
                    (m.Timesheet.Employee.Person.FirstName + " " + m.Timesheet.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.Timesheet.TimesheetDate.Value.Date == parsedDate.Date
                );
            }

            query = query.GroupBy(x => new
            {
                x.Timesheet.EmployeeId,
                x.ProjectId,
                x.ProjectModuleId,
                x.ProjectTaskId
            })
            .Select(g => new TimesheetLines
            {
                Hours = g.Sum(x => x.Hours),
                Id = g.FirstOrDefault().Id,
                BillableHours = g.FirstOrDefault().BillableHours,
                BillableCreatedOnUtc = g.FirstOrDefault().BillableCreatedOnUtc,
                TimesheetId = g.FirstOrDefault().TimesheetId,
                Timesheet = new Timesheet
                {
                    Id = g.FirstOrDefault().Timesheet.Id,
                    TimesheetDate = g.FirstOrDefault().Timesheet.TimesheetDate,
                    EmployeeId = g.Key.EmployeeId,
                    //Employee = new Employee
                    //{
                    //    Id = g.FirstOrDefault().Timesheet.Employee.Id,
                    //    Person = new Person
                    //    {
                    //        Id = g.FirstOrDefault().Timesheet.Employee.Person.Id,
                    //        FullName = g.FirstOrDefault().Timesheet.Employee.Person.FirstName + " " +
                    //                       g.FirstOrDefault().Timesheet.Employee.Person.LastName
                    //    }
                    //},
                    User = new ApplicationUser
                    {
                        Id = g.FirstOrDefault().Timesheet.User.Id,
                        UserName = g.FirstOrDefault().Timesheet.User.UserName,
                        Person = new Person
                        {
                            FullName = g.FirstOrDefault().Timesheet.User.Person.FirstName + " " + g.FirstOrDefault().Timesheet.User.Person.LastName
                        }
                    }
                },
                Project = new Project
                {
                    Id = g.FirstOrDefault().Project.Id,
                    Name = g.FirstOrDefault().Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = g.FirstOrDefault().ProjectModule.Id,
                    Name = g.FirstOrDefault().ProjectModule.Name
                },
                Task = new ProjectTask
                {
                    Id = g.FirstOrDefault().Task.Id,
                    Name = g.FirstOrDefault().Task.Name
                },
                BillableCreatedBy = new ApplicationUser
                {
                    Id = g.FirstOrDefault().BillableCreatedBy.Id,
                    Person = new Person
                    {
                        Id = g.FirstOrDefault().BillableCreatedBy.PersonId,
                        FirstName = g.FirstOrDefault().BillableCreatedBy.Person.FirstName,
                        LastName = g.FirstOrDefault().BillableCreatedBy.Person.LastName,
                        FullName = g.FirstOrDefault().BillableCreatedBy.Person.FirstName + " " + g.FirstOrDefault().BillableCreatedBy.Person.LastName
                    }
                },
            });

            // Paging manually
            var pagedList = new PagedList<TimesheetLines>(query, page, pageSize);
            return pagedList;
        }

        public async Task<List<Timesheet>> GetAllTimesheetByWeek(string siteId, string employeeId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _timesheetRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.EmployeeId == employeeId && x.TimesheetDate >= fromDate && x.TimesheetDate <= toDate);

            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new Timesheet
            {
                Id = x.Id,
                TimesheetDate = x.TimesheetDate,
                TimesheetLines = x.TimesheetLines.Where(m => !m.Deleted).Select(mapping => new TimesheetLines
                {
                    Id = mapping.Id,
                    Hours = mapping.Hours,
                    Description = mapping.Description,
                    TimesheetId = mapping.TimesheetId,
                    ProjectTaskId = mapping.ProjectTaskId,
                    ProjectId = mapping.ProjectId,
                    Project = new Project
                    {
                        Id = mapping.Project.Id,
                        Name = mapping.Project.Name,

                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = mapping.ProjectModule.Id,
                        Name = mapping.ProjectModule.Name,
                    },
                    Task = new ProjectTask
                    {
                        Id = mapping.Task.Id,
                        Name = mapping.Task.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name
                    }
                }).ToList(),
            });

            return query.ToList();
        }
        #endregion

        #region GetTimesheetById
        // Title: GetTimesheetById
        // Description: This method retrieves a timesheet from the database by its unique identifier (`id`). 
        public async Task<Timesheet> GetTimesheetById(string id)
        {
            var query = _timesheetRepository.Table;

            query = query.Where(x => !x.Deleted);

            query = query.Where(x => x.Id == id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetTimesheetByDate
        // Title: GetTimesheetByDate
        // Description: This method retrieves a timesheet based on its name and date. It allows an optional exclusion of a timesheet by its ID, which can be useful for scenarios like checking for duplicates.
        public async Task<Timesheet> GetTimesheetByDate(string SiteId, string LoggedUserId, DateTime? TimesheetDate, string id = null)
        {
            var query = _timesheetRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrEmpty(LoggedUserId))
                query = query.Where(x => x.CreatedById == LoggedUserId);

            if (TimesheetDate != null)
                query = query.Where(x => x.TimesheetDate == TimesheetDate);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllTimesheetListForDropdown
        public async Task<List<Timesheet>> GetAllTimesheetListForDropdown(string SiteId)
        {
            var query = _timesheetRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId);
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetTimesheetDetailsById
        // Title: GetTimesheetDetailsById
        // Description: The method selects relevant fields from the timesheet entity, including related entities such as project module and project task, and returns a `Timesheet` object with these details. 
        public async Task<Timesheet> GetTimesheetDetailsById(string id)
        {
            var query = _timesheetRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new Timesheet
            {
                Id = x.Id,
                TimesheetDate = x.TimesheetDate,
                SiteId = x.SiteId,
                EmployeeId = x.EmployeeId,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                Sites = new Site
                {
                    Id = x.Sites.Id,
                    Name = x.Sites.Name
                },
                TimesheetLines = x.TimesheetLines.Where(m => !m.Deleted).Select(mapping => new TimesheetLines
                {
                    Id = mapping.Id,
                    Hours = mapping.Hours,
                    Description = mapping.Description,
                    ProjectActivityId = mapping.ProjectActivityId,
                    Project = new Project
                    {
                        Id = mapping.Project.Id,
                        Name = mapping.Project.Name,
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = mapping.ProjectModule.Id,
                        Name = mapping.ProjectModule.Name,
                    },
                    Task = new ProjectTask
                    {
                        Id = mapping.Task.Id,
                        Name = mapping.Task.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name,
                        TargetMonth = mapping.ProjectActivity.TargetMonth,
                    }
                }).ToList()
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTimesheet
        // Title: InsertTimesheet
        // Description: This method inserts a new Timesheet entity into the repository. It takes a Timesheet object as input and uses the _timesheetRepository to handle the insertion operation.
        public void InsertTimesheet(Timesheet entity)
        {
            _timesheetRepository.Insert(entity);
        }
        #endregion

        #region UpdateTimesheet
        // Title: UpdateTimesheet
        // Description: This method updates the specified Timesheet entity in the repository. It takes a Timesheet object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTimesheet(Timesheet entity)
        {
            _timesheetRepository.Update(entity);
        }
        #endregion

        #region DeleteTimesheet
        // Title: DeleteTimesheet
        // Description: Marks the specified timesheet entity as deleted by setting its `Deleted` property to true. 
        public void DeleteTimesheet(Timesheet entity)
        {
            entity.Deleted = true;
            _timesheetRepository.Update(entity);
        }
        #endregion

        #region GetAllTimesheets
        // Title: GetAllTimesheets
        // Description: This method retrieves a paginated list of timesheets based on various search criteria such as project name, 
        // project module,task,employee.The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<Timesheet> GetAllTimesheetsForDashboard(string SiteId, string createdBy, string employeeId, string projectId, string projectModuleId, string projectTaskId, string projectActivityId, DateTime? activityDate, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = 5, bool lookup = false)
        {
            var query = _timesheetRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(x => x.CreatedById == createdBy);
            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.EmployeeId == employeeId);

            // Filter to only include Timesheets with associated TimesheetLines, and optionally filter by projectId if provided
            query = query.Where(x => x.TimesheetLines.Any(line => !line.Deleted &&
                (string.IsNullOrWhiteSpace(projectId) || line.ProjectId == projectId)));

            //Search by Date 
            if (activityDate != null)
                query = query.Where(x => x.TimesheetDate == activityDate);
            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.TimesheetDate >= fromDate);
            if (toDate != null)
                query = query.Where(a => a.TimesheetDate <= toDate);

            // Search by projectId in TimesheetLines if provided
            if (!string.IsNullOrWhiteSpace(projectId))
            {
                query = query.Where(x => x.TimesheetLines.Any(m => m.ProjectId == projectId));
            }

            if (!string.IsNullOrWhiteSpace(projectModuleId))
            {
                query = query.Where(x => x.TimesheetLines.Any(m => m.ProjectModuleId == projectModuleId));
            }

            if (!string.IsNullOrWhiteSpace(projectTaskId))
            {
                query = query.Where(x => x.TimesheetLines.Any(m => m.ProjectTaskId == projectTaskId));
            }

            if (!string.IsNullOrWhiteSpace(projectActivityId))
            {
                query = query.Where(x => x.TimesheetLines.Any(m => m.ProjectActivityId == projectActivityId));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy); // Requires System.Linq.Dynamic.Core
            }
            else
            {
                query = query.OrderByDescending(x => x.TimesheetDate);
            }

            query = query.OrderByDescending(x => x.TimesheetDate).Take(2)
            .Select(x => new Timesheet
            {
                Id = x.Id,
                TimesheetDate = x.TimesheetDate,
                Sites = new Site
                {
                    Id = x.Sites.Id,
                    Name = x.Sites.Name
                },
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    UserName = x.User.UserName,
                    Person = new Person
                    {
                        Id = x.User.PersonId,
                        FirstName = x.User.Person.FirstName,
                        LastName = x.User.Person.LastName,
                        FullName = x.User.Person.FirstName + " " + x.User.Person.LastName,
                    }
                },
                TimesheetLines = !string.IsNullOrWhiteSpace(projectId) ? x.TimesheetLines.Where(m => !m.Deleted && m.ProjectId == projectId).Select(mapping => new TimesheetLines
                {
                    Id = mapping.Id,
                    Hours = mapping.Hours,
                    Description = mapping.Description,
                    Project = new Project
                    {
                        Id = mapping.Project.Id,
                        Name = mapping.Project.Name,
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = mapping.ProjectModule.Id,
                        Name = mapping.ProjectModule.Name,
                    },
                    Task = new ProjectTask
                    {
                        Id = mapping.Task.Id,
                        Name = mapping.Task.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name
                    }
                }).ToList()
                : x.TimesheetLines.Where(m => !m.Deleted).Select(mapping => new TimesheetLines
                {
                    Id = mapping.Id,
                    Hours = mapping.Hours,
                    Description = mapping.Description,
                    Project = new Project
                    {
                        Id = mapping.Project.Id,
                        Name = mapping.Project.Name,
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = mapping.ProjectModule.Id,
                        Name = mapping.ProjectModule.Name,
                    },
                    Task = new ProjectTask
                    {
                        Id = mapping.Task.Id,
                        Name = mapping.Task.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name
                    }
                }).ToList(),
            });

            var list = new PagedList<Timesheet>(query, page, pageSize);
            return list;
        }
        #endregion
    }
}

