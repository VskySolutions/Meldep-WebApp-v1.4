import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import projectService from "modules/project/projects.service";
import employeesService from "src/modules/employee/employee.service";
import { color } from "d3";

export default function projectModule () {
  const projectNameDropdown = useMultiSelectDropdown(projectService.getProjectsListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });
  // Single Select
  const projectNameDropdownSingleSelect = useSingleSelectDropdown(projectService.getProjectsListForDropdown, {
    labelKey: "text",
    valueKey: "value",
    dataKey: "statusText"
  });

  const projectUsersByProjectIdForDropdown = useMultiSelectDropdown(projectService.getProjectUserByProjectId, {
    labelKey: "user.person.fullName",
    valueKey: "user.id"
  });

  const projectCategoriesDropdown = useMultiSelectDropdown(commonService.getDropdownTypeByGroupName, {
    labelKey: "type",
    valueKey: "id"
  });

  const projectEmployeesForDropdown = useMultiSelectDropdown(projectService.getProjectEmployees, {
    labelKey: "text",
    valueKey: "value"
  });

  const projectUserByProjectIdDropdownSingleSelect = useSingleSelectDropdown(projectService.getProjectUserByProjectId, {
    labelKey: "user.person.fullName",
    valueKey: "user.id"
  });

 const projectCharterEmployeesWithWeeklyPlanHoursForDropdown =  useSingleSelectDropdown(projectService.getProjectCharterEmployeesWithWeeklyPlanHoursByProjectId, {
      valueKey: "employee.id",
      labelKey: (item) => {
        const weekendHours = item.employee.employeeAssignedHours
          ? item.employee.employeeAssignedHours
              .map(h => {
                const date = new Date(h.weekendDate).toLocaleDateString("en-US", {
                  month: "2-digit",
                  day: "2-digit"
                });
                return `${date}-${h.totalHours}`;
              })
              .join("; ")
          : "0";

        return `${item.employee.person.fullName} (${weekendHours})`;
      }
    }
  );

  // 🔹 Status (Single Select)
  const projectCategoryDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropdownTypeByGroupName, {
    labelKey: "type",
    valueKey: "id"
  });

  const projectSubCategoryDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropdownByTypeId, {
    labelKey: "dropdownValue",
    valueKey: "id",
    dataKey: "description"
  });

  const projectTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectEmployeeDropdownSingleSelect = useSingleSelectDropdown(projectService.getProjectEmployees, {
    labelKey: "text",
    valueKey: "value"
  });

  const projectActiveInActiveDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectApproverDropdownSingleSelect = useSingleSelectDropdown(employeesService.getAllActiveEmployeesListForDropdown, {
    labelKey: "person.fullName",
    valueKey: "id"
  });

  const projectPriorityDropdownSingleSelect = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectPrioritiesDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const projectTypesDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectStatusDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectTagsDropdown = useMultiSelectDropdown(projectService.getAllProjectTagListForDropdown, {
    labelKey: "text",
    valueKey: "value",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  return {
    projectNameDropdown,
    projectNameDropdownSingleSelect,
    projectUsersByProjectIdForDropdown,
    projectUserByProjectIdDropdownSingleSelect,
    projectCategoriesDropdown,
    projectActiveInActiveDropdown,
    projectApproverDropdownSingleSelect,
    projectPriorityDropdownSingleSelect,
    projectPrioritiesDropdown,
    projectTypesDropdown,
    projectStatusDropdown,
    projectEmployeesForDropdown,
    projectCharterEmployeesWithWeeklyPlanHoursForDropdown,
    projectCategoryDropdownSingleSelect,
    projectSubCategoryDropdownSingleSelect,
    projectTypeDropdownSingleSelect,
    projectEmployeeDropdownSingleSelect,
    projectTagsDropdown
  };
}
