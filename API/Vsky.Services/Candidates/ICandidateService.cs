using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Candidates
{
    public interface ICandidateService
    {
        #region GetAllCandidateList
        IPagedList<Candidate> GetAllCandidateList(string SiteId, string SearchText, string fullName, string emailAddress, string mobileNumber, List<string> appliedWorkLocationId, List<string> jobId, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetById
        Task<Candidate> GetById(string id);
        #endregion

        #region GetCandidateDetailsById
        Task<Candidate> GetCandidateDetailsById(string id);
        #endregion

        #region GetCandidateByPersonId
        Task<Candidate> GetCandidateByPersonId(string personId);
        #endregion

        #region InsertCandidate
        void InsertCandidate(Candidate entity);
        #endregion

        #region UpdateCandidate
        void UpdateCandidate(Candidate entity);
        #endregion

        #region DeleteCandidate
        void DeleteCandidate(Candidate entity);
        #endregion

        #region InsertPicture
        void InsertPicture(Picture entity);
        #endregion

        #region UpdatePicture
        void UpdatePicture(Picture entity);
        #endregion
    }
}
