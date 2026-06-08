import { useQuasar } from "quasar";
import viewCandidate from "modules/candidate/components/view.vue";
import editCandidate from "modules/candidate/components/addEdit.vue";
import addCandidateActivity from "modules/candidate/components/addCandidateActivity.vue";
import addCandidateFeedback from "modules/candidate/components/addCandidateFeedback.vue";

let $q;
let activeRowId;

export function initCandidateDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onCandidateView (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewCandidate,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onCandidateAdd (refresh) {
  $q.dialog({
    component: editCandidate,
    componentProps: { }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onCandidateEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editCandidate,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onCandidateAddActivity (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addCandidateActivity,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onCandidateAddFeedback (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addCandidateFeedback,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
