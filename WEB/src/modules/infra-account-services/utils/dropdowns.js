import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import infraAccountsServicesService from "modules/infra-account-services/infraAccountServices.service";

export default function infraAccountServiceModule () {
  const infraAccountServicesForDropdown = useMultiSelectDropdown(infraAccountsServicesService.getAllInfraServiceListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  // Status (Single Select)
  const infraAccountServiceForDropdownSingleSelect = useSingleSelectDropdown(infraAccountsServicesService.getAllInfraServiceListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  return {
    infraAccountServicesForDropdown,
    infraAccountServiceForDropdownSingleSelect
  };
}
