export default [
  {
    path: "/finance-expense-advance",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "finance-expense-advance", component: () => import("modules/finance-expense-advance/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/advance-expense-request", "/approve-advance-expense"], level: "view" }, title: "Advance Expenses" } },
      { path: "/approve-advance-expense", name: "approve-advance-expense", component: () => import("modules/finance-expense-advance/pages/approveAdvanceExpenseList.vue"), meta: { requiresAuth: true, permission: { menu: ["/advance-expense-request", "/approve-advance-expense"], level: "view" }, title: "Approve Advance Expenses" } }
    ]
  }
];
