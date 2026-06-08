export default [
  {
    path: "/requirement",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "requirement", component: () => import("modules/requirement/pages/index.vue"), meta: { requiresAuth: true, title: "Requirement" } }
    ]
  }
];
