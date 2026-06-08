import { useQuasar } from "quasar";
import addEditRequirement from "modules/requirement/components/addEdit.vue";
import viewRequirement from "modules/requirement/components/view.vue";

let $q;
let activeRowId;

export function initRequirementDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onRequirementView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewRequirement,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onRequirementAdd (refresh) {
  $q.dialog({
    component: addEditRequirement,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onRequirementEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditRequirement,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
