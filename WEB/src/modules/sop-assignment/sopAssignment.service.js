import { http } from "boot/axios";

export default {
  getAllSOPAssignmentList (model) {
    return http.post("/sop-assignment/list", model).then(response => response.data);
  },

  getSOPAssignmentById (id) {
    return http.get(`/sop-assignment/${id}`).then(response => response.data);
  },

  getSOPAssignmentByIdInDetail (id) {
    return http.get(`/sop-assignment/details/${id}`).then(response => response.data);
  },

  createUpdateSOPAssignment (id, model) {
    if (id) {
      return http.put(`/sop-assignment/${id}`, model).then(response => response.data);
    } else {
      return http.post("/sop-assignment", model).then(response => response.data);
    }
  },

  saveSOPAssignmentResponses (model) {
    return http.post("/sop-assignment/save-assignment-responce", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },

  submitReview (model) {
    return http.post("/sop-assignment/save-review", model).then(response => response.data);
  },

  deleteSOPAssignment (id) {
    return http.delete(`/sop-assignment/${id}`).then(response => response.data);
  }
};
