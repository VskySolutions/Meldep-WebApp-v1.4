export default [
  {
    path: "/modules",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "modules", component: () => import("modules/module/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/modules"], level: "view" }, title: "Modules" } },
      { path: ":id/menus", name: "menus", component: () => import("modules/module/pages/menu.vue"), meta: { requiresAuth: true, permission: { menu: ["/modules"], level: "view" }, title: "Menus" } }
    ]
  }
];
