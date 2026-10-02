export default [
  {
    path: "/requirement-data-mapping",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "data-mapping", component: () => import("modules/requirement-data-mapping/pages/index.vue"), meta: { requiresAuth: true, title: "Requirement Data Mapping" } }
    ]
  }
];
