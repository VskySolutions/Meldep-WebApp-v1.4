export default [
  {
    path: "/company-contacts",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "company-contacts", component: () => import("modules/company-contacts/pages/index.vue"), meta: { requiresAuth: true, title: "Company Contact" } }
    ]
  }
];
