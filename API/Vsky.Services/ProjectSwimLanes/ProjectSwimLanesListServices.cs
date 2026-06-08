using System.Linq;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Data;
using Vsky.Models;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Services.ProjectSwimLanes
{
    public class ProjectSwimLanesListServices : IProjectSwimLanesListServices
    {
        #region Define Services
        private readonly IRepository<ProjectSwimLanesList> _ProjectSwimLaneListRepository;
        #endregion

        #region Services Initializations
        public ProjectSwimLanesListServices(IRepository<ProjectSwimLanesList> ProjectSwimLaneListRepository)
        {
            _ProjectSwimLaneListRepository = ProjectSwimLaneListRepository;
        }
        #endregion

        #region Get By Id
        public async Task<ProjectSwimLanesList> GetById(string id)
        {
            return await _ProjectSwimLaneListRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region Get All List's By ProjectSwimlaneId
        public async Task<List<ProjectSwimLanesList>> GetAllListByProjectSwimlaneId(string ProjectSwimlaneId)
        {
            return await _ProjectSwimLaneListRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectSwimlaneId == ProjectSwimlaneId).ToListAsync();
        }
        #endregion

        #region CRUD
        public void InsertProjectSwimLaneList(ProjectSwimLanesList entity)
        {
            _ProjectSwimLaneListRepository.Insert(entity);
        }
        public void UpdateProjectSwimLaneList(ProjectSwimLanesList entity)
        {
            _ProjectSwimLaneListRepository.Update(entity);
        }
        public void DeleteProjectSwimLaneList(ProjectSwimLanesList entity)
        {
            entity.Deleted = true;
            _ProjectSwimLaneListRepository.Update(entity);
        }
        #endregion
    }
}
