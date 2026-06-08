import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import usersService from "modules/user-management/userManagement.service";

export default function userModule () {
  const allUsersForDropdown = useMultiSelectDropdown(usersService.getAllUserListForDropdown, {
    valueKey: "id",
    labelFn: (item) => {
      if (item.person) {
        return `${item.person.firstName ?? ""} ${item.person.lastName ?? ""}`.trim();
      }
      return "Unknown";
    }
  });

  const userDropdownSingleSelect = useSingleSelectDropdown(usersService.getAllUserListForDropdown, {
    valueKey: "id",
    labelFn: (item) => {
      if (item.person) {
        return `${item.person.firstName ?? ""} ${item.person.lastName ?? ""}`.trim();
      }
      return "Unknown";
    }
  });

  return {
    allUsersForDropdown,
    userDropdownSingleSelect
  };
}
