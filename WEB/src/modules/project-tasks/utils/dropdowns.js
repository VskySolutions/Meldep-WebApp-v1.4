import { useMultiSelectDropdown, useSingleSelectDropdown, useSingleSelectDropdownWithRowIndex } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import projectTaskService from "modules/project-tasks/projectTasks.service";

export default function projectTaskModule () {
  const projectTasksByProjectIdAndModuleIdForDropdown = useMultiSelectDropdown(projectTaskService.getAllProjectMultiTaskListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const projectTasksByProjectIdAndModuleIdForDropdownSingleSelect = useSingleSelectDropdown(projectTaskService.getAllProjectMultiTaskListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex = useSingleSelectDropdownWithRowIndex(projectTaskService.getProjectTaskListForDropdown, {
    labelKey: "name",
    valueKey: "id"
  });

  const projectTasksWithProjectForDropdown = useSingleSelectDropdown(projectTaskService.getAllProjectTaskWithProjectListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });
  // const projectTasksWithProjectForDropdown = useSingleSelectDropdown(
  //   async () => {
  //     const resp = await projectTaskService.getAllProjectTaskWithProjectListForDropdown();
  //     return resp
  //       .map((item) => {
  //         return {
  //           text: item.text,
  //           value: item.value
  //         };
  //       })
  //       .sort((a, b) => a.text.localeCompare(b.text));
  //   },
  //   {
  //     labelKey: "text",
  //     valueKey: "value"
  //   }
  // );

  const projectTaskPrioritiesForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const projectTaskStatusForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectTaskTagsDropdown = useMultiSelectDropdown(projectTaskService.getAllProjectTaskTagListForDropdown, {
    labelKey: "text",
    valueKey: "value",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const areaForDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const workspaceForDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectTaskActionForDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectTaskPriorityForDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectTaskTypeForDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const sortByFilterDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    projectTasksByProjectIdAndModuleIdForDropdown,
    projectTasksByProjectIdAndModuleIdForDropdownSingleSelect,
    projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex,
    projectTasksWithProjectForDropdown,
    projectTaskPrioritiesForDropdown,
    projectTaskStatusForDropdown,
    projectTaskTagsDropdown,
    areaForDropdownSingleSelect,
    workspaceForDropdownSingleSelect,
    projectTaskActionForDropdownSingleSelect,
    projectTaskPriorityForDropdownSingleSelect,
    projectTaskTypeForDropdownSingleSelect,
    sortByFilterDropdownSingleSelect
  };
}
