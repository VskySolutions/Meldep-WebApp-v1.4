import { useQuasar } from "quasar";
import { ref, nextTick } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import helpDeskService from "modules/helpdesk/helpDesk.service";

let $q;
let activeRowId;

export function initHelpDeskActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export const updatingRow = ref({
  status: null,
  assignedTo: null
});

// Update HelpDesk Status
export const onSubmitHelpDeskStatus = async (id, statusId, refreshHelpDeskList) => {
  try {
    return withRowLoader(
      "status",
      id,
      () => helpDeskService.updateHelpDeskStatus(id, statusId),
      "Ticket status is saved successfully.",
      "Failed to update ticket status.",
      () => refreshHelpDeskList()
    );
  } catch (error) {
    sendError("Error updating test case status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Update AssignedTo
export const onSubmitAssignedTo = async (id, assignedToId, refreshHelpDeskList, refreshAllUserListByRoleForDropdown) => {
  try {
    return withRowLoader(
      "assignedTo",
      id,
      () => helpDeskService.updateAssignedTo(id, assignedToId),
      "Assigned To updated successfully.",
      "Failed to update Assigned To.",
      async () => {
        refreshHelpDeskList();
        refreshAllUserListByRoleForDropdown();
      }
    );
  } catch (error) {
    sendError("Error updating test case status", error);
  } finally {
    activeRowId.value = id;
  }
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Ticket Attributes Dropdowns (Update)
// --------------------------------------------------------------------------------------------------------------------------------------------------
export const formLoading = ref({
  status: false,
  assignedTo: false,
  priority: false,
  category: false
});

const withFormLoader = async (
  field,
  apiCall,
  successMessage = "Updated successfully.",
  errorMessage = "Update failed.",
  afterSuccess = null
) => {
  formLoading.value[field] = true;
  await nextTick();
  document.activeElement?.blur();

  try {
    await apiCall();
    notifySuccess({ message: successMessage });

    if (afterSuccess) {
      await afterSuccess();
    }
  } catch (error) {
    notifyError({ message: errorMessage });
  } finally {
    formLoading.value[field] = false;
  }
};

// Update HelpDesk Status
export const onSubmitHelpDeskStatusFormLoader = async (id, statusId, refreshHelpDeskList, refreshGetHelpDesk) => {
  try {
    return withFormLoader(
      "status",
      () => helpDeskService.updateHelpDeskStatus(id, statusId),
      "Ticket status updated successfully.",
      "Failed to update status.",
      async () => {
        // getHelpDesk(helpDeskId.value);
        refreshGetHelpDesk();
        refreshHelpDeskList();
      }
    );
  } catch (error) {
    sendError("Error updating test case status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Update HelpDesk Priority
export const onSubmitHelpDeskPriorityFormLoader = async (id, priorityId, refreshHelpDeskList, refreshGetHelpDesk) => {
  try {
    return withFormLoader(
      "priority",
      () => helpDeskService.updateHelpDeskPriority(id, priorityId),
      "Priority updated successfully.",
      "Failed to update priority.",
      async () => {
        refreshGetHelpDesk();
        refreshHelpDeskList();
      }
    );
  } catch (error) {
    sendError("Error updating test case status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Update AssignedTo
export const onSubmitAssignedToFormLoader = async (id, assignedToId, refreshHelpDeskList, refreshAllUserListByRoleForDropdown, refreshGetHelpDesk) => {
  try {
    return withFormLoader(
      "assignedTo",
      () => helpDeskService.updateAssignedTo(id, assignedToId),
      "Assignment updated successfully.",
      "Failed to update assignment.",
      async () => {
        refreshHelpDeskList();
        refreshGetHelpDesk();
        refreshAllUserListByRoleForDropdown();
      }
    );
  } catch (error) {
    sendError("Error updating test case status", error);
  } finally {
    activeRowId.value = id;
  }
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
