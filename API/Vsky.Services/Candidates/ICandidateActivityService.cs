using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Candidates
{
    public interface ICandidateActivityService
    {
        IPagedList<CandidateActivities> GetAllCandidateActivityLogs(string sortBy,
         bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        Task<IList<CandidateActivities>> GetAllActivities(string SiteId);

        Task<CandidateActivities> GetById(string id);

        List<CandidateActivities> GetCandidateDetailsById(string SiteId, string id);

        void InsertCandidateActivityLogs(CandidateActivities entity);

        void UpdateCandidateActivityLogs(CandidateActivities entity);

        void DeleteCandidateActivityLogs(CandidateActivities entity);

    }
}
