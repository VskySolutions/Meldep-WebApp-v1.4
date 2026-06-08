import { http } from "boot/axios";

export default {
  getAllAdPostChannel (model) {
    return http.post("/ad-post-channel/list", model).then(response => response.data);
  },

  getChannelNumber () {
    return http.get("/ad-post-channel/number").then(response => response.data);
  },

  getAdPostChannel (id) {
    return http.get(`/ad-post-channel/${id}`).then(response => response.data);
  },

  getAdPostChannelDetails (id) {
    return http.get(`/ad-post-channel/details/${id}`).then(response => response.data);
  },

  saveAdPostChannel (id, model) {
    if (id) {
      return http.put(`/ad-post-channel/${id}`, model).then(response => response.data);
    } else {
      return http.post("/ad-post-channel", model).then(response => response.data);
    }
  },

  deleteAdPostChannel (id) {
    return http.delete(`/ad-post-channel/${id}`).then(response => response.data);
  },

  getAllAdPostChannelListForDropdown () {
    return http.get("/ad-post-channel/dropdown/list").then(response => response.data);
  }
};
