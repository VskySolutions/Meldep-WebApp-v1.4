import { useQuasar } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import dailyplannerService from "modules/my-daily-planner/myDailyPlanner.service";

let $q;
let activeRowId;

export function initDailyPlannerActions (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

// Delete DailyPlanner
export const onSubmitDailyPlannerDelete = async (
  id,
  dailyPlannerDate,
  refreshDailyPlannerList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${dailyPlannerDate}` },
    async () => {
      try {
        await dailyplannerService.deleteDailyplanner(id);
        notifySuccess({ message: "Planner is deleted successfully." });
        refreshDailyPlannerList();
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
