using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.DailyPlanners
{
    public class DailyPlannerLineService : IDailyPlannerLineService
    {
        #region Define Services
        /// <summary>
        /// Define Services
        /// </summary>
        private readonly IRepository<DailyPlannerLine> _dailyPlannerLineRepository;
        #endregion

        #region Services Initializations
        /// <summary>
        /// Services Initializations
        /// </summary>
        /// <param name="dailyPlannerLineRepository"></param>
        public DailyPlannerLineService(IRepository<DailyPlannerLine> dailyPlannerLineRepository)
        {
            _dailyPlannerLineRepository = dailyPlannerLineRepository;
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

        #region GetAllDailyPlannerLine
        // Title: GetAllDailyPlannerLine
        // Description: This method retrieves a paginated list of daily planner Line based on various search criteria such as name, 
        // project name. The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<DailyPlannerLine> GetAllDailyPlannerLine(string SiteId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _dailyPlannerLineRepository.TableNoTracking.Where(x => !x.Deleted && x.DailyPlanner.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new DailyPlannerLine
            {
                Id = x.Id,
                Description = x.Description,
                Hours = x.Hours,
                DailyPlanner = new DailyPlanner
                {
                    Id = x.DailyPlanner.Id,
                    DailyPlannerDate = x.DailyPlanner.DailyPlannerDate
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
                ProjectTask = new ProjectTask
                {
                    Id = x.ProjectTask.Id,
                    Name = x.ProjectTask.Name
                }
            });

            var list = new PagedList<DailyPlannerLine>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetDailyPlannerLineDetailsById
        // Title: GetDailyPlannerLineDetailsById
        // Description: The method selects relevant fields from the daily planner Line entity, and returns a `Daily Planner Line` object with these details. 
        public async Task<DailyPlannerLine> GetDailyPlannerLineDetailsById(string id)
        {
            var query = _dailyPlannerLineRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new DailyPlannerLine
            {
                Id = x.Id,
                Description = x.Description,
                Hours = x.Hours,
                DailyPlanner = new DailyPlanner
                {
                    Id = x.DailyPlanner.Id,
                    DailyPlannerDate = x.DailyPlanner.DailyPlannerDate
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
                ProjectTask = new ProjectTask
                {
                    Id = x.ProjectTask.Id,
                    Name = x.ProjectTask.Name
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDailyPlannerLineById
        // Title: GetDailyPlannerLineById
        // Description: This method retrieves a daily plans Line from the database by its unique identifier (`id`). 
        public async Task<DailyPlannerLine> GetDailyPlannerLineById(string id)
        {
            var query = _dailyPlannerLineRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDailyPlannerLineByDailyPlanner
        // Title: GetDailyPlannerLineByDailyPlanner
        // Description: Retrieves a list of DailyPlannerLine entities associated with a specific DailyPlanner ID.
        public List<DailyPlannerLine> GetDailyPlannerLineByDailyPlanner(string DailyPlannerId)
        {
            var query = _dailyPlannerLineRepository.TableNoTracking.Where(x => !x.Deleted && x.DailyPlannerId == DailyPlannerId);

            var list = query.ToList();
            return list;
        }
        #endregion

        #region GetDailyPlannerLineByProjectModuleId
        public async Task<List<DailyPlannerLine>> GetDailyPlannerLineByProjectModuleId(string ProjectModuleId)
        {
            var query = _dailyPlannerLineRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModuleId == ProjectModuleId);

            query = query.Select(x => new DailyPlannerLine
            {
                Id = x.Id,
                DailyPlannerId = x.DailyPlannerId,
                ProjectModuleId = x.ProjectModuleId,
                ProjectTaskId = x.ProjectTaskId,
                ProjectActivityId = x.ProjectActivityId,
                Description = x.Description,
                Hours = x.Hours,
                DailyPlanner = new DailyPlanner
                {
                    Id = x.DailyPlanner.Id,
                    DailyPlannerDate = x.DailyPlanner.DailyPlannerDate
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

        #region GetAllDailyPlannerLineByProjectModuleIdForMoveModuleAsProject
        public async Task<List<DailyPlannerLine>> GetAllDailyPlannerLineByProjectModuleIdForMoveModuleAsProject(string ProjectModuleId)
        {
            var query = _dailyPlannerLineRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectModuleId == ProjectModuleId);

            query = query.Select(x => new DailyPlannerLine
            {
                Id = x.Id,
                DailyPlannerId = x.DailyPlannerId,
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

        #region InsertDailyPlannerLine
        // Title: InsertDailyPlannerLine
        // Description: This method inserts a daily plan line entity into the repository. It takes a daily plan line object as input and uses the _dailyPlannerLineRepository to handle the insertion operation.
        public void InsertDailyPlannerLine(DailyPlannerLine entity)
        {
            _dailyPlannerLineRepository.Insert(entity);
        }
        #endregion

        #region UpdateDailyPlannerLine
        // Title: UpdateDailyPlannerLine
        // Description: This method updates the daily planner line entity in the repository. It takes a planner line object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateDailyPlannerLine(DailyPlannerLine entity)
        {
            _dailyPlannerLineRepository.Update(entity);
        }
        #endregion
       
        #region DeleteDailyPlannerLine
        // Title : DeleteDailyPlannerLine
        // Description: Deletes a DailyPlannerLine entity from the repository.
        public void DeleteDailyPlannerLine(DailyPlannerLine entity)
        {
            _dailyPlannerLineRepository.Delete(entity);
        }
        #endregion

        #region InsertDailyPlannerLineList
        public void InsertDailyPlannerLineList(IList<DailyPlannerLine> entities)
        {
            _dailyPlannerLineRepository.Insert(entities);
        }
        #endregion

        #region UpdateDailyPlannerLineList
        public void UpdateDailyPlannerLineList(IList<DailyPlannerLine> entities)
        {
            _dailyPlannerLineRepository.Update(entities);
        }
        #endregion

        #region DeleteDailyPlannerLineList
        public void DeleteDailyPlannerLineList(List<DailyPlannerLine> entities)
        {
            var DailyPlannerLine = new List<DailyPlannerLine>();
            foreach (var items in entities)
            {
                items.Deleted = true;
                DailyPlannerLine.Add(items);
            }
            _dailyPlannerLineRepository.Update(DailyPlannerLine);
        }
        #endregion
    }
}
