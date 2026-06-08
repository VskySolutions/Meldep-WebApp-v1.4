export default [
  {
    path: "/sites-items",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "sites-items", component: () => import("modules/sites-items/pages/index.vue"), meta: { requiresAuth: true, title: "Sites Items" } }
    ]
  }
];
