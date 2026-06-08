import { http } from "boot/axios";

export default {
  getAllAdPost (model) {
    return http.post("/ad-post/list", model).then(response => response.data);
  },

  getAdPostNumber () {
    return http.get("/ad-post/number").then(response => response.data);
  },

  getAdPostDetails (id) {
    return http.get(`/ad-post/details/${id}`).then(response => response.data);
  },

  getAdPostingStatusesByAdId (adId) {
    return http.get(`/ad-post/${adId}`).then(response => response.data);
  },

  saveAdPost (id, model) {
    if (id) {
      return http.put(`/ad-post/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/ad-post", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },

  saveAdPostingStatus (model) {
    return http.post("/ad-post/ad-posting-status", model).then(response => response.data);
  },

  deleteAdPost (id) {
    return http.delete(`/ad-post/${id}`).then(response => response.data);
  }

  // getAllRequirementGroupListForDropdown (projectId) {
  //   return http.get(`/requirement-group/dropdown/list?ProjectId=${projectId}`).then(response => response.data);
  // }
};
