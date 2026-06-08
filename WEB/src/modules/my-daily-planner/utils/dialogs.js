import { useQuasar } from "quasar";

import editDailyPlanner from "modules/my-daily-planner/components/addEdit.vue";

let $q;
let activeRowId;

export function initDailyPlannerDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onDailyPlannerAdd (refresh) {
  $q.dialog({
    component: editDailyPlanner,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onDailyPlannerEdit (id, isForwarded, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editDailyPlanner,
    componentProps: { id, isForwarded }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => {
      activeRowId.value = null;
    });
}
