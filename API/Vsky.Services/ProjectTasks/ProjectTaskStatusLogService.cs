using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public class ProjectTaskStatusLogService : IProjectTaskStatusLogService
    {
        #region Define Services
        private readonly IRepository<ProjectTaskStatusLog> _projectTaskStatusLogRepository;
        #endregion

        #region Services Initializations

        public ProjectTaskStatusLogService(IRepository<ProjectTaskStatusLog> projectTaskStatusLogRepository)
        {
            _projectTaskStatusLogRepository = projectTaskStatusLogRepository;
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

        #region GetAllProjectTaskStatus
        // Title: GetAllProjectTaskStatus
        // Description: This method retrieves a paginated list of ProjectTaskStatusLog based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<ProjectTaskStatusLog> GetAllProjectTaskStatus(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _projectTaskStatusLogRepository.Table;
            
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            
            query = query.Select(x => new ProjectTaskStatusLog
            {
                Id = x.Id,
                TaskId = x.TaskId,
                StatusId = x.StatusId,
                StatusChangedBy = x.StatusChangedBy,
                StatusChangedDate = x.StatusChangedDate,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                StatusChangedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.StatusChangedByEmployee.Person.Id,
                        FullName = x.StatusChangedByEmployee.Person.FirstName + " " + x.StatusChangedByEmployee.Person.LastName,
                    }
                },               
            });

            var list = new PagedList<ProjectTaskStatusLog>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetProjectTaskStatusLogById
        // Title: GetProjectTaskStatusLogById
        // Description: This method retrieves a ProjectTaskStatusLog from the database by its unique identifier (`id`). 
        public async Task<ProjectTaskStatusLog> GetProjectTaskStatusLogById(string id)
        {
            var query = _projectTaskStatusLogRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectTaskStatusLogDetailsById
        // Title: GetProjectTaskStatusLogDetailsById
        // Description: The method selects relevant fields from the ProjectTaskStatusLog entity.
        public async Task<ProjectTaskStatusLog> GetProjectTaskStatusLogDetailsById(string id)
        {
            var query = _projectTaskStatusLogRepository.TableNoTracking.Where(x => x.Id == id);
            query = query.Select(x => new ProjectTaskStatusLog
            {
                Id = x.Id,
                TaskId = x.TaskId,
                StatusId = x.StatusId,
                StatusChangedBy = x.StatusChangedBy,
                StatusChangedDate = x.StatusChangedDate,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                StatusChangedByEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.StatusChangedByEmployee.Person.Id,
                        FullName = x.StatusChangedByEmployee.Person.FirstName + " " + x.StatusChangedByEmployee.Person.LastName,
                    }
                },
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertProjectTaskStatusLog
        // Title: InsertProjectTaskStatusLog
        // Description: This method inserts a new ProjectTaskStatusLog entity into the repository. It takes a ProjectTaskStatusLog object as input and uses the _projectTaskStatusLogRepository to handle the insertion operation.
        public void InsertProjectTaskStatusLog(ProjectTaskStatusLog entity)
        {
            _projectTaskStatusLogRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectTaskStatusLog
        // Title: UpdateProjectTaskStatusLog
        // Description: This method updates the specified ProjectTaskStatusLog entity in the repository. It takes a ProjectTaskStatusLog object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProjectTaskStatusLog(ProjectTaskStatusLog entity)
        {
            _projectTaskStatusLogRepository.Update(entity);
        }
        #endregion
    }
}

