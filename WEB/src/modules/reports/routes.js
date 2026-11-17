export default [
  {
    path: "/reports",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "reports", component: () => import("modules/reports/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/reports"], level: "view" }, title: "Report Portal" } },
      { path: "/report-list/:reportId", name: "report-list", component: () => import("modules/reports/pages/reportList.vue"), meta: { requiresAuth: true, permission: { menu: ["/reports"], level: "view" }, title: "Reports" } },
      { path: "/reports/assign-users-to-report", name: "assign-users-to-report", component: () => import("modules/reports/pages/assignUsersToReport.vue"), meta: { requiresAuth: true, permission: { menu: ["/reports"], level: "view" }, title: "Report Security" } },
      { path: "/reports/groupRoleAssignmentList", name: "groupRoleAssignmentList", component: () => import("modules/reports/pages/groupRoleAssignmentList.vue"), meta: { requiresAuth: true, permission: { menu: ["/reports"], level: "view" }, title: "Report Group Role Assignment" } }
    ]
  }
];
