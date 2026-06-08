export default [
  {
    path: "/requirement-group",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "requirement-group", component: () => import("modules/requirement-group/pages/index.vue"), meta: { requiresAuth: true, title: "Requirement Group" } }
    ]
  }
];
