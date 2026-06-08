using System.Threading.Tasks;
using System.Collections.Generic;
using Vsky.Models;

namespace Vsky.Services.ProjectSwimLanes
{
    public interface IProjectSwimLanesListsTasksServices
    {
        #region Get By Id
        Task<ProjectSwimLanesListsTasks> GetById(string id);
        #endregion

        #region  Get All List's By ProjectSwimlaneId
        Task<List<ProjectSwimLanesListsTasks>> GetAllTasksByProjectSwimlaneListId(string ProjectSwimlaneListId);
        #endregion

        #region CRUD
        void InsertProjectSwimLaneListsTasks(ProjectSwimLanesListsTasks entity);
        void UpdateProjectSwimLaneListsTasks(ProjectSwimLanesListsTasks entity);
        void DeleteProjectSwimLaneListsTasks(ProjectSwimLanesListsTasks entity);
        #endregion
    }
}
