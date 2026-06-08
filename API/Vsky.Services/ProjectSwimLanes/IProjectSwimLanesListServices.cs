using System.Threading.Tasks;
using System.Collections.Generic;
using Vsky.Models;

namespace Vsky.Services.ProjectSwimLanes
{
    public interface IProjectSwimLanesListServices
    {
        #region Get By Id
        Task<ProjectSwimLanesList> GetById(string id);
        #endregion

        #region  Get All List's By ProjectSwimlaneId
        Task<List<ProjectSwimLanesList>> GetAllListByProjectSwimlaneId(string ProjectSwimlaneId);
        #endregion

        #region CRUD
        void InsertProjectSwimLaneList(ProjectSwimLanesList entity);
        void UpdateProjectSwimLaneList(ProjectSwimLanesList entity);
        void DeleteProjectSwimLaneList(ProjectSwimLanesList entity);
        #endregion
    }
}
