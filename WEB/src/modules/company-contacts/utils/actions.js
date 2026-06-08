import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import companyContactService from "modules/company-contacts/companyContacts.service";

let $q;
let activeRowId;

export function initCompanyContactActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete Company Contact
export const onSubmitCompanyContactDelete = async (
  id,
  CompanyName,
  refreshCompanyContactList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${CompanyName}` },
    async () => {
      try {
        await companyContactService.deleteCompanyContact(id);
        notifySuccess({ message: "Company contact is deleted successfully." });
        refreshCompanyContactList();
      } catch (error) {
        sendError("Error deleting company contact", error);
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
