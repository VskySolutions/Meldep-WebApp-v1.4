export default [
  {
    path: "/my-daily-planner",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "my-daily-planner", component: () => import("modules/my-daily-planner/pages/index.vue"), meta: { requiresAuth: true, title: "My Daily Planner" } }
    ]
  }
];
