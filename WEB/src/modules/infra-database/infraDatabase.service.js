import { http } from "boot/axios";

export default {
  getAllInfraDatabaseForList (model) {
    return http.post("/infra-database/list", model).then(response => response.data);
  },

  getInfraDatabaseInDetailById (id) {
    return http.get(`/infra-database/details/${id}`).then(response => response.data);
  },

  infraDatabaseAssignToProjectInstance (id, projectInstanceId) {
    return http.post(`/infra-database/assign-to-project-instance?id=${id}&projectInstanceId=${projectInstanceId}`).then(response => response.data);
  },

  addEditInfraDatabase (model) {
    return http.post("/infra-database", model).then(response => response.data);
  },

  addOrUpdateInstructions (id, model) {
    return http.put(`/infra-database/instructions/${id}`, model).then(response => response.data);
  },

  deleteInfraDatabase (id) {
    return http.delete(`/infra-database/${id}`).then(response => response.data);
  },

  deleteAssignProjectInstance (id) {
    return http.delete(`/infra-database/assignProjectInstance/${id}`).then(response => response.data);
  }
};
