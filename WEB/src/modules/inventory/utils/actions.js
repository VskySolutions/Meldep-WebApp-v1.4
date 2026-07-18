import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import inventoryService from "modules/inventory/inventory.service";

let $q;
let activeRowId;

export function initInventoryActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete Inventory
export const onSubmitInventoryDelete = async (
  id,
  itemTypeName,
  refreshInventoryList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${itemTypeName}` },
    async () => {
      try {
        await inventoryService.deleteInventory(id);
        notifySuccess({ message: "Inventory is deleted successfully." });
        refreshInventoryList();
      } catch (error) {
        sendError("Error deleting issue", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
