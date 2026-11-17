import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import testplansService from "modules/test-plan/testPlan.service";
import { ref } from "vue";
import { downloadBlob, describeDownloadError } from "composables/excelImport/downloadBlob.js";

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

// Download the backend-generated Excel import template
export const templateDownloading = ref(false);

export const onTestPlanTemplateDownload = async () => {
  if (templateDownloading.value) return;
  templateDownloading.value = true;
  try {
    const blob = await testplansService.downloadImportTemplate();
    downloadBlob(blob, "TestPlanImportTemplate.xlsx");
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
