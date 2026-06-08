export default [
  {
    path: "/candidate",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "candidate", component: () => import("modules/candidate/pages/index.vue"), meta: { requiresAuth: true, title: "Candidates" } },
      { path: "candidateCenter", name: "candidate-center", component: () => import("modules/candidate/pages/candidateCenter.vue"), props: true, meta: { requiresAuth: true, title: "Candidate Center" } }
    ]
  }
];
