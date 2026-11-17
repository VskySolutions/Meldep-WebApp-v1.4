export default [
  {
    path: "/requirement-group",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "requirement-group", component: () => import("modules/requirement-group/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/requirement-group", "/requirement"], level: "view" }, title: "Requirement Group" } }
    ],
    meta: {
        permission: {
          menu: "/requirement-group",
          level: "View"
      }
    }
  }
];
