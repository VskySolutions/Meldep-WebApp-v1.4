import { http } from "boot/axios";

export default {
  getAllInfraAccountServicesForList (model) {
    return http.post("/infra-account-services/list", model).then(response => response.data);
  },

  getAllInfraServiceListForDropdown (accountId) {
    return http.get(`/infra-account-services/dropdown/list/${accountId}`).then(response => response.data);
  },

  getInfraAccountServicesInDetailById (id) {
    return http.get(`/infra-account-services/details/${id}`).then(response => response.data);
  },

  checkAccountServiceCanBeDeleted (serviceId) {
    return http.get(`/infra-account-services/checkAccountServiceCanBeDeleted/${serviceId}`).then(response => response.data);
  },

  infraServiceAssignToProject (id, projectId) {
    return http.post(`/infra-account-services/assign-to-project?id=${id}&projectId=${projectId}`).then(response => response.data);
  },

  saveInfraAccountServices (id, model) {
    if (id) {
      return http.put(`/infra-account-services/${id}`, model).then(response => response.data);
    } else {
      return http.post("/infra-account-services", model).then(response => response.data);
    }
  },

  addOrUpdateInstructions (id, model) {
    return http.put(`/infra-account-services/instructions/${id}`, model).then(response => response.data);
  },

  deleteAssignProject (id) {
    return http.delete(`/infra-account-services/assignProject/${id}`).then(response => response.data);
  },

  deleteInfraAccountServices (id) {
    return http.delete(`/infra-account-services/${id}`).then(response => response.data);
  }
};
