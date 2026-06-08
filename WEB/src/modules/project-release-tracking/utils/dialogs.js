import { useQuasar } from "quasar";
import addEditReleaseTracking from "modules/project-release-tracking/components/addEdit.vue";
import viewReleaseTracking from "modules/project-release-tracking/components/view.vue";

let $q;
let activeRowId;

export function initReleaseTrackingDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onReleaseTrackingView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewReleaseTracking,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onReleaseTrackingAdd (refresh) {
  $q.dialog({
    component: addEditReleaseTracking,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onReleaseTrackingEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditReleaseTracking,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
