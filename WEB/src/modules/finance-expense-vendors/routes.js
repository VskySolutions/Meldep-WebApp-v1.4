export default [
  {
    path: "/finance-expense-vendors",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "finance-expense-vendors", component: () => import("modules/finance-expense-vendors/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/finance-expense-vendors"], level: "view" }, title: "Expense Vendor Bank Account" } }
    ]
  }
];
