export default [
  {
    path: "/finance-bank-account",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "finance-bank-account", component: () => import("modules/finance-bank-account/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/finance-bank-account"], level: "view" }, title: "Bank Account" } }
    ]
  }
];
