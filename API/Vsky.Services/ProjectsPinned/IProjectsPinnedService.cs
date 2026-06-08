using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectsPinned
{
    public interface IProjectsPinnedService
    {
        #region GetProjectPinnedByUser
        Task<ProjectPinned> GetProjectPinnedByUser(string projectId, string LoggedUserId);
        #endregion

        #region InsertProjectPin
        void InsertProjectPin(ProjectPinned entity);
        #endregion
    }
}
