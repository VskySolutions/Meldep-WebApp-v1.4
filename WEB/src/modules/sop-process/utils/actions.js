import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete, zwConfirm } from "assets/utils";
import sopProcessService from "modules/sop-process/sopProcess.service";

let $q;
let activeRowId;

export function initSOPProcessActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete SOPProcess
export const onSubmitSOPProcessDelete = async (
  id,
  sopProcessName,
  refreshSOPProcessList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${sopProcessName}` },
    async () => {
      try {
        await sopProcessService.deleteSOPProcess(id);
        notifySuccess({ message: "SOP process is deleted successfully." });
        refreshSOPProcessList();
      } catch (error) {
        sendError("Error deleting SOP process", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

export const onSubmitSOPProcessPublished = async (
  id,
  sopProcessName,
  refreshSOPProcessList,
  sopProcessStatusDropdownSingleSelect
) => {
  activeRowId.value = id;

  zwConfirm(
    {
      title: "Publish SOP Process",
      message: `Are you sure you want to publish "${sopProcessName}"?`,
      cancelLabel: "No",
      okLabel: "Yes"
    },
    async () => {
      try {
        // Get Published Status Id
        const statusId =
          await sopProcessStatusDropdownSingleSelect.getValueByLabel("Published");

        // Update SOP Status
        await sopProcessService.updateSOPProcessStatus(id, statusId);

        notifySuccess({
          message: "SOP process published successfully."
        });

        refreshSOPProcessList();

      } catch (error) {
        sendError("Error publishing SOP process", error);

      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

export const onSubmitSOPProcessStatus = async (id, statusId, refreshSOPProcessList) => {
  try {
    await sopProcessService.updateSOPProcessStatus(id, statusId);

    notifySuccess({ message: "Process status is saved successfully." });
    refreshSOPProcessList();
  } catch (error) {
    sendError("Error updating Process status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Update SOP Process Active / Inactive
export const onSubmitSOPProcessActiveInActiveToggle = (
  id,
  active,
  refreshSOPProcessList
) => {
  activeRowId.value = id;

  const isCurrentlyActive = active === true;
  const newStatus = !isCurrentlyActive;

  zwConfirm(
    {
      title: "Confirmation",
      message: `Are you sure you want to ${
        isCurrentlyActive ? "deactivate" : "activate"
      } this SOP process?`,
      cancelLabel: "No",
      okLabel: "Yes"
    },
    async () => {
      try {
        await sopProcessService.updateSOPProcessActiveStatus(
          id,
          newStatus
        );

        notifySuccess({
          message: `SOP process has been ${
            newStatus ? "activated" : "deactivated"
          } successfully.`
        });

        refreshSOPProcessList();
      } catch (error) {
        sendError(
          `Failed to ${
            newStatus ? "activate" : "deactivate"
          } the SOP process.`,
          error
        );
      } finally {
        activeRowId.value = null;
      }
    },

    // NO / CANCEL
    () => {
      activeRowId.value = null;
    }
  );
};

// SOP Process Status Rules
export const getVisibleStatusOptionsByRole = (
  row,
  role,
  loggedUserId,
  statusList
) => {
  const currentStatus = row.statusText?.toLowerCase();

  if (!currentStatus) return [];

  // Always include current status first
  const filterStatuses = (allowedStatuses) => {
    const normalizedAllowed = [
      currentStatus,
      ...allowedStatuses.filter(status => status !== currentStatus)
    ];

    return statusList.filter(
      x => normalizedAllowed.includes(x.text.toLowerCase())
    );
  };

  // Both Roles (Editor + Approver)
  if (role === "both") {
    const isCreator = loggedUserId === row.createdBy.id;

    // Both Role(Editor + Approver) and creator
    if (isCreator) {
      switch (currentStatus) {
      case "draft":
        return filterStatuses(["submitted", "archived"]);

      case "submitted":
        return filterStatuses(["approved"]);

      case "approved":
        return filterStatuses(["published", "archived"]);

      case "published":
        return filterStatuses(["archived"]);

      case "archived":
        return filterStatuses(["draft"]);

      default:
        return [];
      }
    }

    // Both but not editor
    switch (currentStatus) {
      case "submitted":
        return filterStatuses(["approved"]);

      case "approved":
        return filterStatuses(["archived"]);

      case "published":
        return filterStatuses(["archived"]);

      default:
        return [];
    }
  }

  // Editor Only
  if (role === "editor") {
    if (loggedUserId !== row.createdBy.id) return [];

    switch (currentStatus) {
    case "draft":
      return filterStatuses(["submitted", "archived"]);

    case "approved":
      return filterStatuses(["archived", "published"]);

    case "published":
      return filterStatuses(["archived"]);

    case "archived":
      return filterStatuses(["draft"]);

    default:
      return [];
    }
  }

  // Approver Only
  if (role === "approver") {
    switch (currentStatus) {
    case "submitted":
      return filterStatuses(["approved"]);

    case "approved":
      return filterStatuses(["archived"]);

    case "published":
      return filterStatuses(["archived"]);

    default:
      return [];
    }
  }

  return [];
};

// Whether editable
export const isSOPProcessStatusEditable = (
  row,
  role,
  loggedUserId,
  statusList
) => {
  return getVisibleStatusOptionsByRole(
    row,
    role,
    loggedUserId,
    statusList
  ).length > 0;
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
