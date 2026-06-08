import { http } from "boot/axios";

export default {
  getAllRequirementGroup (model) {
    return http.post("/requirement-group/list", model).then(response => response.data);
  },

  getRequirementGroup (id) {
    return http.get(`/requirement-group/${id}`).then(response => response.data);
  },

  getRequirementGroupDetails (id) {
    return http.get(`/requirement-group/details/${id}`).then(response => response.data);
  },

  saveRequirementGroup (id, model) {
    if (id) {
      return http.put(`/requirement-group/${id}`, model).then(response => response.data);
    } else {
      return http.post("/requirement-group", model).then(response => response.data);
    }
  },

  deleteRequirementGroup (id) {
    return http.delete(`/requirement-group/${id}`).then(response => response.data);
  },

  getAllRequirementGroupListForDropdown (projectId) {
    return http.get(`/requirement-group/dropdown/list?ProjectId=${projectId}`).then(response => response.data);
  }
};
