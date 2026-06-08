using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.Inventories
{
    public class InventoryItemTypeService : IInventoryItemTypeService
    {
        #region Define Services
        /// <summary>
        /// Define Services
        /// </summary>
        private readonly IRepository<InventoryItemType> _inventoryItemTypeRepository;
        #endregion

        #region Services Initializations
        /// <summary>
        /// Services Initializations
        /// </summary>
        /// <param name="inventoryItemTypeRepository"></param>
        public InventoryItemTypeService(IRepository<InventoryItemType> inventoryItemTypeRepository)
        {
            _inventoryItemTypeRepository = inventoryItemTypeRepository;
        }
        #endregion

        #region GetAllItemType
        public async Task<List<InventoryItemType>> GetAllItemType(string SiteId)
        {
            var query = _inventoryItemTypeRepository.TableNoTracking.Where(x => x.SiteId == SiteId);
            query = query.Select(x => new InventoryItemType
            {
                Id = x.Id,
                Name = x.Name
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetInventoryItemType
        public async Task<InventoryItemType> GetInventoryItemType(string siteId, string itemTypeId)
        {
            return await _inventoryItemTypeRepository.TableNoTracking.Where (m =>m.SiteId == siteId && m.Id == itemTypeId).FirstOrDefaultAsync();
        }

        #endregion
    }
}
