import { useQuasar } from "quasar";
import addEditNotes from "modules/common/components/addNote.vue";
import viewNoteTimeLineView from "src/components/_note_timeline_view.vue";

let $q;
let activeRowId;

export function initCommonDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onNoteAdd (id, type, moduleId, module, name, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditNotes,
    componentProps: { id, type, moduleId, module, name }
  }).onOk(() => {
    refresh?.();
  })
    .onCancel(() => { activeRowId.value = id; refresh?.(); })
    .onDismiss(() => { activeRowId.value = id; refresh?.(); });
}

export function onNoteTimelineView(id, notesType) {
  activeRowId.value = id;
  $q.dialog({
    component: viewNoteTimeLineView,
    componentProps: { id, notesType}
  })
    .onOk(() => {
    })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}
