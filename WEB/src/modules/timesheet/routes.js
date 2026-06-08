export default [
  {
    path: "/timesheet",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "timesheet", component: () => import("modules/timesheet/pages/index.vue"), meta: { requiresAuth: true, title: "Timesheets" } },
      { path: "/weekly-timesheet", name: "weekly-timesheet", component: () => import("modules/timesheet/pages/weeklyTimesheet.vue"), meta: { requiresAuth: true, title: "Weekly Timesheets" } },
      { path: "/billing-processing", name: "billing-processing", component: () => import("modules/timesheet/pages/billing.vue"), meta: { requiresAuth: true, title: "Billing Processing" } },
      { path: "/billing-processing-employee", name: "billing-processing-employee", component: () => import("modules/timesheet/pages/billingProcessingByEmployee.vue"), meta: { requiresAuth: true, title: "Billing Processing Employee" } }
    ]
  }
];
