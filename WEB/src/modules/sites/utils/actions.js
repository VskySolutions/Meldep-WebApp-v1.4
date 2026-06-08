import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import moduleService from "modules/module/module.service";

let $q;
let activeRowId;

export function initSiteActions(rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export const onRemoveModuleMenuRoleAccess = async (
  siteId,
  moduleMenuId,
  roleId,
  menuName,
  roleName,
  refreshList
) => {
  activeRowId.value = moduleMenuId;

  zwConfirmDelete(
    { data: `${menuName}, ${roleName}` },
    async () => {
      try {
        await moduleService.deleteModuleMenuRoleAccess(
          siteId,
          moduleMenuId,
          roleId
        );

        notifySuccess({
          message: "Role access removed successfully."
        });

        refreshList();
      } catch (error) {
        sendError("Error removing role access.", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

function sendError(message, error) {
  notifyError({ message });
  console.error(message, error);
}
