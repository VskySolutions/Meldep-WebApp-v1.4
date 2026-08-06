import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function projectActionItemModule () {
  const projectActionItemPriorityForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectActionItemPrioritySingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    projectActionItemPriorityForDropdown,
    projectActionItemPrioritySingleSelect
  };
}
