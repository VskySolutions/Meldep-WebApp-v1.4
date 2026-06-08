import { http } from "boot/axios";

export default {
  getAllInfraAccount (model) {
    return http.post("/infra-account/list", model).then(response => response.data);
  },

  getInfraAccountListForDropdown () {
    return http.get("/infra-account/dropdown/list").then(response => response.data);
  },

  getInfraAccountDetails (id) {
    return http.get(`/infra-account/details/${id}`).then(response => response.data);
  },

  checkAccountCanBeDeleted (accountId) {
    return http.get(`/infra-account/checkAccountCanBeDeleted/${accountId}`).then(response => response.data);
  },

  saveInfraAccount (id, model) {
    if (id) {
      return http.put(`/infra-account/${id}`, model).then(response => response.data);
    } else {
      return http.post("/infra-account", model).then(response => response.data);
    }
  },

  deleteInfraAccount (id) {
    return http.delete(`/infra-account/${id}`).then(response => response.data);
  }
};
