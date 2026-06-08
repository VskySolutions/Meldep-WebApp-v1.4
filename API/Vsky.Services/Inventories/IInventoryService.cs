using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Inventories
{
    public interface IInventoryService
    {
        #region GetAllInventory
        IPagedList<Inventory> GetAllInventory(string SiteId, string SearchText, List<string> itemTypeIds, string code, List<string> inventoryStatusIds,List<string> employeeIds, List<string> officeLocationIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetInventoryById
        Task<Inventory> GetInventoryById(string id);
        #endregion

        #region GetInventoryDetailsById
        Task<Inventory> GetInventoryDetailsById(string id);
        #endregion

        #region GetInventoryCode
        Task<Inventory> GetInventoryCode(string SiteId, string itemTypeId);

        #endregion

        #region InsertInventory
        void InsertInventory(Inventory entity);
        #endregion

        #region UpdateInventory
        void UpdateInventory(Inventory entity);
        #endregion

        #region DeleteInventory
        void DeleteInventory(Inventory entity);
        #endregion
    }
}
