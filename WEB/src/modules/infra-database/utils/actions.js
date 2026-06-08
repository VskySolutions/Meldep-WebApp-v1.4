import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import infraDatabaseService from "modules/infra-database/infraDatabase.service";

let activeRowId;

export function initInfraDatabaseActions (rowRef) {
  activeRowId = rowRef;
}

// Delete InfraAccountService
export const onSubmitInfraDatabaseDelete = async (
  item,
  index,
  rows,
  rowValidations,
  refreshInfraDatabaseList
) => {
  activeRowId.value = item.id;

  // Handle new (unsaved) row delete
  if (item.isNew) {
    if (rows.filter(r => !r.deleted).length <= 1) {
      notifyError({ message: "Please add at least one row." });
      activeRowId.value = null;
      return;
    }

    rows.splice(index, 1);
    rowValidations.splice(index, 1);
    activeRowId.value = null;
    return;
  }

  // Existing row delete
  zwConfirmDelete(
    { data: `${item.name ?? ""}` },
    async () => {
      try {
        await infraDatabaseService.deleteInfraDatabase(item.id);

        notifySuccess({
          message: "Infra Database is deleted successfully."
        });

        refreshInfraDatabaseList();
      } catch (error) {
        sendError("Error deleting Infra Database", error);
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
