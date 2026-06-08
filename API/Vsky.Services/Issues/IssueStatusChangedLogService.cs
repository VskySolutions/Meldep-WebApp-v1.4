using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Issues
{
    public class IssueStatusChangedLogService : IIssueStatusChangedLogService
    {
        #region Define Services
        private readonly IRepository<IssueStatusChangedLog> _issueStatusChangedLogRepository;
        #endregion

        #region Services Initializations

        public IssueStatusChangedLogService(IRepository<IssueStatusChangedLog> issueStatusChangedLogRepository)
        {
            _issueStatusChangedLogRepository = issueStatusChangedLogRepository;
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

        #region GetIssueStatusLogById
        // Title: GetIssueStatusLogById
        // Description: This method retrieves a GetIssueStatusLog from the database by its unique identifier (`id`). 
        public async Task<IssueStatusChangedLog> GetIssueStatusLogById(string id)
        {
            var query = _issueStatusChangedLogRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetIssueStatusLogDetailsById
        // Title: GetIssueStatusLogDetailsById
        // Description: The method selects relevant fields from the GetIssueStatusLog entity.
        public async Task<IssueStatusChangedLog> GetIssueStatusLogDetailsById(string id)
        {
            var query = _issueStatusChangedLogRepository.TableNoTracking.Where(x => x.Id == id);
            query = query.Select(x => new IssueStatusChangedLog
            {
                Id = x.Id,
                IssueId = x.IssueId,
                StatusId = x.StatusId,
                StatusChangedBy = x.StatusChangedBy,
                StatusChangedDate = x.StatusChangedDate,
                Issue = new Issue
                {
                    Id = x.Issue.Id,
                    Name = x.Issue.Name
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

        #region InsertIssueStatusChangedLog
        // Title: InsertIssueStatusChangedLog
        // Description: This method inserts a new InsertIssueStatusChangedLog entity into the repository. It takes a InsertIssueStatusChangedLog object as input and uses the _issueStatusChangedLogRepository to handle the insertion operation.
        public void InsertIssueStatusChangedLog(IssueStatusChangedLog entity)
        {
            _issueStatusChangedLogRepository.Insert(entity);
        }
        #endregion

        #region UpdateIssueStatusChangedLog
        // Title: UpdateIssueStatusChangedLog
        // Description: This method updates the specified IssueStatusChangedLog entity in the repository. It takes a IssueStatusChangedLog object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateIssueStatusChangedLog(IssueStatusChangedLog entity)
        {
            _issueStatusChangedLogRepository.Update(entity);
        }
        #endregion
    }
}
