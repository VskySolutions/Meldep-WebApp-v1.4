import { useQuasar } from "quasar";

import viewCustomers from "modules/customer/components/view.vue";
import editCustomer from "modules/customer/components/addEdit.vue";

let $q;
let activeRowId;

export function initCustomerDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onCustomerView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewCustomers,
    componentProps: { id }
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
}

export function onCustomerAdd (refreshCustomerNameDropdown, refresh) {
  $q.dialog({
    component: editCustomer,
    componentProps: { }
  }).onOk(() => {
    refreshCustomerNameDropdown();
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onCustomerEdit (id, customerTypeId, personId, companyId, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editCustomer,
    componentProps: { id, customerTypeId, personId, companyId }
  }).onOk(() => {
    refresh();
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
}
