import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import siteShareService from "modules/sites-sharing/sitesSharing.service";

let $q;
let activeRowId;

export function initSiteShareActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete Site Share
export const onSubmitSiteShareDelete = async (
  id,
  personName,
  refreshSiteShareList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${personName}` },
    async () => {
      try {
        await siteShareService.deleteSiteShare(id);
        notifySuccess({ message: "User is deleted successfully." });
        refreshSiteShareList();
      } catch (error) {
        sendError("Error deleting user", error);
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
