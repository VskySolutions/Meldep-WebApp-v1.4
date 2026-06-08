using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;

namespace Vsky.Services.IssueActivitys
{
    public class IssueActivityService : IIssueActivityService
    {

        #region Fields

        private readonly IRepository<IssueActivity> _issueActivityRepository;

        #endregion

        #region Ctor

        public IssueActivityService(IRepository<IssueActivity> issueActivityRepository)
        {
            _issueActivityRepository = issueActivityRepository;
        }

        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion
      
        public List<IssueActivity> GetByIssueId(string id)
        {
            var query = _issueActivityRepository.TableNoTracking.Where(x => !x.Deleted);

            var projectedData = query.Select(x => new IssueActivity
            {
                Id = x.Id,
                IssueId = x.IssueId,
                ActivityName = x.ActivityName,
                DueDate = x.DueDate,
                PriorityId = x.PriorityId,
                AssignedTo = x.AssignedTo,
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                AssignedToEmployee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.AssignedToEmployee.Person.Id,
                        FullName = x.AssignedToEmployee.Person.FirstName + " " + x.AssignedToEmployee.Person.LastName,
                    }
                },
            });
            return projectedData.ToList();
        }

        public async Task<IssueActivity> GetById(string id)
        {
            var query = _issueActivityRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public void InsertIssueActivity(IssueActivity entity)
        {
            _issueActivityRepository.Insert(entity);
        }
        public void UpdateIssueActivity(IssueActivity entity)
        {
            _issueActivityRepository.Update(entity);
        }

        public void DeleteIssueActivity(IssueActivity entity)
        {
            entity.Deleted = true;

            _issueActivityRepository.Update(entity);
        }
    }
}
