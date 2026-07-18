import { useQuasar } from "quasar";
import addEditIssue from "modules/issue/components/addEdit.vue";
import viewInventory from "modules/inventory/components/view_inventory.vue";
import addeditInventory from "modules/inventory/components/addedit_inventory.vue";

let $q;
let activeRowId;

export function initInventoryDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onInventoryView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewInventory,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onInventoryAdd (refresh) {
  $q.dialog({
    component: addEditIssue,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onInventoryEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addeditInventory,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
