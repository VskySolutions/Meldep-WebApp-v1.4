import { useQuasar } from "quasar";
import assignSOPTemplate from "modules/sop-assignment/components/addEdit.vue";

let $q;
let activeRowId;

export function initSOPTemplateAssignmentDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onSOPTemplateAssign (refresh) {
  $q.dialog({
    component: assignSOPTemplate,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onSOPTemplateAssignmentEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: assignSOPTemplate,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
