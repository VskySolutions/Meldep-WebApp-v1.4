import { LocalStorage } from "quasar";

// Analytics pages are reached by URL (there is no menu entry yet) and are for administrators only: the API
// checks the same roles. The guard reads the stored user directly rather than the Pinia store so it works
// on a cold load, before any component has touched the store.
export const ADMIN_ROLES = ["admin", "site-super-admin", "system-super-admin"];

export function isAdminUser () {
  const roles = LocalStorage.getItem("user")?.roles || [];
  return ADMIN_ROLES.some((role) => roles.includes(role));
}

// Vue Router 4 style: return true to continue, or the redirect target.
function requireAdmin () {
  return isAdminUser() ? true : "/dashboard";
}

export default [
  {
    path: "/analytics",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", redirect: "/analytics/timesheet-register" },
      {
        path: "timesheet-register",
        name: "analytics-timesheet-register",
        component: () => import("modules/analytics/pages/timesheetRegister.vue"),
        meta: { requiresAuth: true, title: "Timesheet Register" },
        beforeEnter: requireAdmin
      }
    ]
  }
];
