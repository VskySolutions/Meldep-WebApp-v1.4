import { useMultiSelectDropdown, useSingleSelectDropdown, useSingleSelectDropdownWithRowIndex } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import projectModulesService from "modules/project-modules/projectModules.service";

export default function projectModuleOfProjectModule () {
  const projectModulesByProjectIdForDropdown = useMultiSelectDropdown(projectModulesService.getAllProjectModuleListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });
  const projectModulesByProjectIdForDropdownSingleSelect = useSingleSelectDropdown(projectModulesService.getAllProjectModuleListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const projectModulesByProjectIdForDropdownSingleSelectWithRowIndex = useSingleSelectDropdownWithRowIndex(projectModulesService.getAllProjectModuleListForDropdown, {
      labelKey: "text",
      valueKey: "value"
  });

  const projectModuleStatusForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const projectModuleStatusForDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    projectModulesByProjectIdForDropdown,
    projectModulesByProjectIdForDropdownSingleSelect,
    projectModulesByProjectIdForDropdownSingleSelectWithRowIndex,
    projectModuleStatusForDropdown,
    projectModuleStatusForDropdownSingleSelect
  };
}
