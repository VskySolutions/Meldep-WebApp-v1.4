import { usePermissionStore } from "stores/permissions";

// For hiding actions in a page. This is for usability: the API still checks every request.
export default function usePermission () {
  const permissions = usePermissionStore();

  return {
    can: (links, level = "view") => permissions.can(links, level),
    canView: links => permissions.can(links, "view"),
    canManage: links => permissions.can(links, "manage")
  };
}
