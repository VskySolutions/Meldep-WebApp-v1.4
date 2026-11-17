export default [
  {
    path: "/customer",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "customer", component: () => import("modules/customer/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/customer"], level: "view" }, title: "Customers" } },
      { path: "customer-center", name: "customer-center", component: () => import("modules/customer/pages/dashboard.vue"), props: true, meta: { requiresAuth: true, permission: { menu: ["/customer"], level: "view" }, title: "Customer Center" } }
    ]
  }
];
