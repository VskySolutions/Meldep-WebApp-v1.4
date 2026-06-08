import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import companyService from "modules/company/company.service";

let $q;
let activeRowId;

export function initCompanyActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Update Company Status
export const onSubmitCompanyStatus = async (id, statusId, refreshCompanyList) => {
  try {
    await companyService.updateCompanyStatus(id, statusId);

    notifySuccess({ message: "Company status is saved successfully." });
    refreshCompanyList();
  } catch (error) {
    sendError("Error updating company status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Delete Company
export const onSubmitCompanyDelete = async (
  id,
  companyName,
  refreshCompanyList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${companyName}` },
    async () => {
      try {
        await companyService.deleteCompany(id);
        notifySuccess({ message: "Company is deleted successfully." });
        refreshCompanyList();
      } catch (error) {
        sendError("Error deleting company", error);
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
