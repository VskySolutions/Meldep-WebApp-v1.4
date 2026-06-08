import { http } from "boot/axios";

export default {
  getAllSitesItemList (model) {
    return http.post("/sites-items/list", model).then(response => response.data);
  },

  getSitesItemDetailsById (id) {
    return http.get(`/sites-items/${id}`).then(response => response.data);
  },

  saveSitesItem (id, model) {
    if (id) {
      return http.put(`/sites-items/${id}`, model).then(response => response.data);
    } else {
      return http.post("/sites-items", model).then(response => response.data);
    }
  },

  deleteSitesItem (id) {
    return http.delete(`/sites-items/${id}`).then(response => response.data);
  }
};
