export default [
  {
    path: "/dropdown",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "dropdown", component: () => import("modules/dropdown/pages/index.vue"), meta: { requiresAuth: true, title: "DropDowns" } }
    ]
  }
];
