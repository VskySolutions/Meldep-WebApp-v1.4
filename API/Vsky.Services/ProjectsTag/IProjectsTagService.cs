using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectsTag
{
    public interface IProjectsTagService
    {
        #region GetProjectTagByUserId
        Task<List<CommonDropDown>> GetProjectTagByUserId(string LoggedUserId);
        #endregion

        #region GetByNameProjectIdAndUserId
        Task<ProjectTags> GetByNameProjectIdAndUserId(string SiteId, string Name, string projectId, string LoggedUserId);
        #endregion

        #region GetProjectTagsByProjectIdAndUserId
        // Title: GetProjectTagsByProjectIdAndUserId
        List<ProjectTags> GetProjectTagsByProjectIdAndUserId(string siteId, string projectId, string LoggedUserId);
        #endregion

        #region InsertProjectTag
        void InsertProjectTag(ProjectTags entity);
        #endregion

        #region DeleteProjectTag
        void DeleteProjectTag(ProjectTags entity);
        #endregion
    }
}
