import { http } from "boot/axios";

export default {
  getAllTags (model) {
    return http.post("/tag-master/list", model).then(response => response.data);
  },
  // updateTags (id, color) {
  //   return http.put(`/tag-master/${id}/${color}`, color).then(response => response.data);
  // }

  getTags (id) {
    return http.get(`/tag-master/${id}`).then(response => response.data);
  },

  updateTags (id, model) {
    return http.put(`/tag-master/${id}`, model).then(response => response.data);
  },

  saveTags (id, model) {
    if (id) {
      return http.put(`/tag-master/tagid/${id}`, model).then(response => response.data);
    } else {
      return http.post("/tag-master", model).then(response => response.data);
    }
  },

  deleteTags (id) {
    return http.delete(`/tag-master/${id}`).then(response => response.data);
  }
};
