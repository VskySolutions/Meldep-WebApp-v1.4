import { useSingleSelectDropdown, useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import sitesService from "modules/sites/site.service"

export default function siteModule () {
  const timeZoneDropdownSingleSelect = useSingleSelectDropdown(sitesService.getAllTimeZoneListForDropdown, {
    labelKey: "displayText",
    valueKey: "name"
  });

  const siteRolesDropdown = useMultiSelectDropdown(sitesService.getAllSitesRoleListForDropdown, {
    labelKey: "applicationRole.name",
    valueKey: "id"
  });

  const siteModulesDropdown = useMultiSelectDropdown(sitesService.getAllSiteModuleListForDropdown, {
    labelKey: "modules.name",
    valueKey: "modules.id"
  });

  const siteModuleMenusDropdown = useMultiSelectDropdown(sitesService.getAllSiteModuleMenuListForDropdown, {
    labelKey: "modulesMenus.displayName",
    valueKey: "modulesMenus.id"
  });

  return {
    timeZoneDropdownSingleSelect,
    siteRolesDropdown,
    siteModulesDropdown,
    siteModuleMenusDropdown
  };
}
