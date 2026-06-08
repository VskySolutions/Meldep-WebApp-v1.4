import { useQuasar } from "quasar";
import addEditRequirementGroup from "modules/requirement-group/components/addEdit.vue";
import viewRequirementGroup from "modules/requirement-group/components/view.vue";

let $q;
let activeRowId;

export function initRequirementGroupDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onRequirementGroupView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewRequirementGroup,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onRequirementGroupAdd (refresh) {
  $q.dialog({
    component: addEditRequirementGroup,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onRequirementGroupEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditRequirementGroup,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
