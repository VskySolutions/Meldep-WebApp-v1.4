import { useQuasar } from "quasar";
import viewExpense from "modules/finance-expense/components/view.vue";
import addEditExpense from "modules/finance-expense/components/addEdit.vue";

let $q;
let activeRowId;

export function initExpenseDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onExpenseView (id, isShowActions, refresh ) {
  activeRowId.value = id;
  $q.dialog({
    component: viewExpense,
    componentProps: { id, isShowActions }
  }) .onOk(() => {
      if (isShowActions) {
        refresh();
      }
      activeRowId.value = id;
    })
    .onCancel(() => {
      activeRowId.value = id;
    })
    .onDismiss(() => {
      activeRowId.value = id;
    });
}

export function onExpenseAdd (refresh) {
  $q.dialog({
     component: addEditExpense,
     componentProps: {}
   }).onOk(() => {
     refresh();
   })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onExpenseEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditExpense,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
