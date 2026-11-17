import { LocalStorage } from "quasar";
import { usePermissionStore } from "stores/permissions";

export const CATCH_ALL_ROUTE = "catch-all";

// Renders the access-denied page while keeping the requested URL, so a refresh checks again.
function deniedLocation (to) {
  return {
    name: CATCH_ALL_ROUTE,
    params: { catchAll: to.path.slice(1).split("/") },
    query: to.query,
    hash: to.hash,
    replace: true
  };
}

// True if this user may open the route. An undeclared protected route is denied.
export function isRouteAllowed (route, permissions) {
  // Admin APIs always require this, so the page follows them in every rollout mode.
  if (route.matched.some(record => record.meta.siteAdmin) && !permissions.isSiteAdmin) return false;

  const required = route.matched.map(record => record.meta.permission).filter(Boolean);
  const isDeclared = required.length > 0 || route.matched.some(record => record.meta.siteMember || record.meta.siteAdmin);

  if (!isDeclared) return !permissions.isEnforced([]);

  // Every record is checked, because merged meta would let a child override its parent.
  return required.every(rule => permissions.can(rule.menu, rule.level));
}

// Decides navigation for every route. This is for usability: the API checks every request itself.
// The page's code doesn't load until this passes, so a denied page never shows or requests data.
export async function permissionGuard (to, next, store) {
  const permissions = usePermissionStore(store);

  if (to.name === CATCH_ALL_ROUTE || to.matched.some(record => record.meta.public)) {
    return next();
  }

  permissions.deniedPath = null;

  if (!LocalStorage.getItem("token")) {
    return next({ path: "/auth/login", query: { redirect: to.fullPath } });
  }

  try {
    await permissions.load();
  } catch (error) {
    // A 401 is already sending the user to login. Otherwise access is unknown, so never allow.
    if (error.response?.status === 401) return next(false);
    permissions.deniedPath = to.path;
    return next(deniedLocation(to));
  }

  if (!permissions.isSiteMember && permissions.siteGateMode === "Enforce" && to.name !== "no_site_access") {
    return next({ name: "no_site_access" });
  }

  if (!isRouteAllowed(to, permissions)) {
    permissions.deniedPath = to.path;
    return next(deniedLocation(to));
  }

  return next();
}
