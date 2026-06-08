import { useQuasar } from "quasar";
import addEditTestCase from "modules/test-case/components/addEdit.vue";
import viewTestCase from "modules/test-case/components/view.vue";

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
