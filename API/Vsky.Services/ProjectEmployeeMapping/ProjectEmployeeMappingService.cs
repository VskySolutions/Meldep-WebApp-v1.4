using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectEmployeeMappings
{
    public class ProjectEmployeeMappingService : IProjectEmployeeMappingService
    {
        #region Define Service
        /// <summary>
        /// Define Service
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<ProjectEmployeeMapping> _projectEmployeeMappingRepository;
        private readonly IRepository<VWEmployeeAssignedHours> _vWEmployeeAssignedHoursRepository;
        private readonly IRepository<ProjectActivity> _projectActivityRepository;

        #endregion

        #region Service Initializations
        /// <summary>
        /// Service Initializations
        /// </summary>
        /// <param name="projectEmployeeMappingRepository"></param>
        public ProjectEmployeeMappingService(
            UserManager<ApplicationUser> userManager, 
            IRepository<ProjectEmployeeMapping> projectEmployeeMappingRepository,
            IRepository<VWEmployeeAssignedHours> vWEmployeeAssignedHoursRepository
        ,
            IRepository<ProjectActivity> projectActivityRepository
        )
        {
            _userManager = userManager;
            _projectEmployeeMappingRepository = projectEmployeeMappingRepository;
            _vWEmployeeAssignedHoursRepository = vWEmployeeAssignedHoursRepository;
            _projectActivityRepository = projectActivityRepository;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Private Methods
        /// </summary>
        /// <param name="orderBy"></param>
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region GetProjectEmployeeById
        // Title : GetProjectEmployeeById
        // Description: This method asynchronously retrieves a `ProjectEmployeeMapping` object from the repository based on the provided unique identifier (ID).
        public async Task<ProjectEmployeeMapping> GetProjectEmployeeById(string id)
        {
            var query = _projectEmployeeMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectEmployeeByProjectId
        // Title : GetProjectEmployeeByProjectId
        // Description: Retrieves Project Employee Mappings with their assigned roles.
        public List<ProjectEmployeeMapping> GetProjectEmployeeByProjectId(string ProjectId)
        {
            var query = _projectEmployeeMappingRepository.TableNoTracking
                .Where(x =>
                    x.ProjectId == ProjectId &&
                    !x.Deleted)
                .Include(x => x.ProjectEmployeeRoleMappings
                    .Where(r => !r.Deleted))
                .ToList();

            return query;
        }
        #endregion

        #region InsertProjectEmployees
        // Title : InsertProjectEmployees
        // Description: Inserts a new ProjectEmployeeMapping entity into the repository.
        public void InsertProjectEmployees(ProjectEmployeeMapping entity)
        {
            _projectEmployeeMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectEmployees
        // Title : UpdateProjectEmployees
        // Description: Updates an existing ProjectEmployeeMapping entity in the repository.
        public void UpdateProjectEmployees(ProjectEmployeeMapping entity)
        {
            _projectEmployeeMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectEmployees
        // Title : DeleteProjectEmployees
        // Description: Deletes a ProjectEmployeeMapping entity from the repository.
        public void DeleteProjectEmployees(ProjectEmployeeMapping entity)
        {
            entity.Deleted = true;
            _projectEmployeeMappingRepository.Update(entity);
        }
        #endregion

        #region InsertProjectEmployeeMappingList
        public void InsertProjectEmployeeMappingList(IList<ProjectEmployeeMapping> entities)
        {
            _projectEmployeeMappingRepository.Insert(entities);
        }
        #endregion

        #region UpdateProjectEmployeeMappingList
        public void UpdateProjectEmployeeMappingList(List<ProjectEmployeeMapping> entities)
        {
            _projectEmployeeMappingRepository.Update(entities);
        }
        #endregion

        #region DeleteProjectEmployeeMappingList
        public void DeleteProjectEmployeeMappingList(List<ProjectEmployeeMapping> entities)
        {
            var list = new List<ProjectEmployeeMapping>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _projectEmployeeMappingRepository.Update(list);
        }
        #endregion

        #region GetProjectCharterEmployeeByProjectId
        public async Task<List<ProjectCharterEmployee>> GetProjectCharterEmployeesWithWeeklyPlanHoursByProjectId(string projectId, string taskId, DateTime? currentDate = null)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(taskId) || !currentDate.HasValue)
                return new List<ProjectCharterEmployee>();

            var month = currentDate.Value.Month;
            var year = currentDate.Value.Year;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var result = await _projectEmployeeMappingRepository.TableNoTracking
                .Where(x => !x.Deleted &&
                            x.ProjectId == projectId &&
                            x.Employee.Active)
                .GroupBy(x => new
                {
                    x.Employee.Id,
                    x.Employee.Person.FirstName,
                    x.Employee.Person.LastName
                })
                .Select(g => new ProjectCharterEmployee
                {
                    Id = g.First().Id,
                    EmployeeId = g.Key.Id,
                    EmployeeName = g.Key.FirstName + " " + g.Key.LastName
                })
                .OrderBy(x => x.EmployeeName)
                .ToListAsync();

            if (!result.Any())
                return result;

            var employeeIds = result.Select(x => x.EmployeeId).ToList();

            // Get employees who already have an activity
            // under the selected task.
            var assignedEmployeeIds = await _projectActivityRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.ProjectId == projectId &&
                    x.TaskId == taskId &&
                    employeeIds.Contains(x.AssignedToId))
                .Select(x => x.AssignedToId)
                .Distinct()
                .ToListAsync();

            var assignedEmployeeIdLookup = assignedEmployeeIds
                .ToHashSet();


            var employeeAssignedHours = await _vWEmployeeAssignedHoursRepository.TableNoTracking
                .Where(x => employeeIds.Contains(x.EmployeeId)
                         && x.WeekendDate >= startDate
                         && x.WeekendDate < endDate)
                .Select(x => new
                {
                    x.EmployeeId,
                    x.TotalHours,
                    x.WeekendDate
                })
                .ToListAsync();

            var lookup = employeeAssignedHours.ToLookup(x => x.EmployeeId);

            foreach (var item in result)
            {
                item.IsActivityAssigned = assignedEmployeeIdLookup.Contains(item.EmployeeId);

                item.EmployeeAssignedHours = lookup[item.EmployeeId]
                    .Select(x => new ProjectCharterEmployeeAssignedHours
                    {
                        TotalHours = x.TotalHours,
                        WeekendDate = x.WeekendDate
                    })
                    .ToList();
            }

            return result;
        }
        #endregion

        #region
        public async Task<List<ProjectEmployeeMapping>> GetProjectEmployeesByProjectId(string projectId)
        {
            return await _projectEmployeeMappingRepository.TableNoTracking
                .Where(x => x.ProjectId == projectId && !x.Deleted)
                .ToListAsync();
        }

        public async Task<List<CommonDropDown>> GetProjectCharterEmployeeByProjectId(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
                return new List<CommonDropDown>();

            var list = await _projectEmployeeMappingRepository.TableNoTracking
               .Where(x => !x.Deleted && x.ProjectId == projectId && x.Employee.Active)
                // Remove duplicate employees
                .GroupBy(x => new
                {
                    x.Employee.Id,
                    x.Employee.Person.FirstName,
                    x.Employee.Person.LastName
                })
               .Select(g => new CommonDropDown
               {
                   Text = string.Concat(g.Key.FirstName, " ", g.Key.LastName),
                   Value = g.Key.Id
               })
               .OrderBy(x => x.Text)
               .ToListAsync();

            return list;
        }
        public async Task<List<CommonDropDown>> GetProjectEmployeesByProjectIdAndReturnUserId(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
                return new List<CommonDropDown>();

            var list = await _projectEmployeeMappingRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.ProjectId == projectId &&
                    x.Employee.Active &&
                    x.Employee.Person != null)
                .Select(x => new
                {
                    PersonId = x.Employee.Person.Id,
                    FirstName = x.Employee.Person.FirstName,
                    LastName = x.Employee.Person.LastName
                })
                        .Distinct()
                .Join(
                    _userManager.Users,
                    employee => employee.PersonId,
                    user => user.Person.Id,
                    (employee, user) => new CommonDropDown
                    {
                        Text = string.Concat(
                            employee.FirstName,
                            " ",
                            employee.LastName
                        ),
                        Value = user.Id
                    })
                .OrderBy(x => x.Text)
                .ToListAsync();

            return list;
        }
        #endregion

        #region GetProjectEmployeeByRoleId
        // Title: GetProjectEmployeeByRoleId
        public List<ProjectEmployeeMapping> GetProjectEmployeesByRoleId(string projectId, string roleId)
        {

            var query = _projectEmployeeMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectId == projectId && x.ProjectEmployeeRoleMappings.Any(r => r.SiteProjectRoleId == roleId));
            query = query.Select(x => new ProjectEmployeeMapping
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    },
                }
            });
            return query.ToList();
        }
        #endregion
    }
}
