export default [
  {
    path: "/employee-org-structure",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "employee-org-structure", component: () => import("modules/employee-org-structure/pages/index.vue"), meta: { requiresAuth: true, title: "Employee Org Structure" } },
      { path: "/employee-org-structure-preview", name: "employee-org-structure-preview", component: () => import("modules/employee-org-structure/pages/preview.vue"), meta: { requiresAuth: true, title: "Preview Employee Org Structure" } }
    ]
  }
];
