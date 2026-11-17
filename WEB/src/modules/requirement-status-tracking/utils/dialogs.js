import { useQuasar } from "quasar";
import addEditRequirementStatusTracking from "modules/requirement-status-tracking/components/addEdit.vue";

let $q;

export function initRequirementStatusTrackingDialogs () {
  $q = useQuasar();
}

// refresh runs only after a successful batch save (Save and Continue or Save and Close), never on Close.
export function onAddRequirementStatusTracking (refresh) {
  $q.dialog({
    component: addEditRequirementStatusTracking,
    componentProps: { onSaved: refresh }
  });
}

export function onEditRequirementStatusTracking (projectId, projectModuleId, requirementId, refresh) {
  $q.dialog({
    component: addEditRequirementStatusTracking,
    componentProps: { projectId, projectModuleId, requirementId, onSaved: refresh }
  }).onOk(() => {
      refresh();
    })
    .onCancel(() => { refresh(); })
    .onDismiss(() => { refresh(); });
}
