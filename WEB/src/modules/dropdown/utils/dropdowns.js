import manageDropdownsService from "modules/dropdown/dropdown.service";

export default function manageDropdownModule () {
  const getDropdownTypesByModuleNameForDropdown = async (moduleName) => {
    const resp = await manageDropdownsService.getDropdownTypeByModuleName(moduleName);
    return resp; // return resolved data
  };

  return {
    getDropdownTypesByModuleNameForDropdown
  };
}
