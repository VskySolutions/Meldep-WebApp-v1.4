using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.IssueActivitys
{
    public interface IIssueActivityService
    {
        //IPagedList<IssueActivity> GetAlIssueActivity(string sortBy,
        //   bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        Task<IssueActivity> GetById(string id);

        List<IssueActivity> GetByIssueId(string id);


        void InsertIssueActivity(IssueActivity entity);

        void UpdateIssueActivity(IssueActivity entity);

        void DeleteIssueActivity(IssueActivity entity);
    }
}
