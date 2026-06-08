import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function sopAssignmentModule () {
  const sopAssignmentPriorityDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const sopAssignmentPrioritiesDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const sopAssignmentStatusesDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    sopAssignmentPriorityDropdownSingleSelect,
    sopAssignmentPrioritiesDropdown,
    sopAssignmentStatusesDropdown
  };
}
