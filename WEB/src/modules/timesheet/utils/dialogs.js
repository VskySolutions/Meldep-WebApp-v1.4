import { useQuasar } from "quasar";

import editTimesheet from "modules/timesheet/components/addEdit.vue";

let $q;
let activeRowId;

export function initTimesheetDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onTimesheetAdd (refreshTimesheetList) {
  $q.dialog({
    component: editTimesheet,
    componentProps: {}
  }).onOk(() => {
    refreshTimesheetList();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onTimesheetEdit (id, isMyTaskActivity, selectedTimesheetLineIds = [], refreshTimesheetList) {
  activeRowId.value = id;
  $q.dialog({
    component: editTimesheet,
    componentProps: { id, timesheetLineIds: selectedTimesheetLineIds, isMyTaskActivity: isMyTaskActivity }
  }).onOk(() => {
    if (selectedTimesheetLineIds?.length) {
      selectedTimesheetLineIds.splice(0);
      localStorage.removeItem("selectedTimesheetLineIds");
    }
    refreshTimesheetList();
  })
    .onCancel(() => { })
    .onDismiss(() => {
      activeRowId.value = null;
    });
}
