export default [
  {
    path: "/sop-process",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "sopProcess", component: () => import("modules/sop-process/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/sop-process"], level: "view" }, title: "SOP Process List" } }
    ]
  }
];
