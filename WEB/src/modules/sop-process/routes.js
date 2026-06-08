export default [
  {
    path: "/sop-process",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "sopProcess", component: () => import("modules/sop-process/pages/index.vue"), meta: { requiresAuth: true, title: "SOP Process List" } }
    ]
  }
];
