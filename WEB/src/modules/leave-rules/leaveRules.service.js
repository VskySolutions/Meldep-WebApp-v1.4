import { http } from "boot/axios";

export default {
  getLeaveRules (model) {
    return http.post("/leaveRules/list", model).then(response => response.data);
  },

  getLeaveRule (id) {
    return http.get(`/leaveRules/${id}`).then(response => response.data);
  },

  saveLeaveRules (id, model) {
    if (id) {
      return http.put(`/leaveRules/${id}`, model).then(response => response.data);
    } else {
      return http.post("/leaveRules", model).then(response => response.data);
    }
  },

  generateLeaveRule (id) {
    return http.post(`/leaveRules/generate/${id}`).then(response => response.data);
  },

  deleteLeaveRule (id) {
    return http.delete(`/leaveRules/${id}`).then(response => response.data);
  }

};
