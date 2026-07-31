export default [
  {
    path: "/requirement-center",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "requirement-center", component: () => import("modules/requirement-center/pages/index.vue"), meta: { requiresAuth: true, title: "Requirement Center" } }
    ]
  }
];
