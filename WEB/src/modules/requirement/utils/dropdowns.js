import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import requirementService from "../requirement.service";

export default function requirementModule () {
  const requirementStatusForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  // single select
  const requirementStatusDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const requirementPriorityDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const requirementTagsDropdown = useMultiSelectDropdown(requirementService.getAllRequirementTagListForDropdown, {
    labelKey: "text",
    valueKey: "value",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const requirementIdentifiedUserTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const requirementApprovalStatusDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const requirementTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const requirementTypeForDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    requirementStatusForDropdown,
    requirementStatusDropdownSingleSelect,
    requirementPriorityDropdownSingleSelect,
    requirementTagsDropdown,
    requirementIdentifiedUserTypeDropdownSingleSelect,
    requirementApprovalStatusDropdownSingleSelect,
    requirementTypeDropdownSingleSelect,
    requirementTypeForDropdown
  };
}
