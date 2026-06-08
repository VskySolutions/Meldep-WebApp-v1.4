import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirm, zwConfirmDelete } from "assets/utils";
import projectModuleService from "modules/project-modules/projectModules.service";

let $q;
let activeRowId;

export function initProjectModuleActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete Project
export const onSubmitProjectModuleUserDelete = async (
  id,
  projectModuleName,
  userName,
  refreshProjectModuleUserList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${projectModuleName}, ${userName}` },
    async () => {
      try {
        await projectModuleService.deleteProjectModuleUser(id);
        notifySuccess({ message: "Project Module user is deleted successfully." });
        refreshProjectModuleUserList();
      } catch (error) {
        sendError("Error deleting project", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

export const onSubmitProjectModuleDelete = async (
  moduleId,
  moduleName,
  projectName,
  refreshProjectModuleList
) => {
  activeRowId.value = moduleId;

  try {
    const resp = await projectModuleService.checkModuleCanBeDeleted(moduleId);
    const canDelete = resp?.canDelete;

    if (!canDelete) {
      zwConfirm(
        {
          title: "Active Tasks or Activities Found",
          message: "This module has active tasks or activities. You cannot delete it.",
          data: `${moduleName}`,
          okLabel: "OK",
          cancel: false
        },
        () => refreshProjectModuleList()
      );
      return;
    }

    zwConfirmDelete(
      { data: `${moduleName}, ${projectName}` },
      async () => {
        try {
          await projectModuleService.deleteProjectModule(moduleId);

          notifySuccess({
            message: "Project module deleted successfully."
          });

          refreshProjectModuleList();
        } catch (error) {
          sendError("Error deleting project module", error);
        }
      }
    );
  } catch (error) {
    sendError("Error checking module delete permission", error);
  } finally {
    activeRowId.value = null;
  }
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
