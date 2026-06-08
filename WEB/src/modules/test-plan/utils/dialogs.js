import { useQuasar } from "quasar";
import addEditTestPlan from "modules/test-plan/components/addEdit.vue";
import viewTestPlan from "modules/test-plan/components/view.vue";

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
  $q.dialog({
    component: addEditTestPlan,
    componentProps: {}
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
