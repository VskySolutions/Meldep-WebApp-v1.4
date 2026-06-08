export default [
  {
    path: "/help-desk",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "help-desk", component: () => import("modules/helpdesk/pages/index.vue"), meta: { requiresAuth: true, title: "Help Desk" } },
      { path: "/help-desk/list", name: "help-desk-list", component: () => import("modules/helpdesk/pages/cardView.vue"), meta: { requiresAuth: true, title: "Help Desk List" } },
      { path: "topics-questions/list", name: "help-desk-topics-questions-list", component: () => import("modules/helpdesk/pages/topicAndQuestions.vue"), meta: { requiresAuth: true, title: "Help Desk Topics Questions List" } }
    ]
  }
];
