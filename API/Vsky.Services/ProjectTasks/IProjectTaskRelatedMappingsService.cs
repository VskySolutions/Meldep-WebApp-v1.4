using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public interface IProjectTaskRelatedMappingsService
    {
        #region InsertProjectTaskRelatedMapping
        void InsertProjectTaskRelatedMapping(ProjectTaskRelatedMapping entity);
        #endregion

        #region UpdateProjectTaskRelatedMapping
        void UpdateProjectTaskRelatedMapping(ProjectTaskRelatedMapping entity);
        #endregion

        #region DeleteProjectTaskRelatedMapping
        void DeleteProjectTaskRelatedMapping(ProjectTaskRelatedMapping entity);
        #endregion
    }
}
