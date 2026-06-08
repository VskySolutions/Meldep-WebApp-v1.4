import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import infraProjectInstanceService from "modules/infra-project-instance/infraProjectInstance.service";

let activeRowId;

export function initInfraProjectInstanceActions (rowRef) {
  activeRowId = rowRef;
}

// Delete InfraProjectInstance
export const onSubmitInfraProjectInstanceDelete = async (
  item,
  index,
  rows,
  rowValidations,
  refreshInfraProjectInstanceList
) => {
  activeRowId.value = item.id;

  if (item.isNew) {
    if (rows.filter(r => !r.deleted).length <= 1) {
      notifyError({ message: "Please add at least one row." });
      return;
    }

    rows.splice(index, 1);
    rowValidations.splice(index, 1);
    activeRowId.value = null;
    return;
  }

  zwConfirmDelete(
    { data: `${item.infraProject?.name ?? ""}` },
    async () => {
      try {
        await infraProjectInstanceService.deleteInfraProjectInstance(item.id);

        notifySuccess({
          message: "Infra project instance is deleted successfully."
        });

        refreshInfraProjectInstanceList();
      } catch (error) {
        sendError("Error deleting Infra Project Instance", error);
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
