export default [
  {
    path: "/project-planning",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/project-planning/workboard/:id?", name: "workboard", component: () => import("modules/project-planning/pages/workboard.vue"), meta: { requiresAuth: true, title: "Project WorkBoard" } },
      { path: "/all-project-planner", name: "project-desktop", component: () => import("modules/project-planning/pages/allProjectPlanner.vue"), meta: { requiresAuth: true, title: "All Project Planner" } },
      { path: "/project-planning/calendar", name: "project-calendar", component: () => import("modules/project-planning/pages/projectTaskCalendar.vue"), meta: { requiresAuth: true, title: "Project Calendar" } }
    ]
  }
];
