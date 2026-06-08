using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public interface IProjectTaskTagService
    {
        #region GetProjectTaskTagByUserId
        Task<List<CommonDropDown>> GetProjectTaskTagByUserId(string LoggedUserId);
        #endregion

        Task<ProjectTask_Tags> GetByNameTaskIdAndUserId(string SiteId, string Name, string TaskId, string LoggedUserId);

        #region GetProjectTaskTagsByTaskIdAndUserId
        // Title: GetProjectTaskTagsByTaskIdAndUserId
        List<ProjectTask_Tags> GetProjectTaskTagsByTaskIdAndUserId(string siteId, string taskId, string LoggedUserId);
        #endregion

        #region InsertProjectTaskTag
        void InsertProjectTaskTag(ProjectTask_Tags entity);
        #endregion

        #region DeleteTaskTag
        void DeleteTaskTag(ProjectTask_Tags entity);
        #endregion
    }
}
