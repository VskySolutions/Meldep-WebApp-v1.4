using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectsColor
{
    public interface IProjectsColorService
    {
        #region GetProjectsColorByUser
        Task<ProjectColor> GetProjectsColorByUser(string projectId, string LoggedUserId);
        #endregion

        #region InsertProjectColor
        void InsertProjectColor(ProjectColor entity);
        #endregion

        #region UpdateProjectColor
        void UpdateProjectColor(ProjectColor entity);
        #endregion
    }
}
