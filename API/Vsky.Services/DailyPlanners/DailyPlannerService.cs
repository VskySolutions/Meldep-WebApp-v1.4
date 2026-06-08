using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;

namespace Vsky.Services.DailyPlanners
{
    public class DailyPlannerService : IDailyPlannerService
    {
        #region Define Services
        private readonly IRepository<DailyPlanner> _dailyPlannerRepository;
        private readonly IRepository<DailyPlannerLine> _dailyPlannerLineRepository;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations
        public DailyPlannerService(
            IRepository<DailyPlanner> dailyPlannerRepository,
            IRepository<DailyPlannerLine> dailyPlannerLineRepository,
            ICommonService commonService)
        {
            _dailyPlannerRepository = dailyPlannerRepository;
            _dailyPlannerLineRepository = dailyPlannerLineRepository;
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

        #region GetAllDailyPlanner
        // Title: GetAllDailyPlanner
        // Description: This method retrieves a paginated list of daily planner based on various search criteria such as name, 
        // project name. The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<DailyPlanner> GetAllDailyPlanner(string SiteId, string createdBy, string SearchText, string employeeId, string projectId, DateTime? activityDate, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _dailyPlannerRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(x => x.CreatedById == createdBy);
            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.EmployeeId == employeeId);

            // Filter to only include DailyPlans with associated DailyPlannerLines, and optionally filter by projectId if provided
            query = query.Where(x => x.DailyPlannerLines.Any(line => !line.Deleted &&
                (string.IsNullOrWhiteSpace(projectId) || line.ProjectId == projectId)));

