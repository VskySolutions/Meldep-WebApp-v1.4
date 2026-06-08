import { useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import financeExpenseAdvanceService from "../financeExpenseAdvance.service";

export default function projectTaskModule () {
  const advanceExpenseReferenceIdDropdown = useMultiSelectDropdown(financeExpenseAdvanceService.getAdvanceExpenseRequestList, {
    labelKey: "text",
    valueKey: "value"
  });

   const advanceExpenseStatusDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
  advanceExpenseReferenceIdDropdown,
  advanceExpenseStatusDropdown
  };
}
