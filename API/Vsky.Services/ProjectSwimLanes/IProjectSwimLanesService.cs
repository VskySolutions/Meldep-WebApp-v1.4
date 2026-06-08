using System.Threading.Tasks;

namespace Vsky.Services.ProjectSwimLanes
{
    public interface IProjectSwimLanesService
    {
        #region GetById
        Task<Models.ProjectSwimLanes> GetById(string id);
        Task<Models.ProjectSwimLanes> GetSwimlaneWithListsById(string id);
        #endregion

        #region GetProjectSwimLaneByName
        Task<Models.ProjectSwimLanes> GetProjectSwimLaneByName(string siteId, string name, string ProjectId = null, string id = null);
        #endregion

        #region CRUD
        void InsertProjectSwimLane(Models.ProjectSwimLanes entity);
        void UpdateProjectSwimLane(Models.ProjectSwimLanes entity);
        void DeleteProjectSwimLane(Models.ProjectSwimLanes entity);
        #endregion
    }
}
