import { useQuasar } from "quasar";
import viewInfraProjectInstance from "modules/infra-project-instance/components/view.vue";
import addUserInProjectInstance from "modules/infra-project-instance/components/addUser.vue";

let $q;
let activeRowId;

export function initInfraProjectInstanceDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onInfraProjectInstanceView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewInfraProjectInstance,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id;
  });
}

export function onInfraProjectInstanceUserAdd (id, refresh) {
  $q.dialog({
    component: addUserInProjectInstance,
    componentProps: { id }
  }).onOk(() => {
    refresh?.();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
