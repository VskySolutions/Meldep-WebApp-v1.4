import { useQuasar } from "quasar";
import addEditIssue from "modules/issue/components/addEdit.vue";
import viewIssue from "modules/issue/components/view.vue";
import issueStatusLog from "modules/issue/components/_issueStatusLog.vue";
import addIssueActivites from "modules/issue/components/addActivity.vue";

let $q;
let activeRowId;

export function initIssueDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onIssueView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewIssue,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onIssueStatusLog (id) {
  activeRowId.value = id;
  $q.dialog({
    component: issueStatusLog,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onIssueAdd (refresh) {
  $q.dialog({
    component: addEditIssue,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onIssueEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addEditIssue,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onIssueAddActivity (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addIssueActivites,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
