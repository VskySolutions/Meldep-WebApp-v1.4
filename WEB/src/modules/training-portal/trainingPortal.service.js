import { http } from "boot/axios";

export default {
  getTrainingPortals (model) {
    return http.post("/training-portals/list", model).then(response => response.data);
  },

  getTraining (id) {
    return http.get(`/training-portals/${id}`).then(response => response.data);
  },

  saveTraining (id, model) {
    if (id) {
      return http.put(`/training-portals/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/training-portals", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },

  deleteTraining (id) {
    return http.delete(`/training-portals/${id}`).then(response => response.data);
  }
};
