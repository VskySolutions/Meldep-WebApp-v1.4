import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import testPlanService from "modules/test-plan/testPlan.service";
import commonService from "services/common.service";

export default function testPlanModule () {
  const testPlansByProjectIdForDropdown = useMultiSelectDropdown(testPlanService.getAllTestPlanListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const testPlansByProjectIdForDropdownSingleSelect = useSingleSelectDropdown(testPlanService.getAllTestPlanListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const areaDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const workspaceDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    testPlansByProjectIdForDropdown,
    testPlansByProjectIdForDropdownSingleSelect,
    areaDropdown,
    workspaceDropdown
  };
}
