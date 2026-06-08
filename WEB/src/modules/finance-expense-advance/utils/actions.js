import { useQuasar } from "quasar";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import financeExpenseAdvanceService from "../financeExpenseAdvance.service";

let $q;
let activeRowId;

export function initAdvanceExpenseActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// -------------------------------------------------------------
// Delete Expense
// -------------------------------------------------------------

export const onSubmitAdvanceExpenseDelete = async (
  id,
  referenceId,
  refreshAdvanceExpenseList
) => {
  activeRowId.value = id;
  zwConfirmDelete(
    { data: `${referenceId}` },
    async () => {
      try {
        await financeExpenseAdvanceService.delete(id);
        notifySuccess({ message: "Advance expense is deleted successfully." });
        refreshAdvanceExpenseList();
      } catch (error) {
        sendError("Error deleting advance expense ", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

