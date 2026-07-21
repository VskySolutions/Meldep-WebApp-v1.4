import { useSingleSelectDropdown, useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function sOPProcessModule () {
  // Status (Single Select)
  const sopProcessCategoryDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropdownTypeByGroupName, {
    labelKey: "type",
    valueKey: "id"
  });

  const sopProcessSubCategoryDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropdownByTypeId, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const sopProcessStatusDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  // Status (Multi Select)
  const sopProcessCategoriesDropdown = useMultiSelectDropdown(commonService.getDropdownTypeByGroupName, {
    labelKey: "type",
    valueKey: "id"
  });

  const sopProcessSubCategoriesDropdown = useMultiSelectDropdown(commonService.getDropdownByTypeId, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const sopProcessStatusesDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    sopProcessCategoryDropdownSingleSelect,
    sopProcessSubCategoryDropdownSingleSelect,
    sopProcessStatusDropdownSingleSelect,
    sopProcessCategoriesDropdown,
    sopProcessSubCategoriesDropdown,
    sopProcessStatusesDropdown
  };
}
