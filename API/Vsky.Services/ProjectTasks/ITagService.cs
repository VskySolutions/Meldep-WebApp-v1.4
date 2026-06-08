using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public interface ITagService
    {
        #region Tag list
        IPagedList<Tags> GetAllTags(string SiteId, string SearchText, string name, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue);

        Task<List<Tags>> GetAllTagList(string SiteId);
        #endregion

        Task<Tags> GetById(string id);

        #region GetTagByName
        Task<Tags> GetTagByName(string SiteId, string Name);
        #endregion

        #region InsertTags
        void InsertTags(Tags entity);
        #endregion

        Task<Tags> GetTagDetailsById(string id);

        #region UpdateTags
        void UpdateTags(Tags entity);
        #endregion

        #region DeleteTags
        void DeleteTags(Tags entity);
        #endregion

    }
}
