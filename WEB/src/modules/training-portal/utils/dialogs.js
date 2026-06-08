import { useQuasar } from "quasar";
import viewTraining from "modules/training-portal/components/view.vue";
import addEditTraining from "modules/training-portal/components/addEdit.vue";

let $q;
let activeRowId;

export function initTrainingPortalDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onTrainingPortalView (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewTraining,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
}

export function onTrainingPortalAdd (refresh) {
  $q.dialog({
    component: addEditTraining,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onTrainingPortalEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditTraining,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
