import { useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import projectService from "modules/project/projects.service";
import commonService from "services/common.service";

export default function projectTargetPlanModule () {
  // Single Select
  const weeklyEmployeeDropdownSingleSelect = useSingleSelectDropdown(projectService.getAllEmployeesWithHoursForWeeklyMonthlyPlanning, {
    labelKey: "employeeName",
    valueKey: "employeeId"
  });

  const ProjectPlanTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    weeklyEmployeeDropdownSingleSelect,
    ProjectPlanTypeDropdownSingleSelect
  };
}
