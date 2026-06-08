import { useQuasar } from "quasar";
import viewProjectModule from "modules/project-modules/components/view.vue";
import addProjectModuleFiles from "src/modules/project-modules/components/_uploadFilesToModule.vue";
import editProjectModule from "modules/project-modules/components/addEdit.vue";
import copyModuleToProjects from "modules/project-modules/components/_copyModuleToProject.vue";
import moveModuleAsProject from "modules/project-modules/components/_moveModuleAsProject.vue";

let $q;
let activeRowId;

export function initProjectModuleDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onProjectModuleView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewProjectModule,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
}

// View Project module Files
export function onProjectFilesAdd (id, name, projectName) {
  activeRowId.value = id;
  $q.dialog({
    component: addProjectModuleFiles,
    componentProps: { id, name, projectName }
  }).onOk()
    .onCancel()
    .onDismiss(() => { activeRowId.value = id; });
}

export function onProjectModuleAdd (
  projectId,
  projectName,
  refresh,
  refreshProjectModuleNameDropdown
) {
  const componentProps = {
    projectIdAttr: projectId ?? null,
    projectName: projectName ?? null
  };

  $q.dialog({
    component: editProjectModule,
    componentProps
  })
    .onOk(() => {
      refresh && refresh();
      refreshProjectModuleNameDropdown && refreshProjectModuleNameDropdown();
    });
}

export function onProjectModuleEdit (id, refresh, refreshProjectModuleNameDropdown) {
  activeRowId.value = id;
  $q.dialog({
    component: editProjectModule,
    componentProps: { id }
  }).onOk(() => {
    refresh && refresh();
    refreshProjectModuleNameDropdown && refreshProjectModuleNameDropdown();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onProjectModuleCopy (id, name, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: copyModuleToProjects,
    componentProps: { id, name }
  }).onOk(() => {
    refresh && refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onProjectModuleMoveAsProject (id, name, projectId, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: moveModuleAsProject,
    componentProps: { id, name, projectId }
  }).onOk(() => {
    refresh && refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
