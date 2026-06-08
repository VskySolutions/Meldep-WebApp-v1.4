export default [
  {
    path: "/inventory",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "inventory", component: () => import("modules/inventory/pages/index.vue"), meta: { requiresAuth: true, title: "Inventory" } }
    ]
  }
];
