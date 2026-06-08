export default [
  {
    path: "/finance-bank-account",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "finance-bank-account", component: () => import("modules/finance-bank-account/pages/index.vue"), meta: { requiresAuth: true, title: "Bank Account" } }
    ]
  }
];
