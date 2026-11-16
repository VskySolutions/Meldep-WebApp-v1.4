import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";

import requirementService from "modules/requirement/requirement.service";

let activeRowId;

export function initRequirementActions (rowRef) {
  activeRowId = rowRef;
}
// -------------------------------------------------------------
// Update Requirement Pin-UnPin
// -------------------------------------------------------------
export const onSubmitRequirementPinned = async (id, status, refreshRequirementList) => {
  try {
    await requirementService.updateRequirementIsPinned(id, status);
    notifySuccess({ message: `Requirement is ${status ? "pinned" : "unpinned"}.` });
    refreshRequirementList();
  } catch (error) {
    sendError("Error updating requirement pin status", error);
  } finally {
    activeRowId.value = id;
  }
};

// -------------------------------------------------------------
// Update Requirement Color
// -------------------------------------------------------------
export const onSubmitRequirementColor = async (
  id,
  color,
  isSliderActive,
  previousColor,
  refreshRequirementList
) => {
  if (!isSliderActive || color === previousColor) return false;

  try {
    isSliderActive = true;
    const sanitizedColor = encodeURIComponent(color);
    await requirementService.updateRequirementColor( id, sanitizedColor);
    notifySuccess({ message: "Color updated successfully." });
    refreshRequirementList();
  } catch (error) {
    sendError("Error updating requirement color", error);
  } finally {
    isSliderActive = false;
    activeRowId.value = id;
  }
};

// -------------------------------------------------------------
// Update Requirement Tags
// -------------------------------------------------------------

export const onSubmitRequirementTags = async (
  requirementId,
  tagNames,
  refreshRequirementList,
  refreshRequirementTagDropdown
) => {
  activeRowId.value = requirementId;
  const model = {
    requirementIds: [requirementId],
    flag: null,
    tagsNameList: tagNames.map(tag => tag.text)
  };

  try {
    await requirementService.saveTags(model);

    notifySuccess({ message: "Tag saved successfully." });

    refreshRequirementList();
    refreshRequirementTagDropdown();
  } catch (error) {
    sendError("Error saving requirement tags", error);
  }
};

export const onSubmitRequirementStatus = async (
  id,
  statusId,
  refreshRequirementList,
  note = null
) => {
  try {
    const payload = {
      requirementIds: [id],
      statusId,
      statusNote: note
    };

    await requirementService.updateRequirementStatus(payload);

    notifySuccess({ message: "Status is saved successfully." });
    refreshRequirementList();
  } catch (error) {
    sendError("Error updating requirement status", error);
  } finally {
    activeRowId.value = id;
  }
};

// Update Requirement Status
// export const onSubmitRequirementStatus = async (id, statusId, refreshRequirementList) => {
//   try {
//     const payload = {
//       requirementIds: [id],
//       statusId
//     };
//     await requirementService.updateRequirementStatus(payload);

//     notifySuccess({ message: "Requirement status is saved successfully." });
//     refreshRequirementList();
//   } catch (error) {
//     sendError("Error updating requirement status", error);
//   } finally {
//     activeRowId.value = id;
//   }
// };

// Update Requirement Priority
export const onSubmitRequirementPriority = async (id, priorityId, refreshRequirementList) => {
  try {
    const payload = {
      requirementIds: [id],
      priorityId
    };
    await requirementService.updateRequirementPriority(payload);

    notifySuccess({ message: "Requirement priority is saved successfully." });
    refreshRequirementList();
  } catch (error) {
    sendError("Error updating requirement priority", error);
  } finally {
    activeRowId.value = id;
  }
};

// -------------------------------------------------------------
// Update Planned start date
// -------------------------------------------------------------

// export const onSubmitPlannedStartDate = async (
//   requirementId,
//   plannedStartDateStr,
//   note = null,
//   refreshRequirementList
// ) => {
//   activeRowId.value = requirementId;
//   try {
//     const payload = {
//       requirementId,
//       plannedStartDateStr,
//       plannedStartDateNote: note
//     };

