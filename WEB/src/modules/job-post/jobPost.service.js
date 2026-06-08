import { http } from "boot/axios";

export default {
  getAllJobPosts (model) {
    return http.post("/jobpost/list", model).then(response => response.data);
  },

  getJobPost (id) {
    return http.get(`/jobpost/${id}`).then(response => response.data);
  },

  updateJobStatus (id, statusId) {
    return http.put(`/jobpost/${id}/${statusId}`).then(response => response.data);
  },

  saveJobPost (id, model) {
    if (id) {
      return http.put(`/jobpost/${id}`, model).then(response => response.data);
    } else {
      return http.post("/jobpost", model).then(response => response.data);
    }
  },

  deleteJobPost (id) {
    return http.delete(`/jobpost/${id}`).then(response => response.data);
  },

  getAllJobPostListForDropdown () {
    return http.get("/jobpost/dropdown/list").then(response => response.data);
  },

  getAllJobPostListForVskyWebsite (siteId) {
    return http.get(`/jobpost/vskyWebsiteDropdown/list?siteId=${siteId}`).then(response => response.data);
  }
};
