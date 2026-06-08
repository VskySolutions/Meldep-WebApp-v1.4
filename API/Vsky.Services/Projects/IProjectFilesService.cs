using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Projects
{
    public interface IProjectFilesService
    {
        #region GetProjectFileById
        Task<ProjectFiles> GetProjectFileById(string id);
        #endregion

        #region GetProjectFileByFileId
        Task<ProjectFiles> GetProjectFileByFileId(string fileId);
        #endregion

        #region GetAllProjectFileByProjectId
        Task<List<ProjectFiles>> GetAllProjectFileByProjectId(string siteId, string projectId);
        #endregion

        #region InsertProjectFile
        void InsertProjectFile(ProjectFiles entity);
        #endregion

        #region UpdateProjectFile
        void UpdateProjectFile(ProjectFiles entity);
        #endregion

        #region DeleteProjectFiles
        void DeleteProjectFiles(ProjectFiles entity);
        #endregion
    }
}

