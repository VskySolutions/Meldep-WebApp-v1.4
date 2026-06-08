import { http } from "boot/axios";

export default {
  getLeads (model) {
    return http.post("/leads/list", model).then(response => response.data);
  },

  getLead (id) {
    return http.get(`/leads/${id}`).then(response => response.data);
  },

  getLeadActivity (id) {
    return http.get(`/leads/${id}/leadActivity`).then(response => response.data);
  },

  getActivityLog (id) {
    return http.get(`/leads/${id}/Activitylog`).then(response => response.data);
  },

  saveLead (id, model) {
    if (id) {
      return http.put(`/leads/${id}`, model).then(response => response.data);
    } else {
      return http.post("/leads", model).then(response => response.data);
    }
  },

  saveLeadActivityLogs (model) {
    return http.post("/leads/leadActivityLogs", model).then(response => response.data);
  },

  updateLeadActivityLogs (id, model) {
    return http.put(`/leads/${id}/leadactivity`, model).then(response => response.data);
  },

  deleteLead (id) {
    return http.delete(`/leads/${id}`).then(response => response.data);
  },

  deleteLeadActivity (Activityid) {
    return http.delete(`/leads/${Activityid}/leadactivity`).then(response => response.data);
  },
  getLeadDetails (id) {
    return http.get(`/leads/${id}/leaddetails`).then(response => response.data);
  },

  getLeadStages () {
    return http.get("/leads/leadstages-list").then(response => response.data);
  },

  getAllLeadActivityListForDropdown () {
    return http.get("/leads/dropdown/list").then(response => response.data);
  },

  getLeadActivities () {
    return http.get("/leads/leadactivities-list").then(response => response.data);
  },

  getLeadListForDropdwon () {
    return http.get("/leads/lead-dropdown").then(response => response.data);
  },

  // Lead Group
  getUsersWithAssignedLeadGroups (model) {
    return http.post("/lead-groups/lead-group-users/list", model).then(response => response.data);
  },

  assignUserToLeadGroup (id, model) {
    if (id) {
      return http.put(`/lead-groups/assign-user-to-lead-group/${id}`, model).then(response => response.data);
    } else {
      return http.post("/lead-groups/assign-user-to-lead-group", model).then(response => response.data);
    }
  },

  updateLeadGroupsUserStatus (id) {
    return http.put(`/lead-groups/active-inactive-status/${id}`).then(response => response.data);
  },

  deleteLeadGroupsUser (id) {
    return http.delete(`/lead-groups/${id}`).then(response => response.data);
  },
};
