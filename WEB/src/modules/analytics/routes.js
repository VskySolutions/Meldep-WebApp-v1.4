// Each register is its own menu (ModulesMenus.Link), so access is granted per register in the permission
// screens. permissionGuard checks meta.permission against the Pinia permission store on every navigation,
// including a direct URL or a refresh, and the API checks the same link on every request.
export const TIMESHEET_REGISTER_LINK = "/analytics/timesheet-register";
export const ATTENDANCE_REGISTER_LINK = "/analytics/attendance-register";

export default [
  {
    path: "/analytics",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", redirect: TIMESHEET_REGISTER_LINK },

      // #region Timesheet Register
      {
        path: "timesheet-register",
        name: "analytics-timesheet-register",
        component: () => import("modules/analytics/pages/timesheetRegister.vue"),
        meta: { requiresAuth: true, permission: { menu: [TIMESHEET_REGISTER_LINK], level: "view" }, title: "Timesheet Register" }
      },
      // #endregion Timesheet Register

      // #region TimeInTimeOut-MovementRegister (Attendance Register)
      {
        path: "attendance-register",
        name: "analytics-attendance-register",
        component: () => import("modules/analytics/pages/attendanceRegister.vue"),
        meta: { requiresAuth: true, permission: { menu: [ATTENDANCE_REGISTER_LINK], level: "view" }, title: "Attendance Register" }
      }
      // #endregion TimeInTimeOut-MovementRegister (Attendance Register)
    ]
  }
];
