export default [
  {
    path: "/project-targetplan",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/project-targetplan/weeklyplanner", name: "weeklyplanner", component: () => import("modules/project-targetplan/pages/weeklyPlanner.vue"), meta: { requiresAuth: true, title: "Weekly Target Plan" } },
      { path: "/project-targetplan/monthlyplanner", name: "monthlyplanner", component: () => import("modules/project-targetplan/pages/monthlyPlanner.vue"), meta: { requiresAuth: true, title: "Monthly Target Plan" } }
    ]
  }
];
