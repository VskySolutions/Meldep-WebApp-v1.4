import { useQuasar } from "quasar";
import addEditEmployee from "modules/employee/components/addEdit.vue";
import viewEmployee from "modules/employee/components/view.vue";

let $q;
let activeRowId;

export function initEmployeeDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onEmployeeView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewEmployee,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onEmployeeAdd (refresh) {
  $q.dialog({
    component: addEditEmployee,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onEmployeeEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditEmployee,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
