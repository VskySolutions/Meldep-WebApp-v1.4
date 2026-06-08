using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.AdPosts
{
    public interface IAdPostService
    {
        #region GetAllAdPosts
        IPagedList<AdPost> GetAllAdPosts(string SiteId, string SearchText, List<string> projectIds, string name, List<string> customerIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetAdPostById
        Task<AdPost> GetAdPostById(string id);
        #endregion

        #region GetAdPostNumber
        Task<AdPost> GetAdPostNumber(string SiteId);
        #endregion

        #region GetAdPostByName
        Task<AdPost> GetAdPostByName(string name, string ProjectId = null, string id = null);
        #endregion

        //#region GetAllAdPostsListForDropdown
        //Task<List<AdPost>> GetAllTestPlansListForDropdown(string projectId = null);
        //#endregion

        #region GetAdPostDetailsById
        Task<AdPost> GetAdPostDetailsById(string id);
        #endregion

        #region InsertAdPost
        void InsertAdPost(AdPost entity);
        #endregion

        #region UpdateAdPost
        void UpdateAdPost(AdPost entity);
        #endregion

        #region DeleteAdPost
        void DeleteAdPost(AdPost entity);
        #endregion
    }
}
