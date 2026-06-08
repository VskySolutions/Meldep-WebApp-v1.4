import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete, zwConfirm } from "assets/utils";
import leadsService from "modules/lead/lead.service";

let $q;
let activeRowId;

export function initLeadActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete Lead
export const onSubmitLeadDelete = async (
  id,
  leadName,
  refreshLeadList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${leadName}` },
    async () => {
      try {
        await leadsService.deleteLead(id);
        notifySuccess({ message: "Lead is deleted successfully." });
        refreshLeadList();
      } catch (error) {
        sendError("Error deleting lead", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

// Delete Lead Activity
export const onSubmitLeadActivityDelete = async (
  id,
  leadActivityName,
  refreshLeadList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${leadActivityName}` },
    async () => {
      try {
        await leadsService.deleteLeadActivity(id);
        notifySuccess({ message: "Lead activity is deleted successfully." });
        refreshLeadList();
      } catch (error) {
        sendError("Error deleting lead activity", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

// Lead Group
export const onSubmitLeadGroupStatus = (
  id,
  active,
  refreshLeadGroupAssignedUsersList
) => {
  activeRowId.value = id;

  const isCurrentlyActive = active === true;
  const newStatus = !isCurrentlyActive;

  zwConfirm(
    {
      title: "Confirmation",
      message: `Are you sure you want to ${
        isCurrentlyActive ? "deactivate" : "activate"
      } this lead group user?`,
      cancelLabel: "No",
      okLabel: "Yes"
    },
    async () => {
      try {
        await leadsService.updateLeadGroupsUserStatus(id);

        notifySuccess({
          message: `Lead group user has been ${
            newStatus ? "activated" : "deactivated"
          } successfully.`
        });

        refreshLeadGroupAssignedUsersList();
      } catch (error) {
        sendError(
          `Failed to ${
            newStatus ? "activate" : "deactivate"
          } the lead group user.`,
          error
        );
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

export const onSubmitLeadGroupUserDelete = async (
  id,
  userName,
  leadGroupName,
  refreshLeadGroupAssignedUsersList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${userName}, ${leadGroupName}` },
    async () => {
      try {
        await leadsService.deleteLeadGroupsUser(id);
        notifySuccess({ message: "Lead group User is deleted successfully." });
        refreshLeadGroupAssignedUsersList();
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

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
