import { useQuasar } from "quasar";
import addLeaveCredit from "modules/leave/components/addLeaveCredit.vue";
import editLeaveCredit from "modules/leave/components/editLeaveCredit.vue";
import applyLeave from "modules/leave/components/_applyLeave.vue";
import viewLeave from "modules/leave/components/view.vue";
import forwardLeave from "modules/leave/components/_forwardLeave.vue";
import approveLeave from "modules/leave/components/_approveOrDeclineLeave.vue";

let $q;
let activeRowId;

export function initLeaveDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onLeaveView (id, type, refresh) {
  activeRowId.value = id;
  let componentProps = {};

  if (type === "applyLeave") {
    componentProps = { id, leaveId: id };
  } else if (type === "approve") {
    componentProps = { id, approveleaveId: id };
  } else if (type === "forward") {
    componentProps = { id, forwardleaveid: id };
  }
  $q.dialog({
    component: viewLeave,
    componentProps: componentProps
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onLeaveCreditAdd (refresh) {
  $q.dialog({
    component: addLeaveCredit,
    componentProps: { }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onLeaveCreditEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editLeaveCredit,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onApplyLeaveAdd (refresh) {
  $q.dialog({
    component: applyLeave,
    componentProps: { }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onLeaveForward (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: forwardLeave,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onLeaveApprove (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: approveLeave,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
