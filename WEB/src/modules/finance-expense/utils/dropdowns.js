import { useSingleSelectDropdown, useSingleSelectDropdownWithRowIndex } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";

export default function projectTaskModule () {
  const locationDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const intervalTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const expenseCategoryForDropdownSingleSelectWithRowIndex = useSingleSelectDropdownWithRowIndex(commonService.getDropdownTypeByGroupName, {
    labelKey: "type",
    valueKey: "id"
  });

  const expenseSubCategoryForDropdownSingleSelectWithRowIndex = useSingleSelectDropdownWithRowIndex(commonService.getDropdownByTypeId, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
   locationDropdownSingleSelect,
   intervalTypeDropdownSingleSelect,
   expenseCategoryForDropdownSingleSelectWithRowIndex,
   expenseSubCategoryForDropdownSingleSelectWithRowIndex
  };
}
