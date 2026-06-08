import { useQuasar } from "quasar";
import viewInfraFTP from "modules/infra-ftp/components/view.vue";

let $q;
let activeRowId;

export function initFTPDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onFTPView (id, isAssign = false, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewInfraFTP,
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
