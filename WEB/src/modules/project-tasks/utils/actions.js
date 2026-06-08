import { useQuasar } from "quasar";
import { notifySuccess, notifyError, notifyWarning, zwConfirmDelete, zwConfirm } from "assets/utils";

import projectTaskService from "modules/project-tasks/projectTasks.service";
import linkTaskToPlan from "modules/project-targetplan/components/_linkRequirementTaskIssueToWeeklyMonthlyPlan.vue";

let $q;
let activeRowId;

export function initProjectTaskActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// -------------------------------------------------------------
// Delete Project Task
// -------------------------------------------------------------

export const onSubmitProjectTaskDelete = async (
  projectTaskId,
  projectTaskName,
  projectName,
  refreshProjectTaskList
) => {
  activeRowId.value = projectTaskId;

  try {
    const resp = await projectTaskService.checkTaskCanBeDeleted(projectTaskId);
    const canDelete = resp?.canDelete;

    if (!canDelete) {
      zwConfirm(
        {
          title: "Active Activities Found",
          message: "This task has active activities. You cannot delete it.",
          data: `${projectTaskName}`,
          okLabel: "OK",
          cancel: false
        },
        () => refreshProjectTaskList()
      );
      return;
    }

    zwConfirmDelete({ data: `${projectName}, ${projectTaskName}` }, async () => {
      try {
        await projectTaskService.deleteProjectTask(projectTaskId);

        notifySuccess({ message: "Project Task is deleted successfully." });
        refreshProjectTaskList();
      } catch (error) {
        sendError("Error deleting project task", error);
      }
    });
  } catch (error) {
    sendError("Error checking task delete permission", error);
  } finally {
    activeRowId.value = null;
  }
};

// -------------------------------------------------------------
// Update Task Status
// -------------------------------------------------------------

export const onSubmitProjectTaskStatus = async (
  projectTaskStatusListWithDisables,
  taskIds,
  projectTaskStatusId,
  refreshProjectTaskList
) => {
  const ids = Array.isArray(taskIds) ? taskIds : [taskIds];

  activeRowId.value = ids.length === 1 ? ids[0] : null;

  const selected = projectTaskStatusListWithDisables.find(
    item => item.value === projectTaskStatusId
  );

  try {
    if (selected?.text?.toLowerCase() === "close") {
      const results = await Promise.all(
        ids.map(id => projectTaskService.checkTaskCanBeDeleted(id))
      );

      const allCanClose = results.every(resp => resp?.canDelete);

      if (!allCanClose) {
        zwConfirm(
          {
            title: "Active Activities Found",
            message:
              ids.length > 1
                ? "Some selected tasks have active activities and cannot be closed."
                : "This task has active activities. You cannot close it.",
            okLabel: "OK",
            cancel: false
          },
          () => refreshProjectTaskList?.()
        );
        return;
      }
    }

    const payload = {
      taskIds: ids,
      statusId: projectTaskStatusId
    };

    await projectTaskService.updateProjectTaskStatus(payload);

    notifySuccess({
      message:
        ids.length > 1
          ? "Status updated successfully."
          : "Task status is saved successfully."
    });

    refreshProjectTaskList?.();
  } catch (error) {
    sendError("Error updating project task status", error);
  }
};

// -------------------------------------------------------------
// Update Task Owner
// -------------------------------------------------------------

export const onSubmitProjectTaskOwner = async (
  projectTaskId,
  assignedToId,
  refreshProjectTaskList
) => {
  activeRowId.value = projectTaskId;
  try {
    await projectTaskService.updateTaskOwner(projectTaskId, assignedToId);

    notifySuccess({ message: "Task Owner is saved successfully." });
    refreshProjectTaskList();
  } catch (error) {
    sendError("Error updating task owner", error);
  }
};

// -------------------------------------------------------------
// Link Tasks To Plan
// -------------------------------------------------------------

export const onSubmitLinkProjectTasksToPlan = (
  rows,
  multiSelectProjectIds,
  multiSelectProjectName,
  multiSelectTaskIds,
  multiSelectTaskNames,
  refreshProjectTaskList,
  setDefaultsForMultiSelects
) => {
  const selectedTasks = rows.value.filter(task => multiSelectTaskIds.value.includes(task.id));

  // check if any selected task is not manage permission
  const hasNonEditable = selectedTasks.some(task => !task.isEditable);

  if (hasNonEditable) {
    notifyWarning({ message: "Some selected tasks have only view permission." });
    return;
  }

  const props = {
    projectId: multiSelectProjectIds.value[0],
    projectName: multiSelectProjectName.value[0],
    type: "Project Tasks",
    ids: multiSelectTaskIds.value,
    names: multiSelectTaskNames.value
  };

  $q.dialog({
    component: linkTaskToPlan,
    componentProps: props
  })
    .onOk(() => {
      setDefaultsForMultiSelects();
      refreshProjectTaskList();
    })
    .onDismiss(() => {
      activeRowId.value = null;
    });
};

// -------------------------------------------------------------
// Update Task Priority
// -------------------------------------------------------------

export const onSubmitProjectTaskPriority = async (
  taskIds,
  projectTaskPriorityId,
  refreshProjectTaskList
) => {
  const ids = Array.isArray(taskIds) ? taskIds : [taskIds];

  activeRowId.value = ids.length === 1 ? ids[0] : null;

  const payload = {
    taskIds: ids,
    priorityId: projectTaskPriorityId
  };

  try {
    await projectTaskService.updateProjectTaskPriority(payload);

    notifySuccess({
      message:
        ids.length > 1
          ? "Priority updated successfully."
          : "Project Task priority is saved successfully."
    });

    refreshProjectTaskList?.();
  } catch (error) {
    sendError("Error updating task priority", error);
  }
};

// -------------------------------------------------------------
// Update Task End Date
// -------------------------------------------------------------

export const onSubmitProjectTaskEndDate = async (
  projectTaskId,
  endDateStr,
  refreshProjectTaskList
) => {
  activeRowId.value = projectTaskId;
  try {
    await projectTaskService.updateProjectTaskEndDate(projectTaskId, endDateStr);

    notifySuccess({ message: "Project due date is saved successfully." });
    refreshProjectTaskList();
  } catch (error) {
    sendError("Error updating task end date", error);
  }
};

// -------------------------------------------------------------
// Update Task Tags
// -------------------------------------------------------------
export const onSubmitProjectTaskTags = async (
  taskIds,
  tagNames,
  refreshProjectTaskList,
  refreshProjectTagDropdown,
  flag = null
) => {
  const ids = Array.isArray(taskIds) ? taskIds : [taskIds];

  activeRowId.value = ids.length === 1 ? ids[0] : null;

  const model = {
    taskIds: ids,
    flag,
    tagsNameList: tagNames.map(tag => tag.text)
  };

  try {
    await projectTaskService.saveTags(model);

    notifySuccess({
      message:
        ids.length > 1
          ? "Tags updated successfully."
          : "Tag saved successfully."
    });

    refreshProjectTaskList?.();
    refreshProjectTagDropdown?.();
  } catch (error) {
    sendError("Error saving project task tags", error);
  }
};

// -------------------------------------------------------------
// Error Handler
// -------------------------------------------------------------

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
