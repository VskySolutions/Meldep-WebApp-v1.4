using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.TimesheetAISummarys
{
    public class TimesheetAISummaryService : ITimesheetAISummaryService
    {
        #region Define Services
        private readonly IRepository<TimesheetAISummary> _timesheetAISummaryRepository;
        #endregion

        #region Services Initializations

        public TimesheetAISummaryService(IRepository<TimesheetAISummary> timesheetAISummaryRepository)
        {
            _timesheetAISummaryRepository = timesheetAISummaryRepository;
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

        #region GetAllTimesheetAISummary
        // Title: GetAllTimesheetAISummary
        // Description: This method retrieves a paginated list of Timesheet AI Summary based on various search 
        // It also supports sorting and includes related data. The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<TimesheetAISummary>> GetAllTimesheetAISummary(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _timesheetAISummaryRepository.TableNoTracking;

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new TimesheetAISummary
            {
                Id = x.Id,
                Summary = x.Summary,
                Task = new ProjectTask
                {
                   Id = x.Task.Id,
                   Name = x.Task.Name,
                }
            });

            var list = new PagedList<TimesheetAISummary>(query, page, pageSize);

            return list;
        }
        #endregion

        #region GetTimesheetAISummaryById
        // Title: GetTimesheetAISummaryById
        // Description: This method retrieves a Timesheet AI Summary from the database by its unique identifier (`id`). 
        public async Task<TimesheetAISummary> GetTimesheetAISummaryById(string id)
        {
            var query = _timesheetAISummaryRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetTimesheetAISummaryDetailsById
        // Title: GetTimesheetAISummaryDetailsById
        // Description: The method selects relevant fields from the Timesheet AI Summary entity.
        public async Task<TimesheetAISummary> GetTimesheetAISummaryDetailsById(string id)
        {
            var query = _timesheetAISummaryRepository.TableNoTracking.Where(x => x.Id == id);
            query = query.Select(x => new TimesheetAISummary
            {
                Id = x.Id,
                Summary = x.Summary,
                Task = new ProjectTask
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTimesheetAISummary
        // Title: InsertTimesheetAISummary
        // Description: This method inserts a new Timesheet AI Summary entity into the repository. It takes a Timesheet AI Summary object as input and uses the _timesheetAISummaryRepository to handle the insertion operation.
        public void InsertTimesheetAISummary(TimesheetAISummary entity)
        {
            _timesheetAISummaryRepository.Insert(entity);
        }
        #endregion

        #region UpdateTimesheetAISummary
        // Title: UpdateTimesheetAISummary
        // Description: This method updates the specified Timesheet AI Summary entity in the repository. It takes a TimesheetAISummary object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTimesheetAISummary(TimesheetAISummary entity)
        {
            _timesheetAISummaryRepository.Update(entity);
        }
        #endregion

        #region DeleteTimesheetAISummary
        // Title: DeleteTimesheetAISummary
        // Description: Marks the specified Timesheet AI Summary entity as deleted by setting its `Deleted` property to true. 
        public void DeleteTimesheetAISummary(TimesheetAISummary entity)
        {
            entity.Deleted = true;

            _timesheetAISummaryRepository.Update(entity);
        }
        #endregion
    }
}
