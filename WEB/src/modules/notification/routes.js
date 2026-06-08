export default [
  {
    path: "/notifications",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "notifications", component: () => import("modules/notification/pages/index.vue"), meta: { requiresAuth: true, title: "Notifications" } }
    ]
  }
];
