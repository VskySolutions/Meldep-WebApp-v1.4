import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import testplansService from "modules/test-plan/testPlan.service";

let $q;
let activeRowId;

export function initTestPlanActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete Test Plan
export const onSubmitTestPlanDelete = async (
  id,
  testPlanName,
  refreshTestPlanList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${testPlanName}` },
    async () => {
      try {
        await testplansService.deleteTestPlan(id);
        notifySuccess({ message: "Test Plan is deleted successfully." });
        refreshTestPlanList();
      } catch (error) {
        sendError("Error deleting Test Plan", error);
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