            //Search by Date 
            if (activityDate != null)
                query = query.Where(x => x.DailyPlannerDate == activityDate);

            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.DailyPlannerDate >= fromDate);
            if (toDate != null)
                query = query.Where(a => a.DailyPlannerDate <= toDate);

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.DailyPlannerLines.Any(line => line.Project.Name.ToLower().Contains(SearchText.ToLower())) ||
                    m.DailyPlannerLines.Any(line => line.ProjectTask.Name.ToLower().Contains(SearchText.ToLower())) ||
                    m.DailyPlannerLines.Any(line => line.ProjectModule.Name.ToLower().Contains(SearchText.ToLower())) ||
                    m.DailyPlannerLines.Any(line => line.ProjectActivity.Name.ToLower().Contains(SearchText.ToLower())) ||
                    m.DailyPlannerLines.Any(line => line.Description.ToLower().Contains(SearchText.ToLower())) ||
                    (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.DailyPlannerLines.Any(line => line.Hours.ToString().Contains(SearchText.ToLower())) ||
                    (m.DailyPlannerDate.Value.Date == parsedDate.Date)
                );
            }

            query = query.OrderByDescending(x => x.DailyPlannerDate).Select(x => new DailyPlanner
            {
                Id = x.Id,
                DailyPlannerDate = x.DailyPlannerDate,
                IsForwordedToTimesheet = x.IsForwordedToTimesheet,
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
                DailyPlannerLines = x.DailyPlannerLines.Where(m => !m.Deleted && (string.IsNullOrWhiteSpace(projectId) || m.ProjectId == projectId)).Select(mapping => new DailyPlannerLine
                {
                    Id = mapping.Id,
                    Hours = mapping.Hours,
                    Description = mapping.Description,
                    UpdatedOnUtc = mapping.UpdatedOnUtc,
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
                    ProjectTask = new ProjectTask
                    {
                        Id = mapping.ProjectTask.Id,
                        Name = mapping.ProjectTask.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name
                    }
                }).ToList(),
                //TotalHours = x.DailyPlannerLines.Where(m => !m.Deleted).Sum(mapping => mapping.Hours)

            });

            var list = new PagedList<DailyPlanner>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetDailyPlannerDetailsById
        // Title: GetDailyPlannerDetailsById
        // Description: The method selects relevant fields from the daily planner entity, and returns a `Daily Planner` object with these details. 
        public async Task<DailyPlanner> GetDailyPlannerDetailsById(string id)
        {
            var query = _dailyPlannerRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new DailyPlanner
            {
                Id = x.Id,
                DailyPlannerDate = x.DailyPlannerDate,
                IsForwordedToTimesheet = x.IsForwordedToTimesheet,
                SiteId = x.SiteId,
                EmployeeId = x.EmployeeId,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                Sites = new Site
                {
                    Id = x.Sites.Id,
                    Name = x.Sites.Name
                },
                DailyPlannerLines = x.DailyPlannerLines.Where(m => !m.Deleted).Select(mapping => new DailyPlannerLine
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
                    ProjectTask = new ProjectTask
                    {
                        Id = mapping.ProjectTask.Id,
                        Name = mapping.ProjectTask.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name
                    }
                }).ToList()
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDailyPlannerById
        // Title: GetDailyPlannerById
        // Description: This method retrieves a daily plans from the database by its unique identifier (`id`). 
        public async Task<DailyPlanner> GetDailyPlannerById(string id)
        {
            var query = _dailyPlannerRepository.Table;
            query = query.Where(x => !x.Deleted);
            query = query.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDailyPlannerByProject
        // Title: GetDailyPlannerByProject
        // Description: This method retrieves a daily plans based on its name and ProjectId. It allows an optional exclusion of a daily planner by its ID, which can be useful for scenarios like checking for duplicates.
        public async Task<DailyPlanner> GetDailyPlannerByDate(string SiteId, string LoggedUserId, DateTime? DailyPlannerDate, string id = null)
        {
            var query = _dailyPlannerRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrEmpty(LoggedUserId))
                query = query.Where(x => x.EmployeeId == LoggedUserId);

            if (DailyPlannerDate != null)
                query = query.Where(x => x.DailyPlannerDate == DailyPlannerDate);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertDailyPlanner
        // Title: InsertDailyPlanner
        // Description: This method inserts a daily plan entity into the repository. It takes a daily plan object as input and uses the _dailyPlannerRepository to handle the insertion operation.
        public void InsertDailyPlanner(DailyPlanner entity)
        {
            _dailyPlannerRepository.Insert(entity);
        }
        #endregion

        #region UpdateDailyPlanner
        // Title: UpdateDailyPlanner
        // Description: This method updates the daily planner entity based on the planner date in the repository. It takes a planner object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateDailyPlanner(DailyPlanner entity)
        {
            _dailyPlannerRepository.Update(entity);
        }
        #endregion

        #region DeleteDailyPlanner
        // Title: DeleteDailyPlanner
        // Description: Marks the specified daily planner entity as deleted by setting its `Deleted` property to true.
        public void DeleteDailyPlanner(DailyPlanner entity)
        {
            entity.Deleted = true;

            _dailyPlannerRepository.Update(entity);
        }
        #endregion

        #region GetAllDailyPlannerDashboard
        // Title: GetAllDailyPlannerDashboard
        public IPagedList<DailyPlanner> GetAllDailyPlannerDashboard(string SiteId, string createdBy, string employeeId, string projectId, DateTime? activityDate, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = 5, bool lookup = false)
        {
            var query = _dailyPlannerRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(x => x.CreatedById == createdBy);
            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.EmployeeId == employeeId);

            // Filter to only include DailyPlans with associated DailyPlannerLines, and optionally filter by projectId if provided
            query = query.Where(x => x.DailyPlannerLines.Any(line => !line.Deleted &&
                (string.IsNullOrWhiteSpace(projectId) || line.ProjectId == projectId)));

            //Search by Date 
            if (activityDate != null)
                query = query.Where(x => x.DailyPlannerDate == activityDate);
            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.DailyPlannerDate >= fromDate);
            if (toDate != null)
                query = query.Where(a => a.DailyPlannerDate <= toDate);

            query = query.OrderByDescending(x => x.DailyPlannerDate).Take(2)
            .Select(x => new DailyPlanner
            {
                Id = x.Id,
                DailyPlannerDate = x.DailyPlannerDate,
                IsForwordedToTimesheet = x.IsForwordedToTimesheet,
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
                DailyPlannerLines = x.DailyPlannerLines.Where(m => !m.Deleted && (string.IsNullOrWhiteSpace(projectId) || m.ProjectId == projectId)).Select(mapping => new DailyPlannerLine
                {
                    Id = mapping.Id,
                    Hours = mapping.Hours,
                    Description = mapping.Description,
                    UpdatedOnUtc = mapping.UpdatedOnUtc,
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
                    ProjectTask = new ProjectTask
                    {
                        Id = mapping.ProjectTask.Id,
                        Name = mapping.ProjectTask.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name
                    }
                }).ToList(),

            });

            var list = new PagedList<DailyPlanner>(query, page, pageSize);
            return list;
        }
        #endregion
    }
}
