import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import issuesService from "modules/issue/issue.service";

let $q;
let activeRowId;

export function initIssueActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Update Issue Status
export const onSubmitIssueStatus = async (id, statusId, refreshIssueList) => {
  try {
    const payload = {
      issueIds: [id],
      statusId
    };
    await issuesService.updateIssueStatus(payload);

    notifySuccess({ message: "Issue status is saved successfully." });
    refreshIssueList();
  } catch (error) {
    sendError("Error updating issue status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Delete Issue
export const onSubmitIssueDelete = async (
  id,
  issueName,
  refreshIssueList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${issueName}` },
    async () => {
      try {
        await issuesService.deleteIssue(id);
        notifySuccess({ message: "Issue is deleted successfully." });
        refreshIssueList();
      } catch (error) {
        sendError("Error deleting issue", error);
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
