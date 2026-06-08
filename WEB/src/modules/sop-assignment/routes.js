export default [
  {
    path: "/sop-assignment",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "sopAssignment", component: () => import("modules/sop-assignment/pages/index.vue"), meta: { requiresAuth: true, title: "SOP Assignment List" } },
      { path: "checklist-for-assignment", name: "checklist-for-assignment", component: () => import("modules/sop-assignment/pages/checklistForAssignment.vue"), meta: { requiresAuth: true, title: "Checklist For Assignment" } },
      { path: "checklist-for-approval", name: "checklist-for-approval", component: () => import("modules/sop-assignment/pages/checklistReview.vue"), meta: { requiresAuth: true, title: "Checklist For Approval" } }
    ]
  }
];
