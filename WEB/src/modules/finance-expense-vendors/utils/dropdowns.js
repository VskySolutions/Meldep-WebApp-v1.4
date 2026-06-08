import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import expenseVendorBankAccountService from "modules/finance-expense-vendors/financeExpenseVendors.service";
import commonService from "services/common.service";

export default function projectTaskModule () {
  const vendorNameDropdown = useMultiSelectDropdown(expenseVendorBankAccountService.getAllExpenseVendorListForDropdown, {
    labelKey: "vendorName",
    valueKey: "id"
  });

  const ownerNameDropdown = useMultiSelectDropdown(expenseVendorBankAccountService.getAllExpenseVendorListForDropdown, {
    labelKey: "person.fullName",
    valueKey: "person.id"
  });


  const vendorNameDropdownSingleSelect = useSingleSelectDropdown(expenseVendorBankAccountService.getAllExpenseVendorListForDropdown, {
    labelKey: "vendorName",
    valueKey: "id"
  });

  const expenseVendorBankAccountDropdownSingleSelect = useSingleSelectDropdown(expenseVendorBankAccountService.getAllVendorsAccountListForDropdown, {
    labelKey: "paymentType.dropDownValue",
    valueKey: "id"
  });

  const accountTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const vendorAccountTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    vendorNameDropdown,
    ownerNameDropdown,
    vendorNameDropdownSingleSelect,
    expenseVendorBankAccountDropdownSingleSelect,
    accountTypeDropdownSingleSelect,
    vendorAccountTypeDropdownSingleSelect
  };
}
