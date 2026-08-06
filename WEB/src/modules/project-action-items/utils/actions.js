import { useQuasar } from "quasar";
import { ref } from "vue";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import projectActionItemsService from "modules/project-action-items/projectActionItems.service";

let $q;
let activeRowId;

export function initProjectActionItemsActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export const updatingRow = ref({
  status: null
});

// Delete ProjectActionItems
export const onSubmitProjectActionItemsDelete = async (
  id,
  title,
  refreshProjectActionItemsList
) => {
  activeRowId.value = id;
  debugger;

  zwConfirmDelete(
    { data: `${title}` },
    async () => {
      try {
        await projectActionItemsService.deleteProjectActionItem(id);
        notifySuccess({ message: "Project action item is deleted successfully." });
        refreshProjectActionItemsList();
      } catch (error) {
        sendError("Error deleting Project action item", error);
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
