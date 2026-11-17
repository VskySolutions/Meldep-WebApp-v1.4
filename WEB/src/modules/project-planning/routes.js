export default [
  {
    path: "/project-planning",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/project-planning/workboard/:id?", name: "workboard", component: () => import("modules/project-planning/pages/workboard.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Project WorkBoard" } },
      { path: "/all-project-planner", name: "project-desktop", component: () => import("modules/project-planning/pages/allProjectPlanner.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Project Task Planner" } },
      { path: "/project-planning/calendar", name: "project-calendar", component: () => import("modules/project-planning/pages/projectTaskCalendar.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Project Calendar" } }
    ]
  }
];
