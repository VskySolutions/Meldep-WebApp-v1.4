import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import projectService from "modules/project/projects.service";

let $q;
let activeRowId;

export function initProjectActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Update Project EndDate
export const onSubmitProjectEndDate = async (id, goLiveDate, refreshProjectList) => {
  try {
    activeRowId.value = id;
    await projectService.updateProjectEndDate(id, goLiveDate);
    notifySuccess({ message: "Project due date is saved successfully." });
    refreshProjectList();
  } catch (error) {
    sendError("Error updating project due date", error);
  } finally {
    activeRowId.value = null;
  }
};

// Update Project Pin-UnPin
export const onSubmitProjectPinned = async (id, status, refreshProjectList) => {
  try {
    activeRowId.value = id;
    await projectService.updateProjectIsPinned(id, status);
    notifySuccess({ message: `Project is ${status ? "pinned" : "unpinned"}.` });
    refreshProjectList();
  } catch (error) {
    sendError("Error updating project pin status", error);
  } finally {
    activeRowId.value = null;
  }
};

// Update Project Color
export const onSubmitProjectColor = async (
  id,
  projectColor,
  isSliderActive,
  previousColor,
  refreshProjectList
) => {
  if (!isSliderActive || projectColor === previousColor) return false;
  const payload = { id, projectColor };

  try {
    isSliderActive = true;
    activeRowId.value = id;

    await projectService.updateProjectColor(id, payload);
    notifySuccess({ message: "Color updated successfully." });
    refreshProjectList();
  } catch (error) {
    sendError("Error updating project color", error);
  } finally {
    isSliderActive = false;
    activeRowId.value = null;
  }
};

// Update Project Active / Inactive
export const onSubmitProjectActiveInActiveToggle = (id, active, refreshProjectList) => {
  const isCurrentlyActive = active === true;
  const newStatus = !isCurrentlyActive;

  const payload = {
    id,
    activeStatus: newStatus ? "Active" : "Inactive"
  };

  $q.dialog({
    title: "Confirmation",
    message: `Are you sure you want to ${isCurrentlyActive ? "deactivate" : "activate"} this project?`,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(async () => {
    try {
      activeRowId.value = id;
      await projectService.updateProjectColor(id, payload);
      notifySuccess({ message: `Project has been ${newStatus ? "activated" : "deactivated"} successfully.` });
      active = newStatus ? "Active" : "Inactive";
      refreshProjectList();
    } catch (error) {
      const msg = `Failed to ${newStatus ? "activate" : "deactivate"} the project.`;
      sendError(msg, error);
    } finally {
      activeRowId.value = null;
    }
  });
};

// Update Project Status
export const onSubmitProjectStatus = async (id, statusId, refreshProjectList) => {
  try {
    activeRowId.value = id;
    await projectService.updateProjectStatus(id, statusId);

    notifySuccess({ message: "Project status is saved successfully." });
    refreshProjectList();
  } catch (error) {
    sendError("Error updating project status", error);
  } finally {
    activeRowId.value = null;
  }
};

// Update Project Priority
export const onSubmitProjectPriority = async (id, statusId, refreshProjectList) => {
  try {
    activeRowId.value = id;
    await projectService.updateProjectPriority(id, statusId);
    notifySuccess({ message: "Project priority is saved successfully." });
    refreshProjectList();
  } catch (error) {
    sendError("Error updating project priority", error);
  } finally {
    activeRowId.value = null;
  }
};

// Delete Project
export const onSubmitProjectDelete = async (
  id,
  projectName,
  refreshProjectList,
  refreshProjectNameDropdown
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${projectName}` },
    async () => {
      try {
        await projectService.deleteProject(id);
        notifySuccess({ message: "Project is deleted successfully." });
        refreshProjectList();
        refreshProjectNameDropdown();
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

// Update Project Tags
export const onSubmitProjectTags = async (
  projectId,
  tagNames,
  refreshProjectList,
  refreshProjectTagsDropdown
) => {
  const model = {
    projectId,
    flag: null,
    tagsNameList: tagNames.map(tag => tag.text),
    color: ""
  };

  try {
    activeRowId.value = projectId;
    await projectService.saveTags(model);
    notifySuccess({ message: "Tag saved successfully." });
    refreshProjectList();
    refreshProjectTagsDropdown();
  } catch (error) {
    sendError("Error saving project tags", error);
  } finally {
    activeRowId.value = null;
  }
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
