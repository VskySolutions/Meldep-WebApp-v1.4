import { useSingleSelectDropdown, useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import leadsService from "modules/lead/lead.service";
import commonService from "services/common.service";

export default function leadModule () {
  const leadNameDropdown = useSingleSelectDropdown(leadsService.getLeadListForDropdwon, {
    labelKey: "person.fullName",
    valueKey: "person.id"
  });

  const leadSourceDropdown = useSingleSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const leadStageDropdown = useSingleSelectDropdown(leadsService.getLeadStages, {
    labelKey: "stageName",
    valueKey: "id"
  });

  const leadActivityDropdown = useSingleSelectDropdown(leadsService.getAllLeadActivityListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const leadGroupsDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    leadNameDropdown,
    leadSourceDropdown,
    leadStageDropdown,
    leadActivityDropdown,
    leadGroupsDropdown
  };
}
