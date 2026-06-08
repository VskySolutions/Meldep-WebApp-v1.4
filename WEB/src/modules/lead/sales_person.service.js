import { http } from "boot/axios";

export default {
  getEmployees (model) {
    return http.post("/salesperson/list", model).then(response => response.data);
  },

  getEmployee (id) {
    return http.get(`/salesperson/${id}`).then(response => response.data);
  },

  saveEmployee (id, model) {
    if (id) {
      return http.put(`/employees/${id}`, model).then(response => response.data);
    } else {
      return http.post("/employees", model).then(response => response.data);
    }
  },

  deleteEmployee (id) {
    return http.delete(`/employees/${id}`).then(response => response.data);
  },

  getAllEmployeesListForDropdown () {
    return http.get("/employees/dropdown/list").then(response => response.data);
  },

  saveEmployeeType (model) {
    return http.post("/employees/saveemployeetype", model).then(response => response.data);
  }
};
