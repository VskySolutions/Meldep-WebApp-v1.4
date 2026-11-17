export default [
  {
    path: "/finance-expense-purchase-request",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "finance-expense-purchase-request", component: () => import("modules/finance-expense-purchase-request/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/finance-expense-purchase-request", "/approve-purchase-expense"], level: "view" }, title: "Purchase Expenses" } },
      { path: "/approve-purchase-expense", name: "approve-purchase-expense", component: () => import("modules/finance-expense-purchase-request/pages/approvePurchaseExpenseList.vue"), meta: { requiresAuth: true, permission: { menu: ["/finance-expense-purchase-request", "/approve-purchase-expense"], level: "view" }, title: "Approve Purchase Expenses" } }
    ]
  }
];
