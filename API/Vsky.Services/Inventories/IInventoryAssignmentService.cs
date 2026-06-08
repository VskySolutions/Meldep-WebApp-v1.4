using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Inventories
{
    public interface IInventoryAssignmentService
    {
        #region GetInventoryAssignmentById
        Task<InventoryAssignment> GetInventoryAssignmentById(string id);
        #endregion

        #region GetInventoryAssignmentByInventoryId
        Task<InventoryAssignment> GetInventoryAssignmentByInventoryId(string employeeId, string inventoryId);
        #endregion

        #region GetInventoryAssignmentsByInventoryId
        Task<List<InventoryAssignment>> GetInventoryAssignmentsByInventoryId(string inventoryId);
        #endregion

        #region InsertInventoryAssignmentList
        void InsertInventoryAssignmentList(IList<InventoryAssignment> entities);
        #endregion

        #region UpdateInventoryAssignmentList
        void UpdateInventoryAssignmentList(IList<InventoryAssignment> entities);
        #endregion

        #region DeleteInventoryAssignmentList
        void DeleteInventoryAssignmentList(List<InventoryAssignment> entities);
        #endregion
    }
}

