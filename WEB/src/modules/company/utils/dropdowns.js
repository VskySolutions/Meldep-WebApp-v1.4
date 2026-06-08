import { useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import companyService from "modules/company/company.service";
import commonService from "services/common.service";

export default function companyModule () {
  const companyNameDropdownSingleSelect = useSingleSelectDropdown(companyService.getCompanyDropdownList, {
    labelKey: "text",
    valueKey: "value"
  });

  const businessTypeDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const primaryEmployeesForDropdownSingleSelect = useSingleSelectDropdown(companyService.getPrimaryEmployeeDropdownList, {
    labelKey: "employee.person.fullName",
    valueKey: "employee.id"
  });

  const businessTypeForSiteIdDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDownForSite, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    companyNameDropdownSingleSelect,
    businessTypeDropdown,
    primaryEmployeesForDropdownSingleSelect,
    businessTypeForSiteIdDropdownSingleSelect
  };
}
