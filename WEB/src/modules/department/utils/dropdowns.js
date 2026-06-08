import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import departmentService from "modules/department/department.service";

export default function departmentModule () {
  const departmentNameDropdown = useMultiSelectDropdown(departmentService.getAllDepartmentListForDropdown);

  const departmentNameDropdownSingleSelect = useSingleSelectDropdown(departmentService.getAllDepartmentListForDropdown, {
    labelKey: "name",
    valueKey: "id"
  });

  return {
    departmentNameDropdown,
    departmentNameDropdownSingleSelect
  };
}
