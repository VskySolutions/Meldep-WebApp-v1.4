using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectModules
{
    public interface IProjectModuleFilesService
    {
        #region GetProjectModuleFileById
        Task<ProjectModuleFiles> GetProjectModuleFileById(string id);
        #endregion

        #region GetProjectModuleFileByFileId
        Task<ProjectModuleFiles> GetProjectModuleFileByFileId(string fileId);
        #endregion

        #region GetAllProjectModuleFilesByProjectModuleId
        Task<List<ProjectModuleFiles>> GetAllProjectModuleFilesByProjectModuleId(string projectModuleId);
        #endregion

        #region InsertProjectModuleFile
        void InsertProjectModuleFile(ProjectModuleFiles entity);
        #endregion

        #region UpdateProjectModuleFile
        void UpdateProjectModuleFile(ProjectModuleFiles entity);
        #endregion

        #region DeleteProjectModuleFile
        void DeleteProjectModuleFile(ProjectModuleFiles entity);
        #endregion
    }
}


