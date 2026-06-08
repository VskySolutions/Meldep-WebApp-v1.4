using System.Linq;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Data;
using Vsky.Models;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Services.ProjectSwimLanes
{
    public class ProjectSwimLanesListsTasksServices : IProjectSwimLanesListsTasksServices
    {
        #region Define Services
        private readonly IRepository<ProjectSwimLanesListsTasks> _ProjectSwimLaneListsTasksRepository;
        #endregion

        #region Services Initializations
        public ProjectSwimLanesListsTasksServices(IRepository<ProjectSwimLanesListsTasks> ProjectSwimLaneListsTasksRepository)
        {
            _ProjectSwimLaneListsTasksRepository = ProjectSwimLaneListsTasksRepository;
        }
        #endregion

        #region Get By Id
        public async Task<ProjectSwimLanesListsTasks> GetById(string id)
        {
            return await _ProjectSwimLaneListsTasksRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region Get All Task's By ProjectSwimlaneListId
        public async Task<List<ProjectSwimLanesListsTasks>> GetAllTasksByProjectSwimlaneListId(string ProjectSwimlaneListId)
        {
            return await _ProjectSwimLaneListsTasksRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectSwimlaneListId == ProjectSwimlaneListId).ToListAsync();
        }
        #endregion

        #region CRUD
        public void InsertProjectSwimLaneListsTasks(ProjectSwimLanesListsTasks entity)
        {
            _ProjectSwimLaneListsTasksRepository.Insert(entity);
        }
        public void UpdateProjectSwimLaneListsTasks(ProjectSwimLanesListsTasks entity)
        {
            _ProjectSwimLaneListsTasksRepository.Update(entity);
        }
        public void DeleteProjectSwimLaneListsTasks(ProjectSwimLanesListsTasks entity)
        {
            entity.Deleted = true;
            _ProjectSwimLaneListsTasksRepository.Update(entity);
        }
        #endregion
    }
}
