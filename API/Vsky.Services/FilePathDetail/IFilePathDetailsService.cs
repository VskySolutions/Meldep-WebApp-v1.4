using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.FilePathDetail
{
    public interface IFilePathDetailsService
    {
        #region GetFilePathById
        Task<FilePathDetails> GetFilePathById(string id);
        #endregion

        #region GetFilePathDetailsById
        Task<FilePathDetails> GetFilePathDetailsById(string id);
        #endregion

        #region InsertFilePathDetails
        void InsertFilePathDetails(IList<FilePathDetails> entity);
        #endregion

        #region UpdateFilePathDetails
        void UpdateFilePathDetails(IList<FilePathDetails> entity);
        #endregion

        #region DeleteFilePathDetails
        void DeleteFilePathDetails(List<FilePathDetails> entity);
        #endregion
    }
}
