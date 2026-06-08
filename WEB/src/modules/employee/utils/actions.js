import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import employeesService from "modules/employee/employee.service";

let $q;
let activeRowId;

export function initEmployeeActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete Employee
export const onSubmitEmployeeDelete = async (
  id,
  employeeName,
  refreshEmployeeList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${employeeName}` },
    async () => {
      try {
        await employeesService.deleteEmployee(id);
        notifySuccess({ message: "Employee is deleted successfully." });
        refreshEmployeeList();
      } catch (error) {
        sendError("Error deleting employee", error);
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
