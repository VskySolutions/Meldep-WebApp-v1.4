import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import customerService from "modules/customer/customer.service";

let $q;
let activeRowId;

export function initCustomerActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Update Customer Advocate
export const onSubmitCustomerAdvocate = async (id, assignedToId, refreshCustomerList) => {
  try {
    await customerService.updateCustomerAdvocate(id, assignedToId);

    notifySuccess({ message: "Customer advocate is saved successfully." });
    refreshCustomerList();
  } catch (error) {
    sendError("Error updating customer advocate", error);
  } finally {
    activeRowId.value = id;
  }
};

// Delete Customer
export const onSubmitCustomerDelete = async (
  id,
  CustomerName,
  refreshCustomerList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${CustomerName}` },
    async () => {
      try {
        await customerService.deleteCustomer(id);
        notifySuccess({ message: "Customer is deleted successfully." });
        refreshCustomerList();
      } catch (error) {
        sendError("Error deleting customer", error);
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
