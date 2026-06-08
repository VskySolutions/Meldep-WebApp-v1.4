import { useQuasar } from "quasar";
import viewDepartment from "modules/department/components/view.vue";
import editDepartment from "modules/department/components/addEdit.vue";

let $q;
let activeRowId;

export function initDepartmentDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onDepartmentView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewDepartment,
    componentProps: { id }
  }).onOk(() => { })
    .onCancel(() => { })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onDepartmentAdd (refreshCompanyNameDropdown, refresh) {
  $q.dialog({
    component: editDepartment,
    componentProps: { }
  }).onOk(() => {
    refreshCompanyNameDropdown();
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onDepartmentEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editDepartment,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
