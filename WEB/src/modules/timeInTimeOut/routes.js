export default [
  {
    path: "/time-in-time-out",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "time-in-time-out", component: () => import("modules/timeInTimeOut/pages/index.vue"), meta: { requiresAuth: true, title: "Time In Time Out" } }
    ]
  }
];
