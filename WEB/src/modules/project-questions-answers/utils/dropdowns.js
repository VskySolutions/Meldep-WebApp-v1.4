import { useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function questionsAnswersModule () {
  const contributorTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    contributorTypeDropdownSingleSelect
  };
}
