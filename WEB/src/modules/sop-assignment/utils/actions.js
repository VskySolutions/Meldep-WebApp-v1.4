import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import sopAssignmentService from "modules/sop-assignment/sopAssignment.service";

let activeRowId;

export function initSOPAssignmentActions (rowRef) {
  activeRowId = rowRef;
}

// Delete SOPAssignment
export const onSubmitSOPAssignmentDelete = async (
  id,
  SOPAssignmentName,
  refreshSOPAssignmentList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${SOPAssignmentName}` },
    async () => {
      try {
        await sopAssignmentService.deleteSOPAssignment(id);
        notifySuccess({ message: "SOP assignment is deleted successfully." });
        refreshSOPAssignmentList();
      } catch (error) {
        sendError("Error deleting SOPAssignment", error);
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
