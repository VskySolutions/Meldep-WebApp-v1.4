using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public interface IProjectTaskFilesService
    {
        #region GetProjectTaskFileById
        Task<ProjectTaskFiles> GetProjectTaskFileById(string id);
        #endregion

        #region GetProjectTaskFileByFileId
        Task<ProjectTaskFiles> GetProjectTaskFileByFileId(string fileId);
        #endregion

        #region GetAllProjectTaskFilesByProjectTaskId
        Task<List<ProjectTaskFiles>> GetAllProjectTaskFilesByProjectTaskId(string projectTaskId);
        #endregion

        #region InsertProjectTaskFile
        void InsertProjectTaskFile(ProjectTaskFiles entity);
        #endregion

        #region UpdateProjectTaskFile
        void UpdateProjectTaskFile(ProjectTaskFiles entity);
        #endregion

        #region DeleteProjectTaskFile
        void DeleteProjectTaskFile(ProjectTaskFiles entity);
        #endregion
    }
}


