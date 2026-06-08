import { useQuasar } from "quasar";
import addEditSOPTemplate from "modules/sop-template/components/addEdit.vue";
import viewSOPTemplate from "modules/sop-template/components/view.vue";

let $q;
let activeRowId;

export function initSOPTemplateDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onSOPTemplateView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewSOPTemplate,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onSOPTemplateAdd (refresh) {
  $q.dialog({
    component: addEditSOPTemplate,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onSOPTemplateEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditSOPTemplate,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
