import { useQuasar } from "quasar";

import addEditPerson from "modules/person/components/addEdit.vue";
import viewPerson from "modules/person/components/view.vue";

let $q;
let activeRowId;

export function initPersonDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onPersonView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewPerson,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onPersonAdd (refresh, refreshPersonNameDropdown) {
  $q.dialog({
    component: addEditPerson,
    componentProps: {}
  }).onOk(() => {
    refresh?.();
    refreshPersonNameDropdown?.();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onPersonAddAndReturnPersonId (refreshPersonNameDropdown, onSuccess) {
  $q.dialog({
    component: addEditPerson,
    componentProps: {}
  }).onOk(async (newPersonId) => {
    await refreshPersonNameDropdown?.();
    // pass newPersonId
    onSuccess?.(newPersonId);
  })
  .onCancel(() => {})
  .onDismiss(() => {});
}

export function onPersonEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditPerson,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
