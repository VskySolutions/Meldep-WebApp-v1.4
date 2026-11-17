export default [
  {
    path: "/issue",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "issue", component: () => import("modules/issue/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/issue"], level: "view" }, title: "Issues" } }
    ]
  }
];
