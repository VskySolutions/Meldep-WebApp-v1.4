using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public interface IProjectTaskStatusLogService
    {
        #region GetAllProjectTaskStatus
        IPagedList<ProjectTaskStatusLog> GetAllProjectTaskStatus(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetProjectTaskStatusLogById
        Task<ProjectTaskStatusLog> GetProjectTaskStatusLogById(string id);
        #endregion

        #region GetProjectTaskStatusLogDetailsById
        Task<ProjectTaskStatusLog> GetProjectTaskStatusLogDetailsById(string id);
        #endregion

        #region InsertProjectTaskStatusLog
        void InsertProjectTaskStatusLog(ProjectTaskStatusLog entity);
        #endregion

        #region UpdateProjectTaskStatusLog
        void UpdateProjectTaskStatusLog(ProjectTaskStatusLog entity);
        #endregion

        //#region DeleteProjectTaskStatusLog
        //void DeleteProjectTaskStatusLog(ProjectTaskStatusLog entity);
        //#endregion
    }
}

