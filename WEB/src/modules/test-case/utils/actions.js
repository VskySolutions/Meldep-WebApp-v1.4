import { useQuasar } from "quasar";
import { ref, nextTick } from "vue";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import testcasesService from "modules/test-case/testCase.service";
import { downloadBlob, describeDownloadError } from "composables/excelImport/downloadBlob.js";

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

// Download the backend-generated Excel import template (can take a few seconds on large sites)
export const templateDownloading = ref(false);

export const onTestCaseTemplateDownload = async () => {
  if (templateDownloading.value) return;
  templateDownloading.value = true;
  try {
    const blob = await testcasesService.downloadImportTemplate();
    downloadBlob(blob, "TestCaseImportTemplate.xlsx");
  } catch (error) {
    sendError(await describeDownloadError(error, "Unable to download the Excel template."), error);
  } finally {
    templateDownloading.value = false;
  }
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
