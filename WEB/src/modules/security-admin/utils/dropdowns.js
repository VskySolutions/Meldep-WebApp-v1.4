import { useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function securityAdminModule () {
  const eventTypeForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "dropDownText"
  });

  return {
    eventTypeForDropdown
  };
}
