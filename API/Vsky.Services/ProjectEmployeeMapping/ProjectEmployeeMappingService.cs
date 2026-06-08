using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ProjectEmployeeMappings;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.ProjectEmployeeMappings
{
    public class ProjectEmployeeMappingService : IProjectEmployeeMappingService
    {
        #region Define Service
        /// <summary>
        /// Define Service
        /// </summary>

        private readonly IRepository<ProjectEmployeeMapping> _projectEmployeeMappingRepository;
        private readonly IRepository<VWEmployeeAssignedHours> _vWEmployeeAssignedHoursRepository;

        #endregion

        #region Service Initializations
        /// <summary>
        /// Service Initializations
        /// </summary>
        /// <param name="projectEmployeeMappingRepository"></param>
        public ProjectEmployeeMappingService(IRepository<ProjectEmployeeMapping> projectEmployeeMappingRepository,
            IRepository<VWEmployeeAssignedHours> vWEmployeeAssignedHoursRepository)
        {
            _projectEmployeeMappingRepository = projectEmployeeMappingRepository;
            _vWEmployeeAssignedHoursRepository = vWEmployeeAssignedHoursRepository;
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
        // Description: Retrieves a list of ProjectEmployeeMapping entities associated with a specific project ID.
        public List<ProjectEmployeeMapping> GetProjectEmployeeByProjectId(string ProjectId)
        {
            var query = _projectEmployeeMappingRepository.TableNoTracking.Where(x => x.ProjectId == ProjectId);
            var list = query.ToList();
            return list;
        }
        #endregion

        //#region GetProjectEmployeeRoleById

        //public async Task<ProjectEmployeeMapping> GetById(string id)
        //{
        //    var query = _projectEmployeeMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
        //    var item = await query.FirstOrDefaultAsync();
        //    return item;
        //}
        //#endregion

        #region GetProjectEmployeeByRoleIdAndProjectId

        public async Task<ProjectEmployeeMapping> GetProjectEmployeeByRoleIdAndProjectId(string SiteId, string projectId, string roleId, string employeeId = null, string id = null)
        {
            var query = _projectEmployeeMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId && x.ProjectId == projectId && x.EmployeeDesignationId == roleId);

            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
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
        // Title: GetProjectCharterEmployeeByProjectId
        //public async Task<List<ProjectEmployeeMapping>> GetProjectCharterEmployeeByProjectId(string projectId, DateTime? currentDate = null)
        //{
        //    //var query = _projectEmployeeMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectId == projectId);
        //    //query = query.Select(x => new ProjectEmployeeMapping
        //    //{
        //    //    Id = x.Id,
        //    //    Employee = new Employee
        //    //    {
        //    //        Id = x.Employee.Id,
        //    //        Person = new Person
        //    //        {
        //    //            FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
        //    //        },
        //    //        EmployeeAssignedHours = x.Employee.EmployeeAssignedHours.Where(c => c.WeekendDate.Month == currentDate.Value.Month && c.WeekendDate.Year == currentDate.Value.Year).Select(p => new VWEmployeeAssignedHours
        //    //        {                
        //    //            TotalHours = p.TotalHours,
        //    //            WeekendDate = p.WeekendDate
        //    //        }).ToList(),
        //    //    }
        //    //});

        //    //// Group by Employee.Id and take the first for each
        //    //var distinctByEmployee = await query
        //    //    .GroupBy(x => x.Employee.Id)
        //    //    .Select(g => g.First())
        //    //    .ToListAsync();

        //    //return distinctByEmployee;

        //    if (string.IsNullOrEmpty(projectId) || !currentDate.HasValue)
        //        return new List<ProjectEmployeeMapping>();

        //    var month = currentDate.Value.Month;
        //    var year = currentDate.Value.Year;

        //    // Step 1: Fetch all EmployeeAssignedHours for the current month
        //    var assignedHoursList = await _vWEmployeeAssignedHoursRepository.TableNoTracking
        //        .Where(h => h.WeekendDate.Month == month && h.WeekendDate.Year == year)
        //        .Select(h => new
        //        {
        //            h.EmployeeId,
        //            h.TotalHours,
        //            h.WeekendDate
        //        })
        //        .ToListAsync(); // fetch to memory

        //    // Step 2: Fetch all project employees
        //    var employeesList = await _projectEmployeeMappingRepository.TableNoTracking
        //        .Where(x => !x.Deleted && x.ProjectId == projectId)
        //        .Select(x => new
        //        {
        //            MappingId = x.Id,
        //            EmployeeId = x.Employee.Id,
        //            FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName
        //        })
        //        .ToListAsync(); // fetch to memory first

        //    // Step 3: Deduplicate employees in-memory
        //    var employees = employeesList
        //        .GroupBy(x => x.EmployeeId)
        //        .Select(g => g.First())
        //        .ToList();

        //    // Step 4: Combine assigned hours and employee info in-memory
        //    var result = employees.Select(e => new ProjectEmployeeMapping
        //    {
        //        Id = e.MappingId,
        //        Employee = new Employee
        //        {
        //            Id = e.EmployeeId,
        //            Person = new Person
        //            {
        //                FullName = e.FullName
        //            },
        //            EmployeeAssignedHours = assignedHoursList
        //                .Where(h => h.EmployeeId == e.EmployeeId)
        //                .Select(p => new VWEmployeeAssignedHours
        //                {
        //                    TotalHours = p.TotalHours,
        //                    WeekendDate = p.WeekendDate
        //                })
        //                .ToList()
        //        }
        //    })
        //       .OrderBy(x => x.Employee.Person.FullName)
        //       .ToList();

        //    return result;
        //}

        public async Task<List<ProjectEmployeeMapping>> GetProjectCharterEmployeesWithWeeklyPlanHoursByProjectId(string projectId, DateTime? currentDate = null)
        {
            if (string.IsNullOrEmpty(projectId) || !currentDate.HasValue)
                return new List<ProjectEmployeeMapping>();

            var month = currentDate.Value.Month;
            var year = currentDate.Value.Year;

            var result = await _projectEmployeeMappingRepository.TableNoTracking
                .Where(x => !x.Deleted && x.ProjectId == projectId && x.Employee.Active)

                // GROUP BY Employee to remove duplicates
                .GroupBy(x => new
                {
                    x.Employee.Id,
                    x.Employee.Person.FirstName,
                    x.Employee.Person.LastName
                })
                .Select(g => new ProjectEmployeeMapping
                {
                    Id = g.First().Id,
                    Employee = new Employee
                    {
                        Id = g.Key.Id,
                        Person = new Person
                        {
                            FullName = g.Key.FirstName + " " + g.Key.LastName
                        },
                        EmployeeAssignedHours = _vWEmployeeAssignedHoursRepository.TableNoTracking
                            .Where(h => h.EmployeeId == g.Key.Id
                                     && h.WeekendDate.Month == month
                                     && h.WeekendDate.Year == year)
                            .Select(h => new VWEmployeeAssignedHours
                            {
                                TotalHours = h.TotalHours,
                                WeekendDate = h.WeekendDate
                            })
                            .ToList()
                    }
                })

                .OrderBy(x => x.Employee.Person.FullName)
                .ToListAsync();

            return result;
        }
        #endregion

        #region
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
        #endregion

        #region GetProjectEmployeeByRoleId
        // Title: GetProjectEmployeeByRoleId
        public List<ProjectEmployeeMapping> GetProjectEmployeesByRoleId(string projectId, string roleId)
        {

            var query = _projectEmployeeMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectId == projectId && x.EmployeeDesignationId == roleId);
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
