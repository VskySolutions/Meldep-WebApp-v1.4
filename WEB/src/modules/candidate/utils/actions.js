import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import candidateService from "modules/candidate/candidate.service";

let $q;
let activeRowId;

export function initCandidateActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Update Candidate Status
export const onSubmitCandidateStatus = async (id, statusId, refreshCandidateList) => {
  try {
    await candidateService.updateCandidateStatus(id, statusId);

    notifySuccess({ message: "Candidate status is saved successfully." });
    refreshCandidateList();
  } catch (error) {
    sendError("Error updating candidate status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Delete Candidate
export const onSubmitCandidateDelete = async (
  id,
  candidateName,
  refreshCandidateList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${candidateName}` },
    async () => {
      try {
        await candidateService.deleteCandidate(id);
        notifySuccess({ message: "Candidate is deleted successfully." });
        refreshCandidateList();
      } catch (error) {
        sendError("Error deleting candidate", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

// Delete Candidate Activity
export const onSubmitCandidateActivityDelete = async (
  id,
  activityName,
  refreshCandidateActivityList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${activityName}` },
    async () => {
      try {
        await candidateService.deleteCandidateActivityLogs(id);
        notifySuccess({ message: "Candidate activity is deleted successfully." });
        refreshCandidateActivityList();
      } catch (error) {
        sendError("Error deleting candidate activity", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

// Delete Candidate Feedback
export const onSubmitCandidateFeedbackDelete = async (
  id,
  candidateQuestion,
  refreshCandidateFeedbackList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${candidateQuestion}` },
    async () => {
      try {
        await candidateService.deleteCandidateFeedbacks(id);
        notifySuccess({ message: "Candidate feedback is deleted successfully." });
        refreshCandidateFeedbackList();
      } catch (error) {
        sendError("Error deleting candidate feedback", error);
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
