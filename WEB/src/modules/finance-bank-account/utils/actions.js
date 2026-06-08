import { useQuasar } from "quasar";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import financeBankAccountService from "modules/finance-bank-account/financeBankAccount.service";

let $q;
let activeRowId;

export function initBankAccountActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// -------------------------------------------------------------
// Delete Bank Account
// -------------------------------------------------------------

export const onSubmitBankAccountDelete = async (
  id,
  accountNumber,
  refreshBankAccountList
) => {
  activeRowId.value = id;
  zwConfirmDelete(
    { data: `${accountNumber}` },
    async () => {
      try {
        await financeBankAccountService.deleteBankAccount(id);
        notifySuccess({ message: "Bank account is deleted successfully." });
        refreshBankAccountList();
      } catch (error) {
        sendError("Error deleting Bank account ", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

// -------------------------------------------------------------
// Update Bank Account Status
// -------------------------------------------------------------

export const onSubmitBankAccountStatus = async (row, refreshBankAccountList) => {
 activeRowId.value = row.id;
  const isCurrentlyActive = row.activeStatus === "Active";
  const newStatus = !isCurrentlyActive;
  $q.dialog({
    title: "Confirmation",
    message: `Are you sure you want to ${isCurrentlyActive ? "deactivate" : "activate"} this account?`,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(() => {
    financeBankAccountService.deactivateBankAccount(row.id, newStatus)
      .then(() => {
        notifySuccess({
          message: `Bank account has been ${newStatus ? "activated" : "deactivated"} successfully.`
        });
        row.activeStatus = newStatus ? "Active" : "Inactive";
        refreshBankAccountList();
      })
      .finally(() => {
        activeRowId.value = null;
      });
    })
    .onCancel(() => {
      activeRowId.value = null;
  });
};
