export default [
  {
    path: "/settings",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "settings", component: () => import("modules/settings/pages/index.vue"), meta: { requiresAuth: true, siteMember: true, title: "Settings" } }
    ]
  }
];
