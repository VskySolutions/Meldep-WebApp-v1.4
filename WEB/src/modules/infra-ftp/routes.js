export default [
  {
    path: "/infra-ftp",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "infra-ftp", component: () => import("modules/infra-ftp/pages/index.vue"), meta: { requiresAuth: true, title: "Infra FTP" } }
    ]
  }
];
