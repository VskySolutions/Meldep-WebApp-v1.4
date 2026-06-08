export default [
  {
    path: "/customer-files",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "customer-files", component: () => import("modules/customer-files/pages/index.vue"), meta: { requiresAuth: true, title: "Customer Files" } }
    ]
  }
];
