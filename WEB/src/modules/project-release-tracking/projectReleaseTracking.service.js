import { http } from "boot/axios";

export default {
  getAllReleaseTracking (model) {
    return http.post("/project-release-tracking/list", model).then(response => response.data);
  },

  // getReleaseTrackingListForDropdown () {
  //   return http.get("/release-tracking/dropdown/list").then(response => response.data);
  // },

  getMappingByReleaseTrackingId (releaseTrackingId) {
    return http.get(`/project-release-tracking/get-mapping-by-releaseTrackingId?releaseTrackingId=${releaseTrackingId}`).then(response => response.data);
  },

  generateVersionNumber (projectId, releaseType) {
    return http.get(`/project-release-tracking/generate-version?projectId=${projectId}&releaseType=${releaseType}`).then(response => response.data);
  },

  getAllReqPlanTaskIssuesByProjectId (projectId) {
    return http.get(`/project-release-tracking/get-all-req-plan-task-issues-by-project/${projectId}`).then(response => response.data);
  },

  getReleaseTrackingInDetailsById (id) {
    return http.get(`/project-release-tracking/details/${id}`).then(response => response.data);
  },

  saveReleaseTracking (id, model) {
    if (id) {
      return http.put(`/project-release-tracking/${id}`, model).then(response => response.data);
    } else {
      return http.post("/project-release-tracking", model).then(response => response.data);
    }
  },

  updateReleaseTrackingStatus (id, statusId) {
    return http.put(`/project-release-tracking/${id}/${statusId}`, statusId).then(response => response.data);
  },

  deleteReleaseTracking (id) {
    return http.delete(`/project-release-tracking/${id}`).then(response => response.data);
  }
};
