export default [
  {
    path: "/lead",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "lead", component: () => import("modules/lead/pages/index.vue"), meta: { requiresAuth: true, title: "Leads" } },
      { path: "/salesperson", name: "salesperson", component: () => import("modules/lead/pages/salesPerson.vue"), meta: { requiresAuth: true, title: "Sales Person" } },
      { path: "/lead-group-user-assignment/list", name: "leadGroupUserAssignmentList", component: () => import("modules/lead/pages/leadGroupUserAssignmentList.vue"), meta: { requiresAuth: true, title: "Lead Group User Assignment" } }
    ]
  }
];
