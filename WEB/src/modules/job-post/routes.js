export default [
  {
    path: "/job-post",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "job-post", component: () => import("modules/job-post/pages/index.vue"), meta: { requiresAuth: true, title: "Job Post" } }
    ]
  }
];
