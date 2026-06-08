export default [
  {
    path: "/department",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "department", component: () => import("modules/department/pages/index.vue"), meta: { requiresAuth: true, title: "Departments" } }
    ]
  }
];
