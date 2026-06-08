import { useQuasar } from "quasar";
import viewCompanyContact from "modules/company-contacts/components/view.vue";
import editCompanyContact from "modules/company-contacts/components/addEdit.vue";

let $q;
let activeRowId;

export function initCompanyContactDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onCompanyContactView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewCompanyContact,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onCompanyContactAdd (refresh) {
  $q.dialog({
    component: editCompanyContact,
    componentProps: { }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onCompanyContactEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editCompanyContact,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}
