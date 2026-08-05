import { useQuasar } from "quasar";
import addEditAdvanceExpense from "modules/finance-expense-advance/components/addEdit.vue";
import viewAdvanceExpense from "modules/finance-expense-advance/components/view.vue";
import viewAdvanceExpenseFiles from "modules/finance-expense-advance/components/_uploadFilesToAdvanceExpense.vue";

let $q;
let activeRowId;

export function initAdvanceExpenseDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onAdvanceExpenseView (id, isShowActions, refresh ) {
  activeRowId.value = id;
  $q.dialog({
    component: viewAdvanceExpense,
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

export function onAdvanceExpenseAdd (refresh) {
  $q.dialog({
     component: addEditAdvanceExpense,
     componentProps: {}
   }).onOk(() => {
     refresh();
   })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onAdvanceExpenseEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditAdvanceExpense,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

// View Advance expense Files
export function onAdvanceExpenseFilesView (id, itemCategoryType, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewAdvanceExpenseFiles,
    componentProps: { id, itemCategoryType }
  }).onOk(refresh)
    .onCancel(refresh)
    .onDismiss(() => { activeRowId.value = id; });
}
