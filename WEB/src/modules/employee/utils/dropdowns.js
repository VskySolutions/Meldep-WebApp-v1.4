import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import employeesService from "src/modules/employee/employee.service";
import commonService from "services/common.service";

export default function employeeModule () {
  const activeEmployeesDropdown = useMultiSelectDropdown(employeesService.getAllActiveEmployeesListForDropdown, {
    labelKey: "person.fullName",
    valueKey: "id"
  });

  const allEmployeesForDropdown = useMultiSelectDropdown(employeesService.getAllEmployeesListForDropdown, {
    labelKey: "person.fullName",
    valueKey: "person.id"
  });

  const employeeDesignationForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const employeeTypeForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const employeeOrgLocationForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  //  Employee (Single Select)
  const activeEmployeesDropdownSingleSelect = useSingleSelectDropdown(employeesService.getAllActiveEmployeesListForDropdown, {
    labelKey: "person.fullName",
    valueKey: "id"
  });

  const employeeDesignationDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const employeeTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDownForSite, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const employeeStatusDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDownForSite, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const employeeDesignationBySiteIdDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDownForSite, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const employeeShiftDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDownForSite, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const employeeOrgLocationDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDownForSite, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    activeEmployeesDropdown,
    allEmployeesForDropdown,
    employeeDesignationForDropdown,
    employeeTypeForDropdown,
    employeeOrgLocationForDropdown,
    activeEmployeesDropdownSingleSelect,
    employeeDesignationDropdownSingleSelect,
    employeeTypeDropdownSingleSelect,
    employeeStatusDropdownSingleSelect,
    employeeDesignationBySiteIdDropdownSingleSelect,
    employeeShiftDropdownSingleSelect,
    employeeOrgLocationDropdownSingleSelect
  };
}
