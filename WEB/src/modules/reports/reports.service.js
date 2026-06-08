import { http } from "boot/axios";

export default {
  getReports (model) {
    return http.post("/reports/list", model).then(response => response.data);
  },
  getReport (id) {
    return http.get(`/reports/embed-report?reportId=${id}`).then(response => response.data);
  },
  getAllReport () {
    return http.get("/reports/embed-all-reports").then(response => response.data);
  },
  getReportById (id) {
    return http.get(`/reports/${id}`).then(response => response.data);
  },
  // saveReport (id, model) {
  //   return http.put(`/reports/${id}`, model).then(response => response.data);
  // },
  saveReport (id, model) {
    if (id) {
      return http.put(`/reports/${id}`, model).then(response => response.data);
    } else {
      return http.post("/reports", model).then(response => response.data);
    }
  },
  getAllReportListForDropdown () {
    return http.get("/reports/dropdown/list").then(response => response.data);
  },
  getReportUsers (model) {
    return http.post("/report-users/list", model).then(response => response.data);
  },
  getReportUserByReportSettingsDetailId (reportSettingsDetailId) {
    return http.get(`/report-users/user/${reportSettingsDetailId}`).then(response => response.data);
  },
  saveReportUser (reportSettingsDetailId, model) {
    if (reportSettingsDetailId) {
      return http.put(`/report-users/${reportSettingsDetailId}`, model).then(response => response.data);
    }
  },
  assignBulk (ids, model) {
    if (ids) {
      return http.put(`/report-users/savebulk/${ids}`, model).then(response => response.data);
    }
  },
  getAllReportGroupRoles (model) {
    return http.post("/reports/report-group-roles/list", model).then(response => response.data);
  },
  saveGroupRole (id, model) {
    if (id) {
      return http.put(`/reports/reportgroupsrole/${id}`, model).then(response => response.data);
    } else {
      return http.post("/reports/reportgroupsrole", model).then(response => response.data);
    }
  },
  updateReportGroupsRoleStatus (id) {
    return http.put(`/reports/updateGroupsRoleStatus/${id}`).then(response => response.data);
  },
  deleteGroupRole (id) {
    return http.delete(`/reports/reportgroupsrole/${id}`).then(response => response.data);
  },
  deleteReport (id) {
    return http.delete(`/reports/${id}`).then(response => response.data);
  }
};
