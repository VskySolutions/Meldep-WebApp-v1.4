export default [
  {
    path: "/person",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "person", component: () => import("modules/person/pages/index.vue"), meta: { requiresAuth: true, title: "Person" } }
    ]
  }
];
