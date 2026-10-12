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
) => {
  try {
    const payload = {
      requirementIds: [id],
      statusId
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
