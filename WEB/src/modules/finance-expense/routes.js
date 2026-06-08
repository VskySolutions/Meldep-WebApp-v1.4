export default [
  {
    path: "/finance-expense",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "finance-expense", component: () => import("modules/finance-expense/pages/index.vue"), meta: { requiresAuth: true, title: "Expenses" } },
      { path: "/approve-expense", name: "approve-expenses", component: () => import("modules/finance-expense/pages/approveExpenseList.vue"), meta: { requiresAuth: true, title: "Approve Expenses" } }
    ]
  }
];
