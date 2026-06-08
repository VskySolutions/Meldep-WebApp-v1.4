import { useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function releaseTrackingModule () {
  // Status (Single Select)
  const releaseTrackingStatusDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const releaseTrackingTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    releaseTrackingStatusDropdownSingleSelect,
    releaseTrackingTypeDropdownSingleSelect
  };
}
