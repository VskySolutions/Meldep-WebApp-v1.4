export default [
  {
    path: "/finance-expense-advance",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "finance-expense-advance", component: () => import("modules/finance-expense-advance/pages/index.vue"), meta: { requiresAuth: true, title: "Advance Expenses" } },
      { path: "/approve-advance-expense", name: "approve-advance-expense", component: () => import("modules/finance-expense-advance/pages/approveAdvanceExpenseList.vue"), meta: { requiresAuth: true, title: "Approve Advance Expenses" } }
    ]
  }
];
