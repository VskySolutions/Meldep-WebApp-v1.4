using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Candidates
{
    public interface ICandidateFeedbacksService
    {
        IPagedList<CandidateFeedback> GetAllCandidateFeedbackLogs(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        Task<CandidateFeedback> GetCandidateFeedbackById(string id);

        List<CandidateFeedback> GetCandidateFeedbackDetailsById(string SiteId, string id);

        void InsertCandidateFeedbacks(CandidateFeedback entity);

        void UpdateCandidateFeedbacks(CandidateFeedback entity);

        void DeleteCandidateFeedbacks(CandidateFeedback entity);
    }
}
