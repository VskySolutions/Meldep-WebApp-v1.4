using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.AdPosts
{
    public interface IAdPostChannelService
    {
        #region GetAllAdPostChannel
        IPagedList<AdPostChannel> GetAllAdPostChannel(string SiteId, string SearchText, List<string> projectIds, string name, List<string> customerIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetAdPostChannelById
        Task<AdPostChannel> GetAdPostChannelById(string id);
        #endregion

        #region GetChannelNumber
        Task<AdPostChannel> GetChannelNumber(string SiteId);
        #endregion

        #region GetAdPostChannelByName
        Task<AdPostChannel> GetAdPostChannelByName(string name, string ProjectId = null, string id = null);
        #endregion

        #region GetAllAdPostChannelsListForDropdown
        Task<List<CommonDropDown>> GetAllAdPostChannelsListForDropdown(string SiteId);
        #endregion

        #region GetAdPostChannelDetailsById
        Task<AdPostChannel> GetAdPostChannelDetailsById(string id);
        #endregion

        #region InsertAdPostChannel
        void InsertAdPostChannel(AdPostChannel entity);
        #endregion

        #region UpdateAdPostChannel
        void UpdateAdPostChannel(AdPostChannel entity);
        #endregion

        #region DeleteAdPostChannel
        void DeleteAdPostChannel(AdPostChannel entity);
        #endregion
    }
}
