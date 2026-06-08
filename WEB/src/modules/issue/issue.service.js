import { http } from "boot/axios";

export default {
  getAllIssue (model) {
    return http.post("/issue/list", model).then(response => response.data);
  },

  getIssue (id) {
    return http.get(`/issue/${id}`).then(response => response.data);
  },

  getIssueDetails (id) {
    return http.get(`/issue/details/${id}`).then(response => response.data);
  },

  saveIssue (id, model) {
    if (id) {
      return http.put(`/issue/${id}`, model).then(response => response.data);
    } else {
      return http.post("/issue", model).then(response => response.data);
    }
  },
  updateIssueStatus (model) {
    return http.post("/issue/updateIssueStatus", model).then(response => response.data);
  },
  updateIssuePriority (model) {
    return http.post("/issue/updateIssuePriority", model).then(response => response.data);
  },
  deleteIssue (id) {
    return http.delete(`/issue/${id}`).then(response => response.data);
  },

  getActivity (id) {
    return http.get(`/issue/${id}/issueActivity`).then(response => response.data);
  },

  saveIssueActivity (model) {
    return http.post("/issue/issueActivity", model).then(response => response.data);
  },

  deleteActivity (Activityid) {
    return http.delete(`/issue/${Activityid}/issueActivity`).then(response => response.data);
  }
  // getAllTestPlanListForDropdown (projectId) {
  //   return http.get(`/test-plan/dropdown/list?ProjectId=${projectId}`).then(response => response.data);
  // }
};
