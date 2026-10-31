import { notifySuccess, zwConfirmDelete } from "assets/utils";

import requirementStatusTrackingService from "../requirementStatusTracking.service";
import { distinctRecordIds } from "./trackingList.js";

// Delete one complete tracking record (all its requirement rows). Cancel leaves everything as is.
export function onSubmitRequirementStatusTrackingDelete (record, onDeleted) {
  zwConfirmDelete(
    {
      title: "Delete Tracking Record",
      message: "This deletes the tracking record and all its requirement rows. Requirements and Data Mappings are not affected.",
      data: `${record.projectName} / ${record.projectModuleName} / ${record.trackingDate}`
    },
    async () => {
      try {
        await requirementStatusTrackingService.deleteRequirementStatusTracking(record.recordId);
        notifySuccess({ message: "Tracking record is deleted successfully." });
        onDeleted?.(true);
      } catch (error) {
        // The http interceptor shows the server error; refresh so the list matches what is persisted.
        onDeleted?.(false);
      }
    }
  );
}

// Delete the selected complete records in a single request (the API deletes all of them or none).
export function onSubmitRequirementStatusTrackingBulkDelete (recordIds, onDeleted) {
  const ids = distinctRecordIds(recordIds);
  if (!ids.length) return;

  zwConfirmDelete(
    {
      title: "Bulk Delete Tracking Records",
      message: `This deletes ${ids.length} selected tracking record(s) and all their requirement rows. Requirements and Data Mappings are not affected.`,
      data: `${ids.length} record(s)`
    },
    async () => {
      try {
        await requirementStatusTrackingService.bulkDeleteRequirementStatusTracking(ids);
        notifySuccess({ message: `${ids.length} tracking record(s) deleted successfully.` });
        onDeleted?.(true);
      } catch (error) {
        onDeleted?.(false);
      }
    }
  );
}
