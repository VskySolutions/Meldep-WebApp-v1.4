import { useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";

import commonService from "services/common.service";

export default function commonModule () {
  const countryNameDropdownSingleSelect = useSingleSelectDropdown(commonService.getCountries, {
    labelKey: "name",
    valueKey: "id"
  });

  const stateNameDropdownSingleSelect = useSingleSelectDropdown(commonService.getStates, {
    labelKey: "name",
    valueKey: "id"
  });

  return {
    countryNameDropdownSingleSelect,
    stateNameDropdownSingleSelect
  };
}
