using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vsky.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Vsky.Services.ProjectSwimLanes
{
    public class MasterProjectSwimlaneListsServices : IMasterProjectSwimlaneListsServices
    {
        #region Define Services
        private readonly IRepository<Models.MasterProjectSwimlaneLists> _MasterProjectSwimlaneListsRepository;
        #endregion

        #region Services Initializations
        public MasterProjectSwimlaneListsServices(
            IRepository<Models.MasterProjectSwimlaneLists> MasterProjectSwimlaneListsRepository)
        {
            _MasterProjectSwimlaneListsRepository = MasterProjectSwimlaneListsRepository;
        }
        #endregion

        #region GetById
        public async Task<Models.MasterProjectSwimlaneLists> GetById(string id)
        {
            return await _MasterProjectSwimlaneListsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region Get List By SwimlaneTypeId
        public async Task<List<Models.MasterProjectSwimlaneLists>> GetMasterProjectSwimlaneBySwimlaneTypeId(string SwimlaneTypeId)
        {
            return await _MasterProjectSwimlaneListsRepository.TableNoTracking.Where(x => !x.Deleted && x.SwimlaneTypeId == SwimlaneTypeId).ToListAsync();
        }
        #endregion

        #region Validate Master ListName
        public async Task<bool> ValidateListName(string siteId, string name)
        {
            var query = await _MasterProjectSwimlaneListsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.Name.ToLower() == name.ToLower()).CountAsync();
            var IsValidName = query > 0 ? false : true;
            return IsValidName;
        }
        #endregion

        #region CRUD
        // Title: InsertMessage
        public void InsertMasterProjectSwimlaneList(Models.MasterProjectSwimlaneLists entity)
        {
            _MasterProjectSwimlaneListsRepository.Insert(entity);
        }
        public void UpdateMasterProjectSwimlaneList(Models.MasterProjectSwimlaneLists entity)
        {
            _MasterProjectSwimlaneListsRepository.Update(entity);
        }
        public void DeleteMasterProjectSwimlaneList(Models.MasterProjectSwimlaneLists entity)
        {
            entity.Deleted = true;
            _MasterProjectSwimlaneListsRepository.Update(entity);
        }
        #endregion
    }
}