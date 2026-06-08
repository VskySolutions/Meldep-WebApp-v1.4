import { useQuasar } from "quasar";
import viewVendor from "modules/finance-expense-vendors/components/view.vue";
import addEditVendor from "modules/finance-expense-vendors/components/addEdit.vue";

let $q;
let activeRowId;

export function initVendorDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onVendorView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewVendor,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onVendorAdd (refresh) {
  $q.dialog({
    component: addEditVendor,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onVendorEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditVendor,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
