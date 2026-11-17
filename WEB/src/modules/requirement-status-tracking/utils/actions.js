import { notifySuccess, zwConfirmDelete } from "assets/utils";

import requirementStatusTrackingService from "../requirementStatusTracking.service";
import { distinctRecordIds, trackingIdsForRequirement } from "./trackingList.js";

// Delete one complete tracking record (all its requirement rows). Cancel leaves everything as is.
export function onSubmitRequirementStatusTrackingDelete (record, onDeleted) {
  zwConfirmDelete(
    {
      // `label` lets the grouped list name the requirement + tracking date of the row being deleted.
      data: record.label || `${record.projectName} / ${record.projectModuleName} / ${record.trackingDate}`
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

// Delete every tracking record of one Requirement ID, not only the rows visible under the current filters. The ids are fetched fresh by requirement id and removed in a single request
// (the API deletes all of them or none). Cancel leaves everything as is.
export async function onSubmitRequirementStatusTrackingDeleteForRequirement (group, onDeleted) {
  let ids = [];
  try {
    const resp = await requirementStatusTrackingService.getRequirementStatusTrackingByRequirementId(group.requirementId);
    ids = trackingIdsForRequirement(resp?.requirementStatusTrackingRecordsList, group);
  } catch (error) {
    onDeleted?.(false);
    return;
  }
  if (!ids.length) {
    onDeleted?.(false);
    return;
  }

  const requirement = `${group.requirementNumber ? `#${group.requirementNumber} - ` : ""}${group.requirementTitle}`;
  zwConfirmDelete(
    {
      title: "Delete Requirement Tracking Records",
      message: `This deletes all ${ids.length} tracking record(s) of this requirement. The requirement itself is not affected.`,
      data: requirement
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
