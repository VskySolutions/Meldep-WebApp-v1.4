import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";

import requirementDataMappingService from "../requirementDataMapping.service";

let activeRowId;

export function initRequirementDataMappingActions (rowRef) {
  activeRowId = rowRef;
}

// Delete Requirement Data Mapping
export const onSubmitBulkDataMappingDelete = async (
  id,
  requirementId,
  source,
  refreshRequirementList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${source}` },
    async () => {
      try {
        await requirementDataMappingService.deleteBulkDataMapping(requirementId);
        notifySuccess({ message: "Data mapping is deleted successfully." });
        refreshRequirementList();
      } catch (error) {
        sendError("Error deleting data mapping", error);
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
// Error Handler
// -------------------------------------------------------------

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
