import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import sopTemplateService from "modules/sop-template/sopTemplate.service";

export default function SOPTemplateModule () {
  // Status (Single Select)
  const inputTypeDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const sopTemplateDropdownSingleSelect = useSingleSelectDropdown(sopTemplateService.getAllSOPTemplateListForDropdown);

  const sopTemplatesDropdown = useMultiSelectDropdown(sopTemplateService.getAllSOPTemplateListForDropdown, {
    labelKey: "name",
    valueKey: "id"
  });

  return {
    inputTypeDropdown,
    sopTemplateDropdownSingleSelect,
    sopTemplatesDropdown
  };
}
