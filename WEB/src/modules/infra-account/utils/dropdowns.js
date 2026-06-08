import { useSingleSelectDropdown, useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import infraAccountService from "src/modules/infra-account/infraAccount.service";

export default function infraAccountModule () {
  const providerTypesForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const infraAccountsForDropdown = useMultiSelectDropdown(infraAccountService.getInfraAccountListForDropdown, {
    labelKey: (item) =>
      `${item.customerId ?? ""} (${item.provider?.dropDownValue ?? ""})`,
    valueKey: "id"
  });

  const itemTypesForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const ownershipTypesForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const paymentTermsForDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const providerTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const infraWalletTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const itemTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const ownershipTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const paymentTermDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const infraAccountDropdownSingleSelect = useSingleSelectDropdown(infraAccountService.getInfraAccountListForDropdown, {
    labelKey: (item) =>
      `${item.customerId ?? ""} (${item.provider?.dropDownValue ?? ""})`,
    valueKey: "id"
  });

  return {
    providerTypesForDropdown,
    infraAccountsForDropdown,
    itemTypesForDropdown,
    ownershipTypesForDropdown,
    paymentTermsForDropdown,
    providerTypeDropdownSingleSelect,
    infraWalletTypeDropdownSingleSelect,
    itemTypeDropdownSingleSelect,
    ownershipTypeDropdownSingleSelect,
    paymentTermDropdownSingleSelect,
    infraAccountDropdownSingleSelect
  };
}
