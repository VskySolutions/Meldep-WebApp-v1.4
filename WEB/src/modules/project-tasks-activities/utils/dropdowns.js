import { useMultiSelectDropdown, useSingleSelectDropdown, useSingleSelectDropdownWithRowIndex } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import projectTasksActivitiesService from "../projectTasksActivities.service";

export default function projectModule () {
  // Status (Single Select)
  const projectTaskActivityActiveInActiveDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectTaskActivityNameForDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    dataKey: "description"
  });

  const projectTaskActivityNameForDropdownSingleSelectWithRowIndex = useSingleSelectDropdownWithRowIndex(projectTasksActivitiesService.getProjectTaskActivityListForDropdown, {
    labelKey: "displayText",
    valueKey: "id",
    dataKey: "activityNameDescription"
  });

  const projectTaskActivityNameDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectTaskActivityStatusDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const projectTaskActivityStatusDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  return {
    projectTaskActivityActiveInActiveDropdown,
    projectTaskActivityNameDropdown,
    projectTaskActivityNameForDropdownSingleSelectWithRowIndex,
    projectTaskActivityStatusDropdown,
    projectTaskActivityNameForDropdownSingleSelect,
    projectTaskActivityStatusDropdownSingleSelect
  };
}
