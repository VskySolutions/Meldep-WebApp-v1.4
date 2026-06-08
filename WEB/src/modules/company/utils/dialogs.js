import { useQuasar } from "quasar";
import viewCompany from "modules/company/components/view.vue";
import editCompany from "modules/company/components/addEdit.vue";

let $q;
let activeRowId;

export function initCompanyDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onCompanyView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewCompany,
    componentProps: { id }
  }).onOk(() => { })
    .onCancel(() => { })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onCompanyAdd (refreshCompanyNameDropdown, refresh) {
  $q.dialog({
    component: editCompany,
    componentProps: { }
  }).onOk(() => {
    refreshCompanyNameDropdown();
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onCompanyEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editCompany,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
