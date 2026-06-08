import { useQuasar } from "quasar";
import addEditInfraAccount from "modules/infra-account/components/addEdit.vue";
import viewInfraAccount from "modules/infra-account/components/view.vue";

let $q;
let activeRowId;

export function initInfraAccountDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onInfraAccountView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewInfraAccount,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onInfraAccountAdd (refresh) {
  $q.dialog({
    component: addEditInfraAccount,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onInfraAccountEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditInfraAccount,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}
