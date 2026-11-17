export default [
  {
    path: "/project",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "project", component: () => import("modules/project/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Projects" } },
      { path: "/project/assign-users-to-project", name: "assign-users-to-project", component: () => import("modules/project/pages/assignUsersToProject.vue"), meta: { requiresAuth: true, permission: { menu: ["/project/assign-users-to-project"], level: "view" }, title: "Project Security" } },
      { path: "/project/projectTaskNotes", name: "projectTaskNotes", component: () => import("modules/project/pages/projectTaskNotes.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Project Task Notes" } }
    ],
    meta: {
        permission: {
          menu: "/project",
          level: "View"
      }
    }
  }
];
