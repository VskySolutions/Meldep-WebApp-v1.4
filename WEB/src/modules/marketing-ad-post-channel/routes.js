export default [
  {
    path: "/marketing-ad-post-channel",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "marketing-ad-post-channel", component: () => import("modules/marketing-ad-post-channel/pages/index.vue"), meta: { requiresAuth: true, title: "Ad Post Channel" } }
    ]
  }
];
