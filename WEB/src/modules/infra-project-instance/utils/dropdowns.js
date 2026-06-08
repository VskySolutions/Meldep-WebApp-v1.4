import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import infraProjectInstanceService from "modules/infra-project-instance/infraProjectInstance.service";
import commonService from "src/services/common.service";

export default function infraProjectInstanceModule () {
  const infraProjectInstanceForDropdown = useMultiSelectDropdown(infraProjectInstanceService.getAllInfraProjectInstanceListForDropdown);

  const platformsForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const instanceTypesForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const platformDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const instanceTypeDropdownSingleSelect = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  // Single Select
  const infraProjectInstanceTypeDropdownSingleSelect = useSingleSelectDropdown(
    infraProjectInstanceService.getAllInfraProjectInstanceListForDropdown,
    {
      labelKey: (item) => `${item.instanceType?.dropDownValue} (${item.url})`,
      valueKey: "id"
    }
  );

  const infraProjectInstancePlatformDropdownSingleSelect = useSingleSelectDropdown(
    infraProjectInstanceService.getAllInfraProjectInstanceListForDropdown,
    {
      labelKey: (item) => `${item.platform.dropDownValue} - ${item.url}`,
      valueKey: "id"
    }
  );

  return {
    infraProjectInstanceForDropdown,
    platformsForDropdown,
    instanceTypesForDropdown,
    platformDropdownSingleSelect,
    instanceTypeDropdownSingleSelect,
    infraProjectInstanceTypeDropdownSingleSelect,
    infraProjectInstancePlatformDropdownSingleSelect
  };
}
