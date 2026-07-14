using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.TestCases
{
    public class TestCaseExecutionLogExecutionLogService : ITestCaseExecutionLogExecutionLogService
    {
        #region Define Services
        private readonly IRepository<TestCaseExecutionLog> _testCaseExecutionLogRepository;
        #endregion

        #region Services Initializations

        public TestCaseExecutionLogExecutionLogService(
            IRepository<TestCaseExecutionLog> testCaseExecutionLogRepository
        )
        {
            _testCaseExecutionLogRepository = testCaseExecutionLogRepository;
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

        #region GetAllTestCaseExecutionLogs
        // Title: GetAllTestCaseExecutionLogs
        // Description: This method retrieves a paginated list of Test Case Execution Log based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<TestCaseExecutionLog>> GetAllTestCaseExecutionLogs(
            string SiteId,
            string LoggedUserId,
            string SearchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            //var query = _testCaseExecutionLogRepository.TableNoTracking.Where(x => !x.Deleted);
            var query = _testCaseExecutionLogRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectReleaseTracking_ReqPlanTaskIssueMapping.TestCase.SiteId == SiteId);

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
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                      m.Status.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                      m.CreatedOnUtc.Date == parsedDate.Date
                );
            }
            query = query.Select(x => new TestCaseExecutionLog
            {
                Id = x.Id,
                Comment = x.Comment,
                StatusId = x.StatusId,
                CreatedOnUtc = x.CreatedOnUtc,
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                ProjectReleaseTracking_ReqPlanTaskIssueMapping = new ProjectReleaseTrackingReqPlanTaskIssueMapping
                {
                    Id = x.ProjectReleaseTracking_ReqPlanTaskIssueMapping.Id,
                    TestCaseId = x.ProjectReleaseTracking_ReqPlanTaskIssueMapping.TestCaseId
                }
            });

            var list = new PagedList<TestCaseExecutionLog>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetTestCaseExecutionLogById
        // Title: GetTestCaseExecutionLogById
        // Description: This method retrieves a TestCaseExecutionLog from the database by its unique identifier (`id`). 
        public async Task<TestCaseExecutionLog> GetTestCaseExecutionLogById(string id)
        {
            var query = _testCaseExecutionLogRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        public async Task<List<TestCaseExecutionLog>> GetTestCaseExecutionLogsByMappingId(string mappingId)
        {
            return await _testCaseExecutionLogRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.ProjectReleaseTracking_ReqPlanTaskIssueMappingId == mappingId)
                .OrderByDescending(x => x.CreatedOnUtc)
                .ToListAsync();
        }

        #region GetTestCaseExecutionLogDetailsById
        // Title: GetTestCaseExecutionLogDetailsById
        // Description: The method selects relevant fields from the TestCaseExecutionLog entity.
        public async Task<TestCaseExecutionLog> GetTestCaseExecutionLogDetailsById(string id)
        {
            var query = _testCaseExecutionLogRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new TestCaseExecutionLog
            {
                Id = x.Id,
                Comment = x.Comment,
                StatusId = x.StatusId,
                CreatedOnUtc = x.CreatedOnUtc,
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                ProjectReleaseTracking_ReqPlanTaskIssueMapping = new ProjectReleaseTrackingReqPlanTaskIssueMapping
                {
                    Id = x.ProjectReleaseTracking_ReqPlanTaskIssueMapping.Id,
                    TestCaseId = x.ProjectReleaseTracking_ReqPlanTaskIssueMapping.TestCaseId
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTestCaseExecutionLog
        // Title: InsertTestCaseExecutionLog
        // Description: This method inserts a new TestCaseExecutionLog entity into the repository. It takes a TestCaseExecutionLog object as input and uses the _testCaseExecutionLogRepository to handle the insertion operation.
        public void InsertTestCaseExecutionLog(TestCaseExecutionLog entity)
        {
            _testCaseExecutionLogRepository.Insert(entity);
        }
        #endregion

        #region UpdateTestCaseExecutionLog
        // Title: UpdateTestCaseExecutionLog
        // Description: This method updates the specified TestCaseExecutionLog entity in the repository. It takes a TestCaseExecutionLog object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTestCaseExecutionLog(TestCaseExecutionLog entity)
        {
            _testCaseExecutionLogRepository.Update(entity);
        }
        #endregion

        #region DeleteTestCaseExecutionLog
        // Title: DeleteTestCaseExecutionLog
        // Description: Marks the specified TestCaseExecutionLog entity as deleted by setting its `Deleted` property to true. 
        public void DeleteTestCaseExecutionLog(TestCaseExecutionLog entity)
        {
            entity.Deleted = true;

            _testCaseExecutionLogRepository.Update(entity);
        }
        #endregion
    }
}
