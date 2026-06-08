import { useQuasar } from "quasar";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";

let $q;
let activeRowId;

export function initProjectTaskActivityActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// -------------------------------------------------------------
// Delete Project Task Activity
// -------------------------------------------------------------

export const onSubmitProjectTaskActivityDelete = async (
  id,
  projectTaskActivityName,
  projectName,
  refreshProjectTaskActivityList
) => {
  activeRowId.value = id;
  zwConfirmDelete(
    { data: `${projectName}, ${projectTaskActivityName}` },
    async () => {
      try {
        await projectActivitiesService.deleteProjectActivity(id);
        notifySuccess({ message: "Project Task Activity is deleted successfully." });
        refreshProjectTaskActivityList();
      } catch (error) {
        sendError("Error deleting project task activity ", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

// -------------------------------------------------------------
// Update Project Task Activity Status
// -------------------------------------------------------------

export const onSubmitProjectTaskActivityStatus = async (row, refreshProjectTaskActivityList) => {
 const isCurrentlyActive = row.active === true;
  const payload = {
    activityIds: [row.id],
    active: !isCurrentlyActive
  };
  const isActive = !isCurrentlyActive;
  $q.dialog({
    title: "Confirmation",
    message: `Are you sure you want to ${isCurrentlyActive ? "deactivate" : "activate"} this activity?`,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(async () => {
    try {
      await projectActivitiesService.updateActivityActiveStatus(payload);
      notifySuccess({
        message: `Activity has been ${isActive ? "activated" : "deactivated"} successfully.`
      });
      row.active = isActive ? "Active" : "Inactive";
      refreshProjectTaskActivityList();
    } catch (error) {
      $q.notify({
        type: "negative",
        message: `Failed to ${isActive ? "activate" : "deactivate"} the activity.`
      });
    }
  });
};

// -------------------------------------------------------------
// Start Project Task Activity Timer
// -------------------------------------------------------------

export const onStartProjectTaskActivityTimer = async (
  activity,
  startNewTask
) => {
  activeRowId.value = activity.id;
  try {
    const timer = {
      taskId: activity.task.id,
      taskName: activity.task.name,
      activityId: activity.id,
      activityName: activity.name
    };
    startNewTask(timer);
  } catch (error) {
    sendError("Error starting task activity timer", error);
  } finally {
    activeRowId.value = null;
  }
};
