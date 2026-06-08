using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.JobsCreate
{
    public interface IJobCreateService
    {
        #region GetAllJobPosts
        IPagedList<JobCreate> GetAllJobPosts(string SiteId, string SearchText, string jobTitle, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetJobPostById
        Task<JobCreate> GetJobPostById(string id);
        #endregion

        #region GetAllJobPostListForDropdown
        Task<List<CommonDropDown>> GetAllJobPostListForDropdown(string SiteId);
        #endregion

        #region GetJobPostDetailsById
        Task<JobCreate> GetJobPostDetailsById(string id);
        #endregion

        #region GetJobPostByTitle
        Task<JobCreate> GetJobPostByTitle(string SiteId, string title, string id = null);
        #endregion

        #region InsertJobPost
        void InsertJobPost(JobCreate entity);
        #endregion

        #region UpdateJobPost
        void UpdateJobPost(JobCreate entity);
        #endregion

        #region DeleteJobPost
        void DeleteJobPost(JobCreate entity);
        #endregion
    }
}