import { useQuasar } from "quasar";
import addEditTestCase from "modules/test-case/components/addEdit.vue";
import viewTestCase from "modules/test-case/components/view.vue";
import viewTestCaseReleaseHistory from "modules/test-case/components/viewTestCaseReleaseHistory.vue";
import testCaseStatusLog from "modules/test-case/components/_testCaseStatusLog.vue";
import siteStatusLog from "modules/sites/components/_siteModifiedLogs.vue";
import excelImportDialog from "src/components/excel-import/_excelImportDialog.vue";
import testCaseService from "modules/test-case/testCase.service";
import { TEST_CASE_IMPORT_CONFIG } from "modules/test-case/utils/importFlow.js";
import { onTestCaseTemplateDownload } from "modules/test-case/utils/actions.js";

let $q;
let activeRowId;

export function initTestCaseDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onTestCaseView (id, testPlanId) {
  activeRowId.value = id;
  $q.dialog({
    component: viewTestCase,
    componentProps: { id, testPlanId }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onTestCaseReleaseHistory (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewTestCaseReleaseHistory,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onTestCaseAdd (refresh) {
  $q.dialog({
    component: addEditTestCase,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

// Excel import: list refreshes only after a confirmed successful import (dialog OK).
export function onTestCaseImport (refresh) {
  $q.dialog({
    component: excelImportDialog,
    componentProps: {
      config: TEST_CASE_IMPORT_CONFIG,
      service: testCaseService,
      downloadTemplate: onTestCaseTemplateDownload
    }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onTestCaseEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditTestCase,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onTestCaseStatusChangeLog(
  releaseVersion,
  testCaseId,
  mappingId,
  name,
  columnName
) {
  activeRowId.value = mappingId ?? testCaseId;

  if (mappingId === null || mappingId === undefined || mappingId === "") {
    $q.dialog({
      component: siteStatusLog,
      componentProps: {
        id: testCaseId,
        name,
        columnName
      }
    })
      .onOk(() => {})
      .onCancel(() => {
        activeRowId.value = testCaseId;
      })
      .onDismiss(() => {
        activeRowId.value = testCaseId;
      });
  } else {
    $q.dialog({
      component: testCaseStatusLog,
      componentProps: {
        releaseVersion,
        id: mappingId,
        name
      }
    })
      .onOk(() => {})
      .onCancel(() => {
        activeRowId.value = mappingId;
      })
      .onDismiss(() => {
        activeRowId.value = mappingId;
      });
  }
}
