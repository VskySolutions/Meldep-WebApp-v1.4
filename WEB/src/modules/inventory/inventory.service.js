import { http } from "boot/axios";

export default {
  getInventorys (model) {
    return http.post("/inventory/list", model).then(response => response.data);
  },

  getAllItemType () {
    return http.get("/inventory/item-types").then(response => response.data);
  },

  getNextInventoryCode (itemTypeId) {
    return http.get(`/inventory/inventory-prefix/${itemTypeId}`).then(response => response.data);
  },

  getInventory (id) {
    return http.get(`/inventory/details/${id}`).then(response => response.data);
  },

  saveInventorys (id, model) {
    if (id) {
      return http.put(`/inventory/${id}`, model).then(response => response.data);
    } else {
      return http.post("/inventory", model).then(response => response.data);
    }
  },

  deleteInventory (id) {
    return http.delete(`/inventory/${id}`).then(response => response.data);
  }
};
