export default [
  {
    path: "/movement-register",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "movement-register", component: () => import("modules/movementRegister/pages/index.vue"), meta: { requiresAuth: true, title: "Movement Registers" } }
    ]
  }
];
