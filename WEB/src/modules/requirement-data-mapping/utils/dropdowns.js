import { useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function requirementModule () {
  const dataMappingIntegrationDirectionDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    dataMappingIntegrationDirectionDropdownSingleSelect
  };
}
