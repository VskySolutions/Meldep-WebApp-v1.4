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
        sendError("Error deleting Timesheet", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

export const onSubmitTimesheetApproval = async (
 selectedWeek,
 action,
  refreshWeeklyTimesheetApprovalList,
  closeDialog
) => {
  try {
  const model = {
      timesheetIds: [
        ...new Set(
          selectedWeek.timesheetLines.map(line => line.timesheetId)
        )
      ],
      employeeId: selectedWeek.employee.id,
      timesheetLines: selectedWeek.timesheetLines.map(line => ({
        id: line.timesheetLineId,
        isApproved: line.isApproved
      })),
      projectNames: [
        ...new Set(
          selectedWeek.timesheetLines.map(line => line.project)
        )
      ],
      TimesheetDate: selectedWeek.weekEndDate,
      approvalStatus: action
    };
    await timesheetService.approveDeclineTimesheet(model);

    notifySuccess({
      message: `Timesheet ${action.toLowerCase()} successfully.`
    });
    closeDialog();
    refreshWeeklyTimesheetApprovalList();
  } catch (error) {
    sendError(`Error ${action.toLowerCase()} timesheet.`, error);
  }
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
