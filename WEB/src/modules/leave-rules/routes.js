export default [
  {
    path: "/leave-rules",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "leave-rules", component: () => import("modules/leave-rules/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/leave-rules"], level: "view" }, title: "Leave Rules" } }
    ]
  }
];
