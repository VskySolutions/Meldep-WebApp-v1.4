export default [
  {
    path: "/tags",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "tags", component: () => import("modules/tags/pages/index.vue"), meta: { requiresAuth: true, title: "Tag Master" } }
    ]
  }
];
