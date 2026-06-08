using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.AdPosts
{
    public interface IAdPostingStatusService
    {
        #region GetAdPostingStatusById
        Task<AdPostingStatus> GetAdPostingStatusById(string id);
        #endregion

        #region GetAdPostingStatusByAdId
        Task<AdPostingStatus> GetAdPostingStatusByAdId(string channelId, string adId);
        #endregion

        #region GetAdPostingStatusesByAdId
        Task<List<AdPostingStatus>> GetAdPostingStatusesByAdId(string adId);
        #endregion

        #region InsertAdPostingStatus
        void InsertAdPostingStatus(AdPostingStatus entity);
        #endregion

        #region UpdateAdPostingStatus
        void UpdateAdPostingStatus(AdPostingStatus entity);
        #endregion

        #region DeleteAdPostingStatus
        void DeleteAdPostingStatus(AdPostingStatus entity);
        #endregion
    }
}

