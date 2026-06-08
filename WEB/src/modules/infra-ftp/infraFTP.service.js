import { http } from "boot/axios";

export default {
  getAllInfraFTPForList (model) {
    return http.post("/infra-ftp/list", model).then(response => response.data);
  },

  getInfraFTPInDetailById (id) {
    return http.get(`/infra-ftp/details/${id}`).then(response => response.data);
  },

  infraFTPAssignToProjectInstance (id, projectInstanceId) {
    return http.post(`/infra-ftp/assign-to-project-instance?id=${id}&projectInstanceId=${projectInstanceId}`).then(response => response.data);
  },

  addEditInfraFTP (model) {
    return http.post("/infra-ftp", model).then(response => response.data);
  },

  addOrUpdateInstructions (id, model) {
    return http.put(`/infra-ftp/instructions/${id}`, model).then(response => response.data);
  },

  deleteInfraFTP (id) {
    return http.delete(`/infra-ftp/${id}`).then(response => response.data);
  },

  deleteAssignProjectInstance (id) {
    return http.delete(`/infra-ftp/assignProjectInstance/${id}`).then(response => response.data);
  }
};
