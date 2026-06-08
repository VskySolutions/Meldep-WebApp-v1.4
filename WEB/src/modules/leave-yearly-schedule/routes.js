export default [
  {
    path: "/leave-yearly-schedule",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "leave-yearly-schedule", component: () => import("modules/leave-yearly-schedule/pages/index.vue"), meta: { requiresAuth: true, title: "Leave Schedule" } }
    ]
  }
];
