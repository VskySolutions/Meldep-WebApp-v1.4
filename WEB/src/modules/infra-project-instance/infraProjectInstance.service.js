import { http } from "boot/axios";

export default {
  getAllInfraProjectInstanceForList (model) {
    return http.post("/infra-project-instance/list", model).then(response => response.data);
  },

  getAllInfraProjectInstanceListForDropdown (projectId) {
    return http.get(`/infra-project-instance/dropdown/list?projectId=${projectId}`).then(response => response.data);
  },

  getInfraProjectInstanceInDetailById (id) {
    return http.get(`/infra-project-instance/details/${id}`).then(response => response.data);
  },

  getInfraProjectInstanceRoleInDetailByInstanceId (instanceId) {
    return http.get(`/infra-project-instance/role-details/${instanceId}`).then(response => response.data);
  },

  addEditInfraProjectInstance (model) {
    return http.post("/infra-project-instance", model).then(response => response.data);
  },

  saveInfraProjectInstanceRoles (model) {
    return http.post("/infra-project-instance/save-project-instance-roles", model).then(response => response.data);
  },

  addOrUpdateInstructions (id, model) {
    return http.put(`/infra-project-instance/instructions/${id}`, model).then(response => response.data);
  },

  deleteInfraProjectInstance (id) {
    return http.delete(`/infra-project-instance/${id}`).then(response => response.data);
  }
};
