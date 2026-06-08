using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.InfraFTPs
{
    public interface IInfraFTPService
    {
        #region GetAllInfraFTPForList
        public IPagedList<InfraFTP> GetAllInfraFTPForList(
            string siteId,
            List<string> infraServiceIds,
            List<string> protocolTypeIds,
            List<string> encryptionTypeIds,
            string Name,
            string searchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetInfraFTPById
        Task<InfraFTP> GetInfraFTPById(string id);
        #endregion

        #region GetInfraFTPInDetailById
        Task<Models.InfraFTP> GetInfraFTPInDetailById(string id);
        #endregion

        #region InsertInfraFTP
        void InsertInfraFTP(Models.InfraFTP entity);
        #endregion

        #region UpdateInfraFTP
        void UpdateInfraFTP(Models.InfraFTP entity);
        #endregion

        #region DeleteInfraFTP
        void DeleteInfraFTP(Models.InfraFTP entity);
        #endregion
    }
}