//     await requirementService.updateRequirementDates(payload);

//     notifySuccess({ message: "Planned start date is saved successfully." });
//     refreshRequirementList();
//   } catch (error) {
//     sendError("Error updating planned start date", error);
//   }
// };

export const onSubmitRequirementDate = async (
  requirementId,
  dateStr,
  field,
  note = null,
  refreshRequirementList
) => {
  activeRowId.value = requirementId;

  try {
    const payload = {
      requirementId,
      plannedStartDateStr: null,
      plannedStartDateNote: null,
      plannedEndDateStr: null,
      plannedEndDateNote: null,
      actualStartDateStr: null,
      actualStartDateNote: null,
      actualEndDateStr: null,
      actualEndDateNote: null
    };

    switch (field) {
      case "plannedStartDate":
        payload.plannedStartDateStr = dateStr;
        payload.plannedStartDateNote = note;
        break;

      case "plannedEndDate":
        payload.plannedEndDateStr = dateStr;
        payload.plannedEndDateNote = note;
        break;

      case "actualStartDate":
        payload.actualStartDateStr = dateStr;
        payload.actualStartDateNote = note;
        break;

      case "actualEndDate":
        payload.actualEndDateStr = dateStr;
        payload.actualEndDateNote = note;
        break;

      default:
        throw new Error("Invalid requirement date field.");
    }

    await requirementService.updateRequirementDates(payload);

    notifySuccess({
      message: `${getDateFieldLabel(field)} is saved successfully.`
    });

    refreshRequirementList();

  } catch (error) {
    sendError(`Error updating ${getDateFieldLabel(field)}`, error);
  }
};

const getDateFieldLabel = (field) => {
  const labels = {
    plannedStartDate: "Planned start date",
    plannedEndDate: "Planned end date",
    actualStartDate: "Actual start date",
    actualEndDate: "Actual end date"
  };

  return labels[field] || "Requirement date";
};

// -------------------------------------------------------------
// Update Planned End date
// -------------------------------------------------------------

// export const onSubmitPlannedEndDate = async (
//   requirementId,
//   plannedEndDateStr,
//   note = null,
//   refreshRequirementList
// ) => {
//   activeRowId.value = requirementId;
//   try {
//     await requirementService.updateRequirementDates(payload);

//     notifySuccess({ message: "Planned end date is saved successfully." });
//     refreshRequirementList();
//   } catch (error) {
//     sendError("Error updating planned end date", error);
//   }
// };

// -------------------------------------------------------------
// Actual start date
// -------------------------------------------------------------

// export const onSubmitActualStartDate = async (
//   requirementId,
//   actualStartDateStr,
//   note = null,
//   refreshRequirementList
// ) => {
//   activeRowId.value = requirementId;
//   try {
//     await requirementService.updateRequirementDates(payload);

//     notifySuccess({ message: "Actual start date is saved successfully." });
//     refreshRequirementList();
//   } catch (error) {
//     sendError("Error updating actual start date", error);
//   }
// };

// -------------------------------------------------------------
// Update Planned End date
// -------------------------------------------------------------

// export const onSubmitActualEndDate = async (
//   requirementId,
//   actualEndDateStr,
//   note = null,
//   refreshRequirementList
// ) => {
//   activeRowId.value = requirementId;
//   try {
//     await requirementService.updateRequirementDates(payload);

//     notifySuccess({ message: "Actual end date is saved successfully." });
//     refreshRequirementList();
//   } catch (error) {
//     sendError("Error updating actual end date", error);
//   }
// };

// Delete Requirement
export const onSubmitRequirementDelete = async (
  id,
  requirementName,
  refreshRequirementList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${requirementName}` },
    async () => {
      try {
        await requirementService.deleteRequirement(id);
        notifySuccess({ message: "Requirement is deleted successfully." });
        refreshRequirementList();
      } catch (error) {
        sendError("Error deleting requirement", error);
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
