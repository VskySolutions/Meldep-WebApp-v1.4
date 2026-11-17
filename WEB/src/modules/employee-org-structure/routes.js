export default [
  {
    path: "/employee-org-structure",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "employee-org-structure", component: () => import("modules/employee-org-structure/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/employee-org-structure"], level: "view" }, title: "Employee Org Structure" } },
      { path: "/employee-org-structure-preview", name: "employee-org-structure-preview", component: () => import("modules/employee-org-structure/pages/preview.vue"), meta: { requiresAuth: true, permission: { menu: ["/employee-org-structure"], level: "view" }, title: "Preview Employee Org Structure" } }
    ]
  }
];
