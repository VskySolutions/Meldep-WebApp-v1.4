import { useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function testCaseModule () {
  const timesheetStatusForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    timesheetStatusForDropdown
  };
}
