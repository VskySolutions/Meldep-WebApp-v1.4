import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import sopTemplateService from "modules/sop-template/sopTemplate.service";

let $q;
let activeRowId;

export function initSOPTemplateActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete SOPTemplate
export const onSubmitSOPTemplateDelete = async (
  id,
  sopTemplateName,
  refreshSOPTemplateList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${sopTemplateName}` },
    async () => {
      try {
        await sopTemplateService.deleteSOPTemplate(id);
        notifySuccess({ message: "SOP template is deleted successfully." });
        refreshSOPTemplateList();
      } catch (error) {
        sendError("Error deleting SOP template", error);
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
