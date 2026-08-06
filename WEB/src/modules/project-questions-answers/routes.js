export default [
  {
    path: "/project-questions-answers",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "project-questions-answers", component: () => import("modules/project-questions-answers/pages/index.vue"), meta: { requiresAuth: true, title: "Project Questions Answers" } }
    ]
  }
];
