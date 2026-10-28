import { useQuasar } from "quasar";
import editDataMappingNotes from "modules/requirement-data-mapping/components/_notes_timeline_view.vue";
import editDataMapping from "modules/requirement-data-mapping/components/addEdit.vue";
import addEditDataMapping from "modules/requirement-data-mapping/components/addEdit.vue";

let $q;
// let activeRowId;

export function initRequirementDataMappingDialogs () {
  $q = useQuasar();
  // activeRowId = rowRef;
}
export function onAddRequirementDataMapping (
  projectId,
  projectModuleId,
  requirementId,
  isRequirementReadonly,
  refresh
) {
  const componentProps = { projectId, projectModuleId, id: requirementId, isRequirementReadonly };
  $q.dialog({
    component: addEditDataMapping,
    componentProps
  })
    .onOk(() => {
      refresh();
    })
    .onCancel(() => { refresh(); })
    .onDismiss(() => { refresh(); });
}

export function onEditRequirementDataMapping (id, isRequirementReadonly, refresh) {
  // activeRowId.value = id;
  $q.dialog({
    component: editDataMapping,
    componentProps: { id, isRequirementReadonly }
  }).onOk(() => {
      // activeRowId.value = id;
      refresh();
    })
    .onCancel(() => { refresh(); })
    .onDismiss(() => { refresh(); });
}

export function onRequirementDataMappingNoteEdit (id, source, target, isManageNotes, refreshDataMappingList) {
  // activeRowId.value = id;
  $q.dialog({
    component: editDataMappingNotes,
    componentProps: { id, source, target, isManageNotes }
  }).onOk(() => {
    refreshDataMappingList();
  })
    .onCancel(() => { refreshDataMappingList(); })
    .onDismiss(() => { refreshDataMappingList(); });
}
