import { useQuasar } from "quasar";
import { notifySuccess } from "assets/utils";
import viewProject from "modules/project/components/view.vue";
import viewProjectFiles from "modules/project/components/_uploadFilesToProject.vue";

import editProject from "modules/project/components/addEdit.vue";

import assignUserToProject from "modules/project/components/_assignUsersToProject.vue";
import sendMessage from "modules/project/components/_projectChat.vue";
import convertTemplateToProject from "modules/project/components/_convertTemplateToProject.vue";
import convertProjectToTemplate from "modules/project/components/_convertProjectToTemplate.vue";
import addNote from "modules/common/components/addNote.vue";
import assignBulk from "modules/project/components/_assignMultipleUsersToMultipleProjects.vue";
import assignUserToProjectModule from "modules/project-modules/components/_assignUsersToModule.vue";

let $q;
let activeRowId;

export function initProjectDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onProjectView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewProject,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectAdd (refresh, refreshProjectNameDropdown) {
  $q.dialog({
    component: editProject,
    componentProps: {}
  }).onOk(() => {
    refresh?.();
    refreshProjectNameDropdown?.();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onProjectEdit (id, isCharter = false, refresh, refreshProjectNameDropdown) {
  activeRowId.value = id;
  $q.dialog({
    component: editProject,
    componentProps: { id, isCharter }
  }).onOk(() => {
    refresh();
    refreshProjectNameDropdown();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

// Assign Users To Project
export function onAssignUserToProject (id, projectName, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: assignUserToProject,
    componentProps: { id, projectName }
  }).onOk(refresh)
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

// Assign bulk Users To Project
export function onAssignBulkUserToProject (refresh) {
  $q.dialog({
    component: assignBulk,
    componentProps: { }
  }).onOk(() => {
    refresh?.();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

// Assign Users To Project Module
export function onAssignBulkUsersToProjectModule (id, projectName, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: assignUserToProjectModule,
    componentProps: { id, projectName }
  }).onOk(refresh)
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onAddNotesToProject (id, type, moduleId, module, name, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: addNote,
    componentProps: { id, type, moduleId, module, name }
  }).onOk(refresh).onCancel(() => {
  }).onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

// View Project Messages
export function onProjectMessage (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: sendMessage,
    componentProps: { id }
  }).onOk(refresh)
    .onCancel(refresh)
    .onDismiss(() => { activeRowId.value = id; });
}

// View Project Files
export function onProjectFilesView (id, name, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewProjectFiles,
    componentProps: { id, name }
  }).onOk(refresh)
    .onCancel(refresh)
    .onDismiss(() => { activeRowId.value = id; });
}

export function convertProjectTemplate (projectId, projectName, startDate, refresh) {
  $q.dialog({
    component: convertProjectToTemplate,
    componentProps: { projectId, projectName, startDate }
  }).onOk(() => {
    notifySuccess({ message: "Project Template Generated." });
    refresh();
  });
}

export function convertTemplateProject (projectId, templateName, refresh) {
  $q.dialog({
    component: convertTemplateToProject,
    componentProps: { projectId, templateName }
  }).onOk(() => {
    notifySuccess({ message: "New Project Created." });
    refresh();
  });
}
