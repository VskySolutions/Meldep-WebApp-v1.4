import { http } from "boot/axios";

export default {
  getDepartments (model) {
    return http.post("/departments/list", model).then(response => response.data);
  },

  getDepartment (id) {
    return http.get(`/departments/${id}`).then(response => response.data);
  },

  saveDepartment (id, model) {
    if (id) {
      return http.put(`/departments/${id}`, model).then(response => response.data);
    } else {
      return http.post("/departments", model).then(response => response.data);
    }
  },

  deleteDepartment (id) {
    return http.delete(`/departments/${id}`).then(response => response.data);
  },

  getAllDepartmentListForDropdown () {
    return http.get("/departments/dropdown/list").then(response => response.data);
  }
};
