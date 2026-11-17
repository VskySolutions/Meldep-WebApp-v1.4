export default [
  {
    path: "/lead",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "lead", component: () => import("modules/lead/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/lead"], level: "view" }, title: "Leads" } },
      { path: "/salesperson", name: "salesperson", component: () => import("modules/lead/pages/salesPerson.vue"), meta: { requiresAuth: true, permission: { menu: ["/lead"], level: "view" }, title: "Sales Person" } },
      { path: "/lead-group-user-assignment/list", name: "leadGroupUserAssignmentList", component: () => import("modules/lead/pages/leadGroupUserAssignmentList.vue"), meta: { requiresAuth: true, permission: { menu: ["/lead-group-user-assignment/list", "/lead"], level: "view" }, title: "Lead Group User Assignment" } }
    ]
  }
];
