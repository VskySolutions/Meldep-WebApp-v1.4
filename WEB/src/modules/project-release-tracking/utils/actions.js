import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import { ref, nextTick } from "vue";
import releaseTrackingService from "modules/project-release-tracking/projectReleaseTracking.service";

let activeRowId;

export function initReleaseTrackingActions (rowRef) {
  activeRowId = rowRef;
}

export const updatingRow = ref({
  status: null
});

export const onSubmitReleaseTrackingStatus = async (
  id,
  statusId,
  refreshReleaseTrackingList
) => {
  try {
    await withRowLoader(
      "status",
      id,
      () => releaseTrackingService.updateReleaseTrackingStatus(id, statusId),
      "Release tracking status is saved successfully.",
      "Failed to update release tracking status.",
      refreshReleaseTrackingList
    );
  } catch (error) {
    sendError("Error updating release tracking status", error);
  }
};

// Delete ReleaseTracking
export const onSubmitReleaseTrackingDelete = async (
  id,
  ReleaseTrackingName,
  refreshReleaseTrackingList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${ReleaseTrackingName}` },
    async () => {
      try {
        await releaseTrackingService.deleteReleaseTracking(id);
        notifySuccess({ message: "Release tracking is deleted successfully." });
        refreshReleaseTrackingList();
      } catch (error) {
        sendError("Error deleting ReleaseTracking", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

export const onSubmitRetestingItemDelete = (
  rowId,
  name,
  markDeleted
) => {
  activeRowId.value = rowId;

  zwConfirmDelete(
    { data: name },
    () => {
      if (typeof markDeleted === "function") {
        markDeleted();
      }

      notifySuccess({
        message: "Test case marked for deletion."
      });

      activeRowId.value = null;
    },
    () => {
      activeRowId.value = null;
    }
  );
};

const withRowLoader = async (
  field,
  rowId,
  apiCall,
  successMessage = "Updated successfully.",
  errorMessage = "Update failed.",
  afterSuccess = null
) => {
  updatingRow.value[field] = rowId;

  await nextTick();
  document.activeElement?.blur();

  try {
    await apiCall();

    if (afterSuccess) {
      await afterSuccess();
    }

    notifySuccess({ message: successMessage });
  } catch (error) {
    notifyError({ message: errorMessage });
  } finally {
    updatingRow.value[field] = null;
  }
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
