export default [
  {
    path: "/dropdown",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "dropdown", component: () => import("modules/dropdown/pages/index.vue"), meta: { requiresAuth: true, title: "DropDowns" } },
      { path: "/dropdowntypes", name: "dropdowntypes", component: () => import("modules/dropdown/pages/typeList.vue"), meta: { requiresAuth: true, title: "DropDownTypes" } },
      { path: "/manage-dropdowns", name: "manage-dropdowns", component: () => import("modules/dropdown/pages/manageDropdowns.vue"), meta: { requiresAuth: true, title: "Manage-Dropdowns" } }
    ]
  }
];
