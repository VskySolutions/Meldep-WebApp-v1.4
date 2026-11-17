const routes = [
  {
    path: "/auth",
    component: () => import("layouts/auth_layout.vue"),
    children: [
      { path: "", component: () => import("modules/auth/pages/login.vue"), meta: { public: true, title: "Login" } },
      { path: "login", name: "login", component: () => import("modules/auth/pages/login.vue"), meta: { public: true, title: "Login" } },
      { path: "forgot-password", name: "forgot_password", component: () => import("modules/auth/pages/forgot_password.vue"), meta: { public: true, title: "Forgot Password" } },
      { path: "reset-password/:userid", name: "reset_password", component: () => import("modules/auth/pages/reset_password.vue"), meta: { public: true, title: "Reset Password" } }
    ]
  }
];
export default routes;
