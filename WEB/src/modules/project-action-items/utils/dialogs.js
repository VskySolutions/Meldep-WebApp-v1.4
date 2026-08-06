import { useQuasar } from "quasar";
import addEditProjectActionItems from "modules/project-action-items/components/addEdit.vue";
import viewProjectActionItems from "modules/project-action-items/components/view.vue";

let $q;
let activeRowId;

export function initProjectActionItemsDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onProjectActionItemsView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewProjectActionItems,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectActionItemsAdd (refresh) {
  $q.dialog({
    component: addEditProjectActionItems,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onProjectActionItemsEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditProjectActionItems,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
