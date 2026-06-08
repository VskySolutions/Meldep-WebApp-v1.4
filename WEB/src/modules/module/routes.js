export default [
  {
    path: "/modules",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "modules", component: () => import("modules/module/pages/index.vue"), meta: { requiresAuth: true, title: "Modules" } },
      { path: ":id/menus", name: "menus", component: () => import("modules/module/pages/menu.vue"), meta: { title: "Menus" } }
    ]
  }
];
