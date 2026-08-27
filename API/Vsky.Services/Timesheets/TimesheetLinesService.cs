using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Timesheets
{
    public class TimesheetLinesService : ITimesheetLinesService
    {
        #region Define Services
        /// <summary>
        /// Define Services
        /// </summary>
        private readonly IRepository<TimesheetLines> _timesheetLinesRepository;
        private readonly IRepository<Notes> _notesRepository;
        #endregion

        #region Services Initializations
        /// <summary>
        /// Services Initializations
        /// </summary>
        /// <param name="timesheetLinesRepository"></param>
        public TimesheetLinesService(IRepository<TimesheetLines> timesheetLinesRepository, IRepository<Notes> notesRepository)
        {
            _timesheetLinesRepository = timesheetLinesRepository;
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

        #region GetAllTimesheetLines
        // Title: GetAllTimesheetLines
        // Description: This method retrieves a paginated list of timesheet Line based on various search criteria such as name, 
        // project name. The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<TimesheetLines> GetAllTimesheetLines(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(x => !x.Deleted);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new TimesheetLines
            {
                Id = x.Id,
                Description = x.Description,
                Hours = x.Hours,
                Timesheet = new Timesheet
                {
                    Id = x.Timesheet.Id,
                    TimesheetDate = x.Timesheet.TimesheetDate
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name
                }
            });

            var list = new PagedList<TimesheetLines>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetTimesheetByTaskId
        // Title: GetAllTimesheetLines
        // Description: This method retrieves a paginated list of timesheet Line based on various search criteria such as name, 
        // project name. The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<List<TimesheetLines>> GetTimesheetByTaskId(string id)
        {
            var query = _timesheetLinesRepository.Table
                .Where(x => !x.Deleted && x.ProjectTaskId == id)
                .GroupBy(x => new
                {
                    x.ProjectTaskId,
                    x.Timesheet.EmployeeId
                })
                .Select(group => new TimesheetLines
                {
                    Hours = group.Sum(x => x.Hours), // Sum hours for the grouped items
                    Description = group.FirstOrDefault().Description,
                    Timesheet = new Timesheet
                    {
                        Id = group.FirstOrDefault().Timesheet.Id,
                        TimesheetDate = group.FirstOrDefault().Timesheet.TimesheetDate,
                        EmployeeId = group.Key.EmployeeId,
                        Employee = new Employee
                        {
                            Id = group.FirstOrDefault().Timesheet.Employee.Id,
                            Person = new Person
                            {
                                Id = group.FirstOrDefault().Timesheet.Employee.Person.Id,
                                FullName = group.FirstOrDefault().Timesheet.Employee.Person.FirstName + " " +
                                           group.FirstOrDefault().Timesheet.Employee.Person.LastName
                            }
                        }
                    },
                    Project = new Project
                    {
                        Id = group.FirstOrDefault().Project.Id,
                        Name = group.FirstOrDefault().Project.Name,
                        ProjectNotesCount = _notesRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == group.FirstOrDefault().Project.Id && m.Type == "Projects"),
                    },
                    Task = new ProjectTask
                    {
                        Id = group.FirstOrDefault().Task.Id,
                        Name = group.FirstOrDefault().Task.Name
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = group.FirstOrDefault().ProjectActivity.Id,
                        Name = group.FirstOrDefault().ProjectActivity.Name
                    }
                });

            return await query.ToListAsync();
        }

        #endregion

        #region GetTimesheetLinesDetailsById
        // Title: GetTimesheetLinesDetailsById
        // Description: The method selects relevant fields from the timesheet Line entity, and returns a `Timesheet Line` object with these details. 
        public async Task<TimesheetLines> GetTimesheetLinesDetailsById(string id)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new TimesheetLines
            {
                Id = x.Id,
                Description = x.Description,
                Hours = x.Hours,
                Timesheet = new Timesheet
                {
                    Id = x.Timesheet.Id,
                    TimesheetDate = x.Timesheet.TimesheetDate
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetTimesheetLinesById
        // Title: GetTimesheetLinesById
        // Description: This method retrieves a timesheet Line from the database by its unique identifier (`id`). 
        public async Task<TimesheetLines> GetTimesheetLinesById(string id)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(x => !x.Deleted);
            query = query.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetTimesheetLinesByTimesheet
        // Title: GetTimesheetLinesByTimesheet
        // Description: Retrieves a list of TimesheetLines entities associated with a specific Timeshee ID.
        public List<TimesheetLines> GetTimesheetLinesByTimesheet(string TimesheetId)
        {
            var query = _timesheetLinesRepository.Table;
            query = query.Where(x => !x.Deleted && x.TimesheetId == TimesheetId);
            var list = query.ToList();
            return list;
        }
        #endregion

        #region GetTimesheetLineByTimesheetId
        public async Task<TimesheetLines> GetTimesheetLineByTimesheetId(string timesheetId)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(x => !x.Deleted);
            query = query.Where(x => x.TimesheetId == timesheetId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetTimesheetLinesByProjectModuleId
        public async Task<List<TimesheetLines>> GetTimesheetLinesByProjectModuleId(string ProjectModuleId)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModuleId == ProjectModuleId);
            query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new TimesheetLines
            {
                Id = x.Id,
                TimesheetId = x.TimesheetId,
                ProjectModuleId = x.ProjectModuleId,
                ProjectTaskId = x.ProjectTaskId,
                ProjectActivityId = x.ProjectActivityId,
                Description = x.Description,
                Hours = x.Hours,
                Timesheet = new Timesheet
                {
                    Id = x.Timesheet.Id,
                    TimesheetDate = x.Timesheet.TimesheetDate
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetTimesheetLinesByProjectModuleIdForMoveModuleAsProject
        public async Task<List<TimesheetLines>> GetTimesheetLinesByProjectModuleIdForMoveModuleAsProject(string ProjectModuleId)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModuleId == ProjectModuleId);
            query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new TimesheetLines
            {
                Id = x.Id,
                TimesheetId = x.TimesheetId,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                ProjectTaskId = x.ProjectTaskId,
                ProjectActivityId = x.ProjectActivityId,
                Description = x.Description,
                Hours = x.Hours,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetTimesheetLinesDetailsByIds
        public async Task<List<TimesheetLines>> GetTimesheetLinesDetailsByIds(string[] ids)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(x => !x.Deleted && ids.Any(m => m == x.Id));
            query = query.OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectModule.Name).ThenBy(x => x.Task.Name).ThenBy(x => x.ProjectActivity.Name).Select(x => new TimesheetLines
            {
                Description = x.Description,
                Hours = x.Hours,
                Timesheet = new Timesheet
                {
                    Id = x.Timesheet.Id,
                    TimesheetDate = x.Timesheet.TimesheetDate
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name
                },
                ProjectActivity = new ProjectActivity
                {
                    Id = x.ProjectActivity.Id,
                    Name = x.ProjectActivity.Name,
                }

            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region Get Timesheets For Requirement Center
        public async Task<List<TimesheetLines>> GetTimesheetsByRequirementId(string siteId, string requirementId)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(m => !m.Deleted && m.Timesheet.SiteId == siteId && m.Task.RequirementId == requirementId);

            query = query.Select(x => new TimesheetLines
            {
                Id = x.Id,
                Hours = x.Hours,
                Description = x.Description,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name
                },
                ProjectActivity = new ProjectActivity
                {
                    Id = x.ProjectActivity.Id,
                    Name = x.ProjectActivity.Name
                },
                Timesheet = new Timesheet
                {
                    TimesheetDate = x.Timesheet.TimesheetDate,
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
                    },
                }
            });

            var list = await query
                .OrderByDescending(x => x.Timesheet.TimesheetDate)
                .ThenBy(x => x.Timesheet.User.Person.FullName)
                .ToListAsync();
            return list;
        }
        public async Task<List<TimesheetLines>> GetTimesheetDetails(
            string siteId,
            string requirementId,
            string groupBy,
            string groupId,
            string createdBy,
            string searchText,
            string employeeId,
            string projectTaskId,
            string projectActivityId,
            DateTime? activityDate,
            DateTime? fromDate,
            DateTime? toDate,
            bool thisWeek,
            int lastNumberOfWeeks
        )
        {
            var query = GetFilteredTimesheetQuery(
                siteId,
                requirementId,
                createdBy,
                searchText,
                employeeId,
                projectTaskId,
                projectActivityId,
                activityDate,
                fromDate,
                toDate,
                thisWeek,
                lastNumberOfWeeks);

            switch ((groupBy ?? "date").ToLower())
            {
                case "employee":
                    query = query.Where(x => x.Timesheet.User.Id == groupId);
                    break;

                case "task":
                    query = query.Where(x => x.Task.Id == groupId);
                    break;

                default:
                    var date = DateTime.Parse(groupId);

                    query = query.Where(x =>
                        x.Timesheet.TimesheetDate >= date.Date &&
                        x.Timesheet.TimesheetDate < date.Date.AddDays(1));
                    break;
            }

            return await query
                .Select(x => new TimesheetLines
                {
                    Id = x.Id,
                    Hours = x.Hours,
                    Description = x.Description,

                    Project = new Project
                    {
                        Id = x.Project.Id,
                        Name = x.Project.Name
                    },

                    Task = new ProjectTask
                    {
                        Id = x.Task.Id,
                        Name = x.Task.Name
                    },

                    ProjectActivity = new ProjectActivity
                    {
                        Id = x.ProjectActivity.Id,
                        Name = x.ProjectActivity.Name
                    },

                    Timesheet = new Timesheet
                    {
                        TimesheetDate = x.Timesheet.TimesheetDate,

                        User = new ApplicationUser
                        {
                            Id = x.Timesheet.User.Id,
                            UserName = x.Timesheet.User.UserName,

                            Person = new Person
                            {
                                Id = x.Timesheet.User.PersonId,
                                FirstName = x.Timesheet.User.Person.FirstName,
                                LastName = x.Timesheet.User.Person.LastName,
                                FullName = x.Timesheet.User.Person.FirstName + " " +
                                           x.Timesheet.User.Person.LastName
                            }
                        }
                    }
                })
                .OrderByDescending(x => x.Timesheet.TimesheetDate)
                .ThenBy(x => x.Timesheet.User.Person.FullName)
                .ToListAsync();
        }

        public async Task<List<TimesheetGroupModel>> GetGroupedTimesheetsByRequirementId(
            string siteId,
            string requirementId,
            string groupBy,
            string createdBy,
            string searchText,
            string employeeId,
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
            var query = GetFilteredTimesheetQuery(
                siteId,
                requirementId,
                createdBy,
                searchText,
                employeeId,
                projectTaskId,
                projectActivityId,
                activityDate,
                fromDate,
                toDate,
                thisWeek,
                lastNumberOfWeeks);

            switch ((groupBy ?? "date").ToLower())
            {
                case "employee":
                    {
                        var result = query
                            .GroupBy(x => new
                            {
                                x.Timesheet.User.Id,
                                Name = x.Timesheet.User.Person.FirstName + " " +
                                       x.Timesheet.User.Person.LastName
                            })
                            .Select(g => new TimesheetGroupModel
                            {
                                Id = g.Key.Id,
                                Name = g.Key.Name,
                                Count = g.Count(),
                                Hours = g.Sum(x => x.Hours)
                            });

                        result = (sortBy ?? "").ToLower() switch
                        {
                            "hours" => descending ? result.OrderByDescending(x => x.Hours) : result.OrderBy(x => x.Hours),
                            "count" => descending ? result.OrderByDescending(x => x.Count) : result.OrderBy(x => x.Count),
                            _ => descending ? result.OrderByDescending(x => x.Name) : result.OrderBy(x => x.Name)
                        };

                        return await result
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                    }

                case "task":
                    {
                        var result = query
                            .GroupBy(x => new
                            {
                                x.Task.Id,
                                x.Task.Name
                            })
                            .Select(g => new TimesheetGroupModel
                            {
                                Id = g.Key.Id,
                                Name = g.Key.Name,
                                Count = g.Count(),
                                Hours = g.Sum(x => x.Hours)
                            });

                        result = (sortBy ?? "").ToLower() switch
                        {
                            "hours" => descending ? result.OrderByDescending(x => x.Hours) : result.OrderBy(x => x.Hours),
                            "count" => descending ? result.OrderByDescending(x => x.Count) : result.OrderBy(x => x.Count),
                            _ => descending ? result.OrderByDescending(x => x.Name) : result.OrderBy(x => x.Name)
                        };

                        return await result
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                    }

                default:
                    {
                        var dateGroups = await query
                            .GroupBy(x => x.Timesheet.TimesheetDate.Value.Date)
                            .Select(g => new
                            {
                                Date = g.Key,
                                Count = g.Count(),
                                Hours = g.Sum(x => x.Hours)
                            })
                            .ToListAsync();

                        var result = dateGroups
                            .Select(x => new TimesheetGroupModel
                            {
                                Id = x.Date.ToString("yyyy-MM-dd"),
                                Name = x.Date.ToString("MM/dd/yyyy"),
                                Count = x.Count,
                                Hours = x.Hours
                            });

                        result = (sortBy ?? "").ToLower() switch
                        {
                            "hours" => descending
                                ? result.OrderByDescending(x => x.Hours)
                                : result.OrderBy(x => x.Hours),

                            "count" => descending
                                ? result.OrderByDescending(x => x.Count)
                                : result.OrderBy(x => x.Count),

                            _ => descending
                                ? result.OrderByDescending(x => DateTime.ParseExact(x.Name, "MM/dd/yyyy", null))
                                : result.OrderBy(x => DateTime.ParseExact(x.Name, "MM/dd/yyyy", null))
                        };

                        return result
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
                    }
            }
        }

        #region Private method
        private IQueryable<TimesheetLines> GetFilteredTimesheetQuery(
            string siteId,
            string requirementId,
            string createdBy,
            string searchText,
            string employeeId,
            string projectTaskId,
            string projectActivityId,
            DateTime? activityDate,
            DateTime? fromDate,
            DateTime? toDate,
            bool thisWeek,
            int lastNumberOfWeeks
        )
        {
            var query = _timesheetLinesRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.Timesheet.SiteId == siteId &&
                    x.Task.RequirementId == requirementId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(x => x.CreatedById == createdBy);

            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.Timesheet.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(projectTaskId))
                query = query.Where(x => x.ProjectTaskId == projectTaskId);

            if (!string.IsNullOrWhiteSpace(projectActivityId))
                query = query.Where(x => x.ProjectActivityId == projectActivityId);

            if (activityDate.HasValue && activityDate.Value != DateTime.MinValue)
            {
                var date = activityDate.Value.Date;

                query = query.Where(x =>
                    x.Timesheet.TimesheetDate >= date &&
                    x.Timesheet.TimesheetDate < date.AddDays(1));
            }

            if (fromDate.HasValue)
            {
                var startDate = fromDate.Value.Date;

                query = query.Where(x =>
                    x.Timesheet.TimesheetDate >= startDate);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.Timesheet.TimesheetDate < endDate);
            }

            if (thisWeek)
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                query = query.Where(x =>
                    x.Timesheet.TimesheetDate >= startOfWeek &&
                    x.Timesheet.TimesheetDate < endOfWeek);
            }

            if (lastNumberOfWeeks > 0)
            {
                var from = DateTime.Today.AddDays(-(lastNumberOfWeeks * 7));

                query = query.Where(x =>
                    x.Timesheet.TimesheetDate >= from);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                DateTime.TryParse(searchText, out var parsedDate);

                query = query.Where(x =>
                    x.Project.Name.Contains(searchText) ||
                    x.Task.Name.Contains(searchText) ||
                    x.ProjectActivity.Name.Contains(searchText) ||
                    x.Description.Contains(searchText) ||
                    (x.Timesheet.Employee.Person.FirstName + " " +
                     x.Timesheet.Employee.Person.LastName).Contains(searchText) ||
                    x.Hours.ToString().Contains(searchText) ||
                    (parsedDate != default &&
                     x.Timesheet.TimesheetDate >= parsedDate.Date &&
                     x.Timesheet.TimesheetDate < parsedDate.Date.AddDays(1)));
            }

            return query;
        }

        public async Task<List<TimesheetLines>> GetAllTimesheetsByRequirementId(
          string siteId,
          string requirementId,
          string createdBy,
          string searchText,
          string employeeId,
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
            var query = GetFilteredTimesheetQuery(
                siteId,
                requirementId,
                createdBy,
                searchText,
                employeeId,
                projectTaskId,
                projectActivityId,
                activityDate,
                fromDate,
                toDate,
                thisWeek,
                lastNumberOfWeeks);

            query = query
                .Select(x => new TimesheetLines
                {
                    Id = x.Id,
                    Hours = x.Hours,
                    Description = x.Description,

                    Project = new Project
                    {
                        Id = x.Project.Id,
                        Name = x.Project.Name
                    },

                    Task = new ProjectTask
                    {
                        Id = x.Task.Id,
                        Name = x.Task.Name
                    },

                    ProjectActivity = new ProjectActivity
                    {
                        Id = x.ProjectActivity.Id,
                        Name = x.ProjectActivity.Name
                    },

                    Timesheet = new Timesheet
                    {
                        TimesheetDate = x.Timesheet.TimesheetDate,

                        User = new ApplicationUser
                        {
                            Id = x.Timesheet.User.Id,
                            UserName = x.Timesheet.User.UserName,

                            Person = new Person
                            {
                                Id = x.Timesheet.User.PersonId,
                                FirstName = x.Timesheet.User.Person.FirstName,
                                LastName = x.Timesheet.User.Person.LastName,
                                FullName = x.Timesheet.User.Person.FirstName + " " +
                                           x.Timesheet.User.Person.LastName
                            }
                        }
                    }
                });
                //.OrderByDescending(x => x.Timesheet.TimesheetDate)
                //.ThenBy(x => x.Timesheet.User.Person.FullName)
                //.ToListAsync();

            var list = new PagedList<TimesheetLines>(query, page, pageSize);
            return list;
        }
        #endregion

        #endregion

        #region InsertTimesheetLines
        // Title: InsertTimesheetLines
        // Description: This method inserts a timesheet line entity into the repository. It takes a timesheet line object as input and uses the _timesheetLinesRepository to handle the insertion operation.
        public void InsertTimesheetLines(TimesheetLines entity)
        {
            _timesheetLinesRepository.Insert(entity);
        }
        #endregion

        #region UpdateTimesheetLines
        // Title: UpdateTimesheetLines
        // Description: This method updates the Timesheet Lines entity in the repository. It takes a Timesheet line object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTimesheetLines(TimesheetLines entity)
        {
            _timesheetLinesRepository.Update(entity);
        }
        #endregion

        #region DeleteTimesheetLines
        // Title : DeleteTimesheetLines
        // Description: Deletes a TimesheetLines entity from the repository.
        public void DeleteTimesheetLines(TimesheetLines entity)
        {
            entity.Deleted = true;
            _timesheetLinesRepository.Update(entity);
        }
        #endregion

        #region InsertTimesheetLinesList
        // Title: InsertTimesheetLinesList
        // Description: This method inserts a timesheet line entity into the repository. It takes a timesheet line object as input and uses the _timesheetLinesRepository to handle the insertion operation.
        public void InsertTimesheetLinesList(IList<TimesheetLines> entity)
        {
            _timesheetLinesRepository.Insert(entity);
        }
        #endregion

        #region UpdateTimesheetLinesList
        // Title: UpdateTimesheetLinesList
        // Description: This method updates the Timesheet Lines entity in the repository. It takes a Timesheet line object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTimesheetLinesList(IList<TimesheetLines> entity)
        {
            _timesheetLinesRepository.Update(entity);
        }
        #endregion

        #region DeleteTimesheetLinesList
        public void DeleteTimesheetLinesList(List<TimesheetLines> entities)
        {
            var TimesheetLines = new List<TimesheetLines>();
            foreach (var items in entities)
            {
                items.Deleted = true;
                TimesheetLines.Add(items);
            }
            _timesheetLinesRepository.Update(TimesheetLines);
        }
        #endregion

        #region GetTimesheetLinesDetailsByMeetingUId
        // Title: GetTimesheetLinesDetailsById
        // Description: The method selects relevant fields from the timesheet Line entity, and returns a `Timesheet Line` object with these details. 
        public async Task<TimesheetLines> GetTimesheetLinesDetailsByMeetingUId(string meetingUId)
        {
            var query = _timesheetLinesRepository.TableNoTracking.Where(x => !x.Deleted && x.MeetingUId == meetingUId).Select(x => new TimesheetLines
            {
                Id = x.Id,
                Description = x.Description,
                Hours = x.Hours,
                Timesheet = new Timesheet
                {
                    Id = x.Timesheet.Id,
                    TimesheetDate = x.Timesheet.TimesheetDate
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name
                },
                ProjectActivity = new ProjectActivity
                {
                    Id = x.ProjectActivity.Id,
                    Name = x.ProjectActivity.Name
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion
    }
}
