export default [
  {
    path: "/company",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "company", component: () => import("modules/company/pages/index.vue"), meta: { requiresAuth: true, title: "Company" } }
    ]
  }
];
