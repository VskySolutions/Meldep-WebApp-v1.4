import { http } from "boot/axios";

export default {
  getAllEmployeeOrgStructureList (model) {
    return http.post("/employee-org-structure/list", model).then(response => response.data);
  },

  getEmployeeOrgStructure (id) {
    return http.get(`/employee-org-structure/${id}`).then(response => response.data);
  },

  getEmployeeOrgStructurePreview (year) {
    return http.get(`/employee-org-structure/preview?year=${year}`).then(response => response.data);
  },

  saveEmployeeOrgStructure (id, model) {
    if (id) {
      return http.put(`/employee-org-structure/${id}`, model).then(response => response.data);
    } else {
      return http.post("/employee-org-structure", model).then(response => response.data);
    }
  },

  deleteEmployeeOrgStructure (id) {
    return http.delete(`/employee-org-structure/${id}`).then(response => response.data);
  }
};
