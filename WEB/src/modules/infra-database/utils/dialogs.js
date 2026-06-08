import { useQuasar } from "quasar";
import viewInfraDatabase from "modules/infra-database/components/view.vue";

let $q;
let activeRowId;

export function initDatabaseDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onDatabaseView (id, isAssign = false, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewInfraDatabase,
    componentProps: { id, isAssign }
  }).onOk(() => { activeRowId.value = id; refresh?.(); })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; refresh?.(); });
}
