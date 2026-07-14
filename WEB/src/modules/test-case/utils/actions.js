import { useQuasar } from "quasar";
import { ref, nextTick } from "vue";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import testcasesService from "modules/test-case/testCase.service";

let $q;
let activeRowId;

export function initTestCaseActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export const updatingRow = ref({
  status: null
});

// Update Test Case Status
export const onSubmitTestCaseStatus = async (
  testCaseId,
  statusId,
  mappingId,
  refreshTestCaseList
) => {
  try {
    await withRowLoader(
      "status",
      testCaseId,
      () => testcasesService.updateTestCaseStatus(testCaseId, statusId, mappingId),
      "Test case status updated successfully.",
      "Failed to update test case status.",
      refreshTestCaseList
    );
  } catch (error) {
    sendError("Error updating test case status", error);
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

const withRowLoader = async (
  field,
  rowId,
  apiCall,
  successMessage = "Updated successfully.",
  errorMessage = "Update failed.",
  afterSuccess = null
) => {
  updatingRow.value[field] = rowId;

  await nextTick();
  document.activeElement?.blur();

  try {
    await apiCall();

    if (afterSuccess) {
      await afterSuccess();
    }

    notifySuccess({ message: successMessage });
  } catch (error) {
    notifyError({ message: errorMessage });
  } finally {
    updatingRow.value[field] = null;
  }
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
