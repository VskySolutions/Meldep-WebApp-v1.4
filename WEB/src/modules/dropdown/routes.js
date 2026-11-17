export default [
  {
    path: "/dropdown",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "dropdown", component: () => import("modules/dropdown/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/dropdown", "/dropdowntypes"], level: "view" }, title: "DropDowns" } },
      { path: "/dropdowntypes", name: "dropdowntypes", component: () => import("modules/dropdown/pages/typeList.vue"), meta: { requiresAuth: true, permission: { menu: ["/dropdown", "/dropdowntypes"], level: "view" }, title: "DropDownTypes" } },
      { path: "/manage-dropdowns", name: "manage-dropdowns", component: () => import("modules/dropdown/pages/manageDropdowns.vue"), meta: { requiresAuth: true, permission: { menu: ["/dropdown", "/dropdowntypes"], level: "view" }, title: "Manage-Dropdowns" } }
    ]
  }
];
