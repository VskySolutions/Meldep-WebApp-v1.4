export default [
  {
    path: "/leave",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/leave-credit", name: "leave-credit", component: () => import("modules/leave/pages/credits.vue"), meta: { requiresAuth: true, permission: { menu: ["/leave-credit"], level: "view" }, title: "Leave Credit" } },
      { path: "/apply-leave", name: "apply-leave", component: () => import("modules/leave/pages/apply.vue"), meta: { requiresAuth: true, permission: { menu: ["/apply-leave", "/approve-leaves", "/forward-leaves"], level: "view" }, title: "Apply Leave" } },
      { path: "/forward-leaves", name: "forward-leaves", component: () => import("modules/leave/pages/forward.vue"), meta: { requiresAuth: true, permission: { menu: ["/apply-leave", "/approve-leaves", "/forward-leaves"], level: "view" }, title: "Forward Leaves" } },
      { path: "/approve-leaves", name: "approve-leaves", component: () => import("modules/leave/pages/approve.vue"), meta: { requiresAuth: true, permission: { menu: ["/apply-leave", "/approve-leaves", "/forward-leaves"], level: "view" }, title: "Approve Leaves" } }
    ]
  }
];
