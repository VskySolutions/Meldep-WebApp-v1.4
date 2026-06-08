import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import customerService from "src/modules/customer/customer.service";
import commonService from "services/common.service";

export default function customerModule () {
  const customerNameDropdown = useMultiSelectDropdown(customerService.getAllCustomerListForDropdown, {
    valueKey: "id",
    labelFn: (item) => {
      if (item.company?.name) {
        return item.company.name;
      }
      if (item.person) {
        return `${item.person.firstName ?? ""} ${item.person.lastName ?? ""}`.trim();
      }
      return "Unknown";
    }
  });

  const parentCustomerDropdown = useMultiSelectDropdown(customerService.getAllParentCustomerList, {
    valueKey: "id",
    typeFn: (item) => (item.company ? "business" : "individual"),
    labelFn: (item) => {
      if (item.company?.name) {
        return item.company.name;
      }
      if (item.person) {
        return `${item.person.firstName ?? ""} ${item.person.lastName ?? ""}`.trim();
      }
      return "Unknown";
    }
  });

  const customerTypesDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const customerDropdownSingleSelect = useSingleSelectDropdown(customerService.getAllCustomerListForDropdown, {
    valueKey: "id",
    labelFn: (item) => {
      if (item.company?.name) {
        return item.company.name;
      }
      if (item.person) {
        return `${item.person.firstName ?? ""} ${item.person.lastName ?? ""}`.trim();
      }
      return "Unknown";
    }
  });

  const customerContactDropdownSingleSelect = useSingleSelectDropdown(customerService.getAllCustomerContactListForDropdown, {
    labelKey: "person.fullName",
    valueKey: "person.id"
  });

  const customerContactDropdown = useMultiSelectDropdown(customerService.getAllCustomerContactListForDropdown, {
    labelKey: "person.fullName",
    valueKey: "person.id"
  });

  const customerTypesDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const parentCustomerDropdownSingleSelect = useSingleSelectDropdown(customerService.getAllParentCustomerList, {
    valueKey: "id",
    typeFn: (item) => (item.company ? "business" : "individual"),
    labelFn: (item) => {
      if (item.company?.name) {
        return item.company.name;
      }
      if (item.person) {
        return `${item.person.firstName ?? ""} ${item.person.lastName ?? ""}`.trim();
      }
      return "Unknown";
    }
  });

  return {
    customerNameDropdown,
    parentCustomerDropdown,
    customerTypesDropdown,
    customerDropdownSingleSelect,
    customerContactDropdownSingleSelect,
    customerContactDropdown,
    customerTypesDropdownSingleSelect,
    parentCustomerDropdownSingleSelect
  };
}
