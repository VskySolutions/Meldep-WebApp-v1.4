import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import customerService from "src/modules/customer/customer.service";

export default function companyContactsModule () {
  const companyContactNameDropdown = useMultiSelectDropdown(customerService.getAllCompanyContactList, {
    labelKey: "person.fullName",
    valueKey: "id"
  });

  const companyContactDropdownSingleSelect = useSingleSelectDropdown(customerService.getAllContactListForDropdown, {
    labelKey: "person.fullName",
    valueKey: "id"
  });

  return {
    companyContactNameDropdown,
    companyContactDropdownSingleSelect
  };
}
