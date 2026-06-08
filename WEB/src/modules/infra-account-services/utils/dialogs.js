import { useQuasar } from "quasar";
import addEditBulkInfraAccountServices from "modules/infra-account-services/components/addEditBulk.vue";
import viewInfraAccountServices from "modules/infra-account-services/components/view.vue";

let $q;
let activeRowId;

export function initInfraAccountServicesDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onInfraAccountServicesView (id, isAssign = false, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewInfraAccountServices,
    componentProps: { id, isAssign }
  }).onOk(() => {
    activeRowId.value = id;
    refresh?.();
  })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => {
      activeRowId.value = id;
      refresh?.();
  });
}

export function onInfraAccountServicesAddBulk (refresh) {
  $q.dialog({
    component: addEditBulkInfraAccountServices,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
