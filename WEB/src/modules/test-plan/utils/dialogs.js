import { useQuasar } from "quasar";
import addEditTestPlan from "modules/test-plan/components/addEdit.vue";
import viewTestPlan from "modules/test-plan/components/view.vue";
import excelImportDialog from "src/components/excel-import/_excelImportDialog.vue";
import testPlanService from "modules/test-plan/testPlan.service";
import { TEST_PLAN_IMPORT_CONFIG } from "modules/test-plan/utils/importFlow.js";
import { onTestPlanTemplateDownload } from "modules/test-plan/utils/actions.js";

let $q;
let activeRowId;

export function initTestPlanDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onTestPlanView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewTestPlan,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onTestPlanAdd (refresh) {
  // const componentProps = {};
  //if (projectId) componentProps.projectIdAttr = projectId;
  $q.dialog({
    component: addEditTestPlan,
    componentProps : {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

// Excel import: list refreshes only after a confirmed successful import (dialog OK).
export function onTestPlanImport (refresh) {
  $q.dialog({
    component: excelImportDialog,
    componentProps: {
      config: TEST_PLAN_IMPORT_CONFIG,
      service: testPlanService,
      downloadTemplate: onTestPlanTemplateDownload
    }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onTestPlanEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditTestPlan,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
