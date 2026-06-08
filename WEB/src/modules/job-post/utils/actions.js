import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import jobPostService from "modules/job-post/jobPost.service";

let $q;
let activeRowId;

export function initJobPostActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Update Job Post Status
export const onSubmitJobPostStatus = async (id, statusId, refreshJobPostList) => {
  try {
    await jobPostService.updateJobStatus(id, statusId);

    notifySuccess({ message: "Job post status is saved successfully." });
    refreshJobPostList();
  } catch (error) {
    sendError("Error updating job post status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Delete Job Post
export const onSubmitJobPostDelete = async (
  id,
  jobTitle,
  refreshJobPostList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${jobTitle}` },
    async () => {
      try {
        await jobPostService.deleteJobPost(id);
        notifySuccess({ message: "Job post is deleted successfully." });
        refreshJobPostList();
      } catch (error) {
        sendError("Error deleting job post", error);
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
