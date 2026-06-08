export default [
  {
    path: "/sop-template",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "sopTemplate", component: () => import("modules/sop-template/pages/index.vue"), meta: { requiresAuth: true, title: "SOP Template List" } }
    ]
  }
];
