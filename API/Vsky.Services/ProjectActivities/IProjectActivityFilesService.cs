using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectActivities
{
    public interface IProjectActivityFilesService
    {
        #region GetProjectActivityFileById
        Task<ProjectActivityFiles> GetProjectActivityFileById(string id);
        #endregion

        #region GetProjectActivityFileByFileId
        Task<ProjectActivityFiles> GetProjectActivityFileByFileId(string fileId);
        #endregion

        #region GetAllProjectActivityFilesByProjectActivityId
        Task<List<ProjectActivityFiles>> GetAllProjectActivityFilesByProjectActivityId(string projectActivityId);
        #endregion

        #region InsertProjectActivityFile
        void InsertProjectActivityFile(ProjectActivityFiles entity);
        #endregion

        #region UpdateProjectActivityFile
        void UpdateProjectActivityFile(ProjectActivityFiles entity);
        #endregion

        #region DeleteProjectActivityFile
        void DeleteProjectActivityFile(ProjectActivityFiles entity);
        #endregion
    }
}


