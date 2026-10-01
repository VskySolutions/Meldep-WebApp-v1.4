import { useQuasar } from "quasar";
import editDataMappingNotes from "modules/requirement-data-mapping/components/_notes_timeline_view.vue";
import addEditDataMapping from "modules/requirement-data-mapping/components/addEdit.vue";

let $q;
let activeRowId;

export function initRequirementDataMappingDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onAddRequirementDataMapping (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditDataMapping,
    componentProps: { id }
  }).onOk(() => {
      refresh();
    })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onRequirementDataMappingNoteEdit (id) {
  // activeRowId.value = id;
  $q.dialog({
    component: editDataMappingNotes,
    componentProps: { id }
  }).onOk(() => {
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}