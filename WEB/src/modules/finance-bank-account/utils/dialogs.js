import { useQuasar } from "quasar";
import addEditBankAccount from "modules/finance-bank-account/components/addEdit.vue";
import viewBankAccount from "modules/finance-bank-account/components/view.vue";

let $q;
let activeRowId;

export function initBankAccountDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onBankAccountView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewBankAccount,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onBankAccountAdd (refresh) {
  $q.dialog({
     component: addEditBankAccount,
     componentProps: {}
   }).onOk(() => {
     refresh();
   })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onBankAccountEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditBankAccount,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
