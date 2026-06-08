import { useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import moduleService from "modules/module/module.service";

export default function moduleModule () {
  const modulesDropdown = useMultiSelectDropdown(moduleService.getModules, {
    labelKey: "name",
    valueKey: "id"
  });

  const moduleMenusDropdown = useMultiSelectDropdown(moduleService.getMenu, {
    labelKey: "menuName",
    valueKey: "id"
  });

  return {
    modulesDropdown,
    moduleMenusDropdown
  };
}
