import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import testcasesService from "modules/test-case/testCase.service";

let $q;
let activeRowId;

export function initTestCaseActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Update Test Case Status
export const onSubmitTestCaseStatus = async (id, statusId, refreshTestCaseList) => {
  try {
    await testcasesService.updateTestCaseStatus(id, statusId);

    notifySuccess({ message: "Test case status is saved successfully." });
    refreshTestCaseList();
  } catch (error) {
    sendError("Error updating test case status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Delete Test Case
export const onSubmitTestCaseDelete = async (
  id,
  testCaseName,
  refreshTestCaseList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${testCaseName}` },
    async () => {
      try {
        await testcasesService.deleteTestCase(id);
        notifySuccess({ message: "Test Case is deleted successfully." });
        refreshTestCaseList();
      } catch (error) {
        sendError("Error deleting Test Case", error);
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
