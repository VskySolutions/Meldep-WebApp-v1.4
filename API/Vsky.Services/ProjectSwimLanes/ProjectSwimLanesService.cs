using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vsky.Data;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Services.ProjectSwimLanes
{
    public class ProjectSwimLanesService : IProjectSwimLanesService
    {
        #region Define Services
        private readonly IRepository<Models.ProjectSwimLanes> _ProjectSwimLaneRepository;
        #endregion

        #region Services Initializations
        public ProjectSwimLanesService(
            IRepository<Models.ProjectSwimLanes> ProjectSwimLaneRepository)
        {
            _ProjectSwimLaneRepository = ProjectSwimLaneRepository;
        }
        #endregion

        #region GetById
        public async Task<Models.ProjectSwimLanes> GetById(string id)
        {
            return await _ProjectSwimLaneRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        
        public async Task<Models.ProjectSwimLanes> GetSwimlaneWithListsById(string id)
        {
            return await _ProjectSwimLaneRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == id)
                .Include(x => x.SwimlaneType)
                .Include(x => x.ProjectSwimLanesList.OrderBy(m => m.SortOrder))
                .FirstOrDefaultAsync();
        }
        #endregion

        #region GetProjectSwimLaneByName
        public async Task<Models.ProjectSwimLanes> GetProjectSwimLaneByName(string siteId, string name, string ProjectId, string id = null)
        {
            var query = _ProjectSwimLaneRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectId == ProjectId && x.Name.ToLower() == name.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region CRUD
        // Title: InsertMessage
        public void InsertProjectSwimLane(Models.ProjectSwimLanes entity)
        {
            _ProjectSwimLaneRepository.Insert(entity);
        }
        public void UpdateProjectSwimLane(Models.ProjectSwimLanes entity)
        {
            _ProjectSwimLaneRepository.Update(entity);
        }
        public void DeleteProjectSwimLane(Models.ProjectSwimLanes entity)
        {
            entity.Deleted = true;
            _ProjectSwimLaneRepository.Update(entity);
        }
        #endregion
    }
}
