import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import infraFTPService from "modules/infra-ftp/infraFTP.service";

let activeRowId;

export function initInfraFTPActions (rowRef) {
  activeRowId = rowRef;
}

// Delete InfraAccountService
export const onSubmitInfraFTPDelete = async (
  item,
  index,
  rows,
  rowValidations,
  refreshInfraFTPList
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
        await infraFTPService.deleteInfraFTP(item.id);

        notifySuccess({
          message: "Infra FTP is deleted successfully."
        });

        refreshInfraFTPList();
      } catch (error) {
        sendError("Error deleting Infra FTP", error);
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
