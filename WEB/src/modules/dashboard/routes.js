export default [
  {
    path: "/dashboard",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "dashboard", component: () => import("modules/dashboard/pages/index.vue"), meta: { requiresAuth: true, siteMember: true, title: "Dashboard" } }
    ],
    meta: {
        permission: {
          menu: "/dashboard",
          level: "View"
      }
    }
  }
];
