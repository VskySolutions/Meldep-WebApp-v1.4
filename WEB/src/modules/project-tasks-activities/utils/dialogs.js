import { useQuasar } from "quasar";
import taskAssignment from "modules/project-tasks-activities/components/addEditBulk.vue";
import viewProjectTaskActivity from "modules/project-tasks-activities/components/view.vue";
import EditProjectTaskActivity from "modules/project-tasks-activities/components/addEdit.vue";

let $q;
let activeRowId;

export function initProjectTaskActivityDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onProjectTaskActivityView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewProjectTaskActivity,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectTaskAssignment (taskId, projectId, moduleId, taskName, projectName, moduleName, pageHeading, refresh) {
  activeRowId.value = taskId;
  $q.dialog({
    component: taskAssignment,
    componentProps: {
      projectIdAttr: projectId,
      moduleIdAttr: moduleId,
      taskIdAttr: taskId,
      projectName,
      moduleName,
      taskName,
      pageHeading
    }
  }).onOk(() => {
    refresh();
  }).onCancel(() => {
    activeRowId.value = null;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
}

export function onProjectTaskActivityAdd (projectId, moduleId, taskId, taskName, projectName, moduleName, refresh) {
  $q.dialog({
    component: EditProjectTaskActivity,
    componentProps: { projectIdAttr: projectId, moduleIdAttr: moduleId, taskIdAttr: taskId, projectName, moduleName, taskName }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onProjectTaskActivityEdit (id, projectId, moduleId, taskName, projectName, moduleName, isMyTaskActicity, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: EditProjectTaskActivity,
    componentProps: { id, projectId, moduleId, taskName, projectName, moduleName, isMyTaskActicity }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
