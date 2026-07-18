import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import inventoryService from "modules/inventory/inventory.service";

export default function inventoryModule () {
  const itemTypeForDropdown = useMultiSelectDropdown(inventoryService.getAllItemType, {
    labelKey: "name",
    valueKey: "id"
  });

  const officeLocationForDropdown = useMultiSelectDropdown(commonService.getDropDownForSite, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const inventoryStatusForDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  return {
    itemTypeForDropdown,
    officeLocationForDropdown,
    inventoryStatusForDropdown
  };
}
