import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import timesheetService from "modules/timesheet/timesheet.service";

let $q;
let activeRowId;

export function initTimesheetActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete Timesheet
export const onSubmitTimesheetDelete = async (
  id,
  timesheetDate,
  refreshTimesheetList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${timesheetDate}` },
    async () => {
      try {
        await timesheetService.deleteTimesheet(id);
        notifySuccess({ message: "Timesheet is deleted successfully." });
        refreshTimesheetList();
      } catch (error) {
        sendError("Error deleting Test Case", error);
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
