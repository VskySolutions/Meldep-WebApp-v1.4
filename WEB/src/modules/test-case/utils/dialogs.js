import { useQuasar } from "quasar";
import addEditTestCase from "modules/test-case/components/addEdit.vue";
import viewTestCase from "modules/test-case/components/view.vue";
import viewTestCaseReleaseHistory from "modules/test-case/components/viewTestCaseReleaseHistory.vue";
import testCaseStatusLog from "modules/test-case/components/_testCaseStatusLog.vue";
import siteStatusLog from "modules/sites/components/_siteModifiedLogs.vue";

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
