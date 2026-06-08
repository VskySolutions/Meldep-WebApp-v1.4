import { useQuasar } from "quasar";

import viewProjectTask from "modules/project-tasks/components/view.vue";
import editProjectTask from "modules/project-tasks/components/addEdit.vue";
import addNote from "modules/common/components/addNote.vue";

import viewTaskLevelTimeSheet from "modules/project-tasks/components/_taskLevelTimesheets.vue";
import copyProjectTask from "modules/project-tasks/components/_copyTaskToProject.vue";
import addViewFiles from "src/modules/project-tasks/components/_uploadFilesToTask.vue";
// import siteStatusLog from "modules/sites/components/_siteModifiedLogs.vue";
import taskStatusLog from "modules/project-tasks/components/_taskStatusLog.vue";
import AddBulkTask from "modules/project-tasks/components/addBulk.vue";
import EditBulkTask from "modules/project-tasks/components/editBulk.vue";

let $q;
let activeRowId;

export function initProjectTaskDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onProjectTaskView (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewProjectTask,
    componentProps: { id }
  }).onOk(refresh)
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectTaskAdd (
  refresh,
  refreshProjectTaskNameDropdown,
  projectId,
  moduleId,
  startDate,
  endDate
) {
  const componentProps = {};

  if (projectId) componentProps.projectIdAttr = projectId;
  if (moduleId) componentProps.moduleIdAttr = moduleId;
  if (startDate) componentProps.startDate = startDate;
  if (endDate) componentProps.endDate = endDate;

  $q.dialog({
    component: editProjectTask,
    componentProps
  })
    .onOk(() => {
      refresh && refresh();
      refreshProjectTaskNameDropdown && refreshProjectTaskNameDropdown();
    });
}

export function onProjectTaskAddBulk (
  projectId,
  moduleId,
  startDate,
  endDate,
  refresh,
  refreshProjectTaskNameDropdown
) {
  const componentProps = {};

  if (projectId) componentProps.projectIdAttr = projectId;
  if (moduleId) componentProps.moduleIdAttr = moduleId;
  if (startDate) componentProps.startDate = startDate;
  if (endDate) componentProps.endDate = endDate;

  $q.dialog({
    component: AddBulkTask,
    componentProps
  })
    .onOk(() => {
      refresh && refresh();
      refreshProjectTaskNameDropdown && refreshProjectTaskNameDropdown();
    });
}

export function onProjectTaskEditBulk (
  projectId,
  moduleId,
  refresh,
  refreshProjectTaskNameDropdown
) {
  const componentProps = {};

  if (projectId) componentProps.projectIdAttr = projectId;
  if (moduleId) componentProps.moduleIdAttr = moduleId;

  $q.dialog({
    component: EditBulkTask,
    componentProps
  })
    .onOk(() => {
      refresh && refresh();
      refreshProjectTaskNameDropdown && refreshProjectTaskNameDropdown();
    });
}

export function onProjectTaskEdit (id, refresh) {
  $q.dialog({
    component: editProjectTask,
    componentProps: { id }
  }).onOk(refresh)
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectTaskLevelTimeSheetView (id, flag = "") {
  activeRowId.value = id;
  $q.dialog({
    component: viewTaskLevelTimeSheet,
    componentProps: { id, flag }
  }).onOk()
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectTaskFiles (id, name, projectName, moduleName) {
  activeRowId.value = id;
  $q.dialog({
    component: addViewFiles,
    componentProps: { id, name, projectName, moduleName }
  }).onOk()
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectTaskCopy (id, name, projectModuleId, isCopy, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: copyProjectTask,
    componentProps: { id, name, projectModuleId, isCopy }
  })
    .onOk(refresh)
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectTaskMove (id, name, projectModuleId, isMove, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: copyProjectTask,
    componentProps: { id, name, projectModuleId, isMove }
  })
    .onOk(refresh)
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

// export function onProjectTaskStatusChangeLog (id, name, columnName, refresh) {
//   activeRowId.value = id;
//   $q.dialog({
//     component: siteStatusLog,
//     componentProps: { id, name, columnName }
//   }).onOk(refresh)
//     .onCancel(() => { activeRowId.value = id; })
//     .onDismiss(() => { activeRowId.value = id; });
// }

export function onProjectTaskNotesAdd (id, type, moduleId, module, name) {
  activeRowId.value = id;
  $q.dialog({
    component: addNote,
    componentProps: { id, type, moduleId, module, name }
  }).onOk(() => {
  })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectTaskStatusChangeLog (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: taskStatusLog,
    componentProps: { id }
  }).onOk(refresh)
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}
