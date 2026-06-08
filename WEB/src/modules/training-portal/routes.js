export default [
  {
    path: "/training-portal",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "training-portal", component: () => import("modules/training-portal/pages/index.vue"), meta: { requiresAuth: true, title: "Training Portals" } }
    ]
  }
];
