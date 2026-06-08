import { notifySuccess, notifyError, zwConfirm, zwConfirmDelete } from "assets/utils";
import infraAccountsServicesService from "modules/infra-account-services/infraAccountServices.service";

let activeRowId;

export function initInfraAccountServiceActions (rowRef) {
  activeRowId = rowRef;
}

// Delete InfraAccountService
export const onSubmitInfraAccountServiceDelete = async (
  item,
  refreshInfraAccountServiceList
) => {
  activeRowId.value = item.id;

  try {
    const resp = await infraAccountsServicesService.checkAccountServiceCanBeDeleted(item.id);
    const canDelete = resp?.canDelete;

    if (canDelete) {
      zwConfirmDelete(
        { data: `${item.itemType?.dropDownValue ?? ""}, ${item.name ?? ""}` },
        async () => {
          try {
            await infraAccountsServicesService.deleteInfraAccountServices(item.id);

            notifySuccess({ message: "Infra Account Service is deleted successfully." });

             refreshInfraAccountServiceList();
          } catch (error) {
            sendError("Error deleting Infra Account Service", error);
          } finally {
            activeRowId.value = null;
          }
        },
        () => {
          activeRowId.value = null;
        }
      );
    } else {
      zwConfirm(
        {
          title: "Service In Use",
          message: "This service is currently used in projects and cannot be deleted.",
          data: `${item.itemType?.dropDownValue ?? ""}, ${item.name ?? ""}`,
          okLabel: "OK",
          cancel: false
        },
        () => {
          activeRowId.value = null;
        }
      );
    }
  } catch (error) {
    sendError("Error checking Infra Account Service delete eligibility", error);
    activeRowId.value = null;
  }
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
