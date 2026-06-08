import { useQuasar } from "quasar";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import ApproverNoteDialog from "../components/approveNote.vue";
import financeExpenseService from "modules/finance-expense/financeExpense.service";

let $q;
let activeRowId;

export function initExpenseActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// -------------------------------------------------------------
// Delete Expense
// -------------------------------------------------------------

export const onSubmitExpenseDelete = async (
  id,
  payee,
  refreshExpenseList
) => {
  activeRowId.value = id;
  zwConfirmDelete(
    { data: `${payee}` },
    async () => {
      try {
        await financeExpenseService.deleteExpense(id);
        notifySuccess({ message: "Expense is deleted successfully." });
        refreshExpenseList();
      } catch (error) {
        sendError("Error deleting expense ", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

export const onForwardToApprover = async (
  row,
  approver,
  setLoading,
  refreshExpenseList
) => {
  activeRowId.value = row.id;
  row.approver = approver;
  $q.dialog({
    title: "<span class=\"text-primary\">Confirm</span>",
    message:
      approver === "Request For Cancellation"
        ? "Are you sure you want to send request for cancellation ?"
        : approver === "Submitted"
        ? "Are you sure you want to submit expense?"
        : "",
    html: true,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  })
    .onOk(async () => {
      if (typeof setLoading === 'function') {
      setLoading(true);
      }
      try {
        await financeExpenseService.forwardExpenseToApprovers(row);
        notifySuccess({
          message:
            approver === "Request For Cancellation"
              ? "Request Sent Successfully!"
              : approver === "Submitted"
              ? "Submitted Successfully!"
              : ""
        });
        refreshExpenseList();
      } catch (error) {
        $q.notify({
          type: "negative",
          message: "Failed to update expense."
        });
      } finally {
        setLoading(false);
        activeRowId.value = null;
      }
    })
    .onCancel(() => {
      activeRowId.value = null;
    });
};

export const onCancelExpenseRequest = (
  row,
  approver,
  refreshApproveExpenseList
) => {
  activeRowId.value = row.id;
  row.approver = approver;
  if (approver === "Cancelled") {
    $q.dialog({
      component: ApproverNoteDialog,
      componentProps: {
        title: "<span class=\"text-primary\">Confirm</span>",
        message: "Are you sure you want to cancel ?",
        approver
      }
    })
      .onOk((note) => {
        row.preApproverNote = note;
        financeExpenseService.forwardExpenseToApprovers(row)
          .then(() => {
            notifySuccess({
              message: "Request Cancelled Successfully!"
            });
            refreshApproveExpenseList();
          })
          .catch(() => {
            $q.notify({
              type: "negative",
              message: "Failed to update expense."
            });
          });
      })
      .onCancel(() => {
        activeRowId.value = null;
      });
  }
};
