import { http } from "boot/axios";

export default {
  getAllItemSubCategoryAttributeList () {
    return http.get("/item-subcategory-attributes/item-subcategory-attribute-list").then(response => response.data);
  },
  getAllItemSubcategoryAttributeValuesByAttributeId (itemSubcategoryAttributeId) {
    return http.get(`/item-subcategory-attributes/item-subcategory-attribute-value-list/${itemSubcategoryAttributeId}`).then(response => response.data);
  },
  getItemSubcategoryAttributeDetailsById  (id) {
    return http.get(`/item-subcategory-attributes/${id}`).then(response => response.data);
  },
  getItemSubcategoryAttributeValueDetailsById  (id) {
    return http.get(`/item-subcategory-attributes/item-subcategory-attribute-value/${id}`).then(response => response.data);
  },
  deleteItemSubCategoryAttribute (id) {
    return http.delete(`/item-subcategory-attributes/deleteItemSubCategoryAttribute/${id}`).then(response => response.data);
  },
  deleteItemSubCategoryAttributeValue (id) {
    return http.delete(`/item-subcategory-attributes/deleteItemSubCategoryAttributeValue/${id}`).then(response => response.data);
  },
  checkItemSubcategoryAttributeCanBeDeleted (id) {
    return http.get(`/item-subcategory-attributes/checkItemSubcategoryAttributeCanBeDeleted/${id}`).then(response => response.data);
  },
  saveItemSubCategoryAttribute (id, model) {
    if (id) {
      return http.put(`/item-subcategory-attributes/updateItemSubCategoryAttribute/${id}`, model).then(response => response.data);
    } else {
      return http.post("/item-subcategory-attributes/createItemSubCategoryAttribute", model).then(response => response.data);
    }
  },
  saveItemSubCategoryAttributeValue (id, model) {
    if (id) {
      return http.put(`/item-subcategory-attributes/updateItemSubCategoryAttributeValue/${id}`, model).then(response => response.data);
    } else {
      return http.post("/item-subcategory-attributes/createItemSubCategoryAttributeValue", model).then(response => response.data);
    }
  },
  // sites item subCategory attributes
  getAllSitesItemSubCategoryAttributesListByItemSubCategoryId (itemSubCategoryId) {
    return http.get(`/sites_item_subcategory_attributes_mapping/sites_item_subcategory_attributes-list?itemSubCategoryId=${itemSubCategoryId}`).then(response => response.data);
  },
  getItemAttributeListNotInMappingAsync (itemSubcategoryAttributeId) {
    return http.get(`/item-subcategory-attributes/item-subcategory-attribute-list/${itemSubcategoryAttributeId}`).then(response => response.data);
  },
  saveSitesItemSubCategoryAttributesMapping (model) {
    return http.post("/sites_item_subcategory_attributes_mapping", model).then(response => response.data);
  },
  deleteSitesItemSubCategoryAttributesMapping (ids) {
    return http.post(`/sites_item_subcategory_attributes_mapping/delete?ids=${ids}`).then(response => response.data);
  }
};
