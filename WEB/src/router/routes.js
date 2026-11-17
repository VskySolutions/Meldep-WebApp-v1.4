const routes = [
  {
    path: "/",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "index", component: () => import("modules/dashboard/pages/index.vue"), meta: { requiresAuth: true, siteMember: true, title: "Home" } },
      { path: "terms", name: "terms", component: () => import("pages/terms.vue"), meta: { requiresAuth: true, siteMember: true, title: "Terms & Conditions" } },
      { path: "privacy", name: "privacy", component: () => import("pages/privacy.vue"), meta: { requiresAuth: true, siteMember: true, title: "Privacy Policy" } }
    ]
  },
  { path: "/approve-leave", name: "approve_leave", component: () => import("pages/approve_leave.vue"), meta: { requiresAuth: true, permission: { menu: ["/approve-leaves", "/forward-leaves"], level: "view" }, title: "Approve Leave" } },
  { path: "/not-authorized", name: "not_authorized", component: () => import("src/pages/not_authorized.vue"), meta: { public: true, title: "Not Authorized" } },
  { path: "/no-site-access", name: "no_site_access", component: () => import("src/pages/no_site_access.vue"), meta: { public: true, requiresAuth: true, title: "No Site Access" } },
  { path: "/under-maintenance", name: "under_maintenance", component: () => import("src/pages/under_maintenance.vue"), meta: { public: true, title: "Under Maintenance" } },
  { path: "/:catchAll(.*)*", name: "catch-all", component: () => import("pages/error.vue"), meta: { public: true, title: "Error" } }
];

export default routes;
