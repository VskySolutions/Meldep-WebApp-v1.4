import { useQuasar } from "quasar";

import viewLead from "modules/lead/components/view.vue";
import editLead from "modules/lead/components/addEdit.vue";
import addLeadActivities from "modules/lead/components/addActivity.vue";
import assignUserToLeadGroup from "modules/lead/components/assignUserToLeadGroup.vue";

let $q;
let activeRowId;

export function initLeadDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onLeadView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewLead,
    componentProps: { id }
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
}

export function onLeadAdd (refreshLeadNameDropdown, refresh) {
  $q.dialog({
    component: editLead,
    componentProps: { }
  }).onOk(() => {
    refreshLeadNameDropdown();
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onLeadEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editLead,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
}

export function onLeadAddActivity (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addLeadActivities,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
}

export function onLeadGroupAdd (refresh) {
  $q.dialog({
    component: assignUserToLeadGroup,
    componentProps: { }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
