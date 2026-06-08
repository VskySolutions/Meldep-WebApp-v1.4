import { useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function leaveModule () {
  const employeeDesignationsDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    employeeDesignationsDropdown,
  };
}
