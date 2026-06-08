export default [
  {
    path: "/user-management",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "user-management", component: () => import("modules/user-management/pages/index.vue"), meta: { requiresAuth: true, title: "Users" } }
    ]
  }
];
