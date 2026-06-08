export default [
  {
    path: "/employee",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "employee", component: () => import("modules/employee/pages/index.vue"), meta: { requiresAuth: true, title: "Employees" } }
    ]
  }
];
