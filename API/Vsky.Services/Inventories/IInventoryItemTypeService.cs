using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Inventories
{
    public interface IInventoryItemTypeService
    {
        #region GetAllItemType
        Task<List<InventoryItemType>> GetAllItemType(string SiteId);
        #endregion

        #region GetInventoryItemType
        Task<InventoryItemType> GetInventoryItemType(string SiteId,string itemTypeId);
        #endregion
    }
}
