import { useQuasar } from "quasar";
import addEditSOPProcess from "modules/sop-process/components/addEdit.vue";
import viewSOPProcess from "modules/sop-process/components/view.vue";
import sopProcessStatusLog from "modules/sop-process/components/_sopProcessStatusLog.vue";

let $q;
let activeRowId;

export function initSOPProcessDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onSOPProcessView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewSOPProcess,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onSOPProcessAdd (refresh) {
  $q.dialog({
    component: addEditSOPProcess,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onSOPProcessEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditSOPProcess,
    componentProps: { id }
  }).onOk(() => {
    refresh();
    activeRowId.value = id;
  })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { });
}

export function onSOPProcessStatusLog (id) {
  activeRowId.value = id;
  $q.dialog({
    component: sopProcessStatusLog,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}
