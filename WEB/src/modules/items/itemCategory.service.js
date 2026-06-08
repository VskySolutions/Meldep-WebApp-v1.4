import { http } from "boot/axios";

export default {
  getAllItemCategoriesAsync (model) {
    return http.post("/item-category/list", model).then(response => response.data);
  },
  getAllItemCategoryList () {
    return http.get("/item-category/item-category-list").then(response => response.data);
  },
  getAllItemSubcategoryList (itemCategoryId) {
    return http.get(`/item-category/item-subcategory-list?itemCategoryId=${itemCategoryId}`).then(response => response.data);
  },
  generatePrefixForSubcategoryName (subCategoryName, subCategoryId) {
    return http.get(`/item-category/prefix?subCategoryName=${subCategoryName}&subCategoryId=${subCategoryId}`).then(response => response.data);
  },
  getItemCategoryDetailsById  (id) {
    return http.get(`/item-category/${id}`).then(response => response.data);
  },
  getItemSubcategoryDetailsById  (id) {
    return http.get(`/item-category/item-subcategory/${id}`).then(response => response.data);
  },
  deleteItemCategory (id) {
    return http.delete(`/item-category/deleteItemCategory/${id}`).then(response => response.data);
  },
  deleteItemSubcategory (id) {
    return http.delete(`/item-category/deleteItemSubcategory/${id}`).then(response => response.data);
  },
  checkItemCategoryCanBeDeleted (id) {
    return http.get(`/item-category/checkItemCategoryCanBeDeleted/${id}`).then(response => response.data);
  },
  saveBulkItemSubcategories (model) {
    return http.post("/item-category/saveBulkItemSubcategories", model).then(response => response.data);
  },
  saveItemCategory (id, model) {
    if (id) {
      return http.put(`/item-category/updateItemCategory/${id}`, model).then(response => response.data);
    } else {
      return http.post("/item-category/createItemCategory", model).then(response => response.data);
    }
  },
  saveItemSubcategory (id, model) {
    if (id) {
      return http.put(`/item-category/updateItemSubcategory/${id}`, model).then(response => response.data);
    } else {
      return http.post("/item-category/createItemSubcategory", model).then(response => response.data);
    }
  }
};
