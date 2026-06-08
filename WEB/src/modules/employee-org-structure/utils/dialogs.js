import { useQuasar } from "quasar";
import viewEmployeeOrgStructure from "modules/employee-org-structure/components/view.vue";
import addEditEmployeeOrgStructure from "modules/employee-org-structure/components/addEdit.vue";

let $q;
let activeRowId;

export function initEmployeeOrgStructureDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onEmployeeOrgStructureView (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewEmployeeOrgStructure,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

export function onEmployeeOrgStructureAdd (refresh) {
  $q.dialog({
    component: addEditEmployeeOrgStructure,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
};

export function onEmployeeOrgStructureEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditEmployeeOrgStructure,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
};
