import { http } from "boot/axios";

export default {
  getRoles () {
    return http.get("/roles").then(response => response.data);
  },

  getSiteRoles (model) {
    return http.post("/roles/list", model).then(response => response.data);
  },

  getMasterRole (id) {
    return http.get(`/roles/${id}`).then(response => response.data);
  },

  getUserCountBySiteRole (id) {
    return http.get(`/roles/user-count/${id}`).then(response => response.data);
  },

  saveMasterRole (id, model) {
    if (id) {
      return http.put(`/roles/${id}`, model).then(response => response.data);
    } else {
      return http.post("/roles", model).then(response => response.data);
    }
  },

  deleteMasterRole (id) {
    return http.delete(`/roles/${id}`).then(response => response.data);
  }
};
