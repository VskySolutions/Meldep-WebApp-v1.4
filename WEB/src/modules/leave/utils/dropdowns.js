import { useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function leaveModule () {
  const leaveStatusDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const leaveCategoryDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    leaveStatusDropdown,
    leaveCategoryDropdown
  };
}
