import { http } from "boot/axios";

export default {
  getEmployees (model) {
    return http.post("/employees/list", model).then(response => response.data);
  },

  getEmployee (id) {
    return http.get(`/employees/${id}`).then(response => response.data);
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

  getAllEmployeesListForDropdown (siteId) {
    return http.get(`/employees/dropdown/list?siteId=${siteId}`).then(response => response.data);
  },

  getDefaultLeaveApproverNameForDropdown (siteId) {
    return http.get(`/employees/leave-approver/list?siteId=${siteId}`).then(response => response.data);
  },

  getAllActiveEmployeesListForDropdown (siteId) {
    return http.get(`/employees/activedropdown/list?siteId=${siteId}`).then(response => response.data);
  },

  // getAllActivityOwnerListForDropdown () {
  //   return http.get("/employees/ownerdropdown/list").then(response => response.data);
  // },
  getAllActivityOwnerListForDropdown (month) {
    return http.get(`/employees/ownerdropdown/list?TargetMonthStr=${month}`).then(response => response.data);
  },

  getEmployeesByStatus (statusId) {
    return http.get(`/employees/byStatus/list/${statusId}`).then(response => response.data);
  },

  saveEmployeeType (model) {
    return http.post("/employees/saveemployeetype", model).then(response => response.data);
  },

  getAllEmployeesListForDropdownById (id) {
    return http.get(`/employees/dropdown/list/${id}`).then(response => response.data);
  }
};
