import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import releaseTrackingService from "modules/project-release-tracking/projectReleaseTracking.service";

let activeRowId;

export function initReleaseTrackingActions (rowRef) {
  activeRowId = rowRef;
}

// Update ReleaseTracking Status
export const onSubmitReleaseTrackingStatus = async (id, statusId, refreshReleaseTrackingList) => {
  try {
    await releaseTrackingService.updateReleaseTrackingStatus(id, statusId);

    notifySuccess({ message: "Release tracking status is saved successfully." });
    refreshReleaseTrackingList();
  } catch (error) {
    sendError("Error updating release tracking status", error);
  } finally {
    activeRowId.value = id;
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

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
