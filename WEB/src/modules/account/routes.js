export default [
  {
    path: "/account",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "account", component: () => import("modules/account/pages/index.vue"), meta: { requiresAuth: true, title: "Account" } },
      { path: "profile", name: "profile", component: () => import("modules/account/pages/profile.vue"), meta: { requiresAuth: true, title: "My Profile" } },
      { path: "change-password", name: "change_password", component: () => import("modules/account/pages/change_password.vue"), meta: { requiresAuth: true, title: "Change Password" } },
      { path: "help-desk", name: "help_desk", component: () => import("modules/helpdesk/pages/index.vue"), meta: { requiresAuth: true, title: "Help Desk" } },
      { path: "manage-notifications", name: "manage_notifications", component: () => import("modules/notification/pages/manageNotification.vue"), meta: { requiresAuth: true, title: "Manage Notifications" } },
      { path: "settings", name: "settings", component: () => import("modules/settings/pages/index.vue"), meta: { requiresAuth: true, title: "Settings" } }
    ]
  }
];
