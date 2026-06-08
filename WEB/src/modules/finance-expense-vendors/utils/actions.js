import { useQuasar } from "quasar";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import financeExpenseVendorService from "modules/finance-expense-vendors/financeExpenseVendors.service";

let $q;
let activeRowId;

export function initVendorActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// -------------------------------------------------------------
// Delete Vendor
// -------------------------------------------------------------

export const onSubmitVendorDelete = async (
  id,
  vendorName,
  refreshVendorList
) => {
  activeRowId.value = id;
  zwConfirmDelete(
    { data: `${vendorName}` },
    async () => {
      try {
        await financeExpenseVendorService.deleteVendor(id);
        notifySuccess({ message: "Vendor is deleted successfully." });
        refreshVendorList();
      } catch (error) {
        sendError("Error deleting vendor ", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

