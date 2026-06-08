export default [
  {
    path: "/leave",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/leave-credit", name: "leave-credit", component: () => import("modules/leave/pages/credits.vue"), meta: { requiresAuth: true, title: "Leave Credit" } },
      { path: "/apply-leave", name: "apply-leave", component: () => import("modules/leave/pages/apply.vue"), meta: { requiresAuth: true, title: "Apply Leave" } },
      { path: "/forward-leaves", name: "forward-leaves", component: () => import("modules/leave/pages/forward.vue"), meta: { requiresAuth: true, title: "Forward Leaves" } },
      { path: "/approve-leaves", name: "approve-leaves", component: () => import("modules/leave/pages/approve.vue"), meta: { requiresAuth: true, title: "Approve Leaves" } }
    ]
  }
];
