export default [
  {
    path: "/item",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "add-categories-subcategories", name: "add-categories-subcategories", component: () => import("modules/items/pages/categoryAndSubCategory.vue"), meta: { requiresAuth: true, title: "Add Categories and Subcategories" } },
      { path: "/item-subcategory/subcategory-and-attribute-mapping", name: "subcategory-attributes-mapping", component: () => import("modules/items/pages/subCategoryAndAttributeMapping.vue"), meta: { requiresAuth: true, title: "Subcategory Attributes Mapping" } },
      { path: "add-attributes-values", name: "add-subcategory-attributes-values", component: () => import("modules/items/pages/attributesAndValues.vue"), meta: { requiresAuth: true, title: "Add Subcategory Attributes and Values" } }
    ]
  }
];
