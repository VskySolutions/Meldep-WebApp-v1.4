import { useQuasar } from "quasar";
import siteStatusLog from "modules/sites/components/_siteModifiedLogs.vue";

let $q;
let activeRowId;

export function initSiteDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onSiteModifiedLog (id, name, columnName) {
  activeRowId.value = id;
  $q.dialog({
    component: siteStatusLog,
    componentProps: { id, name, columnName }
  }).onOk(() => { })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}
