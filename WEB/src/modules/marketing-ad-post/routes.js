export default [
  {
    path: "/marketing-ad-post",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "marketing-ad-post", component: () => import("modules/marketing-ad-post/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/marketing-ad-post"], level: "view" }, title: "Ad" } }
    ]
  }
];
