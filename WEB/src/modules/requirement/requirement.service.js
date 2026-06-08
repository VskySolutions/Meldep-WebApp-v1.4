import { http } from "boot/axios";

export default {
  getAllRequirement (model) {
    return http.post("/requirement/list", model).then(response => response.data);
  },

  getRequirement (id) {
    return http.get(`/requirement/${id}`).then(response => response.data);
  },

  getRequirementDetails (id) {
    return http.get(`/requirement/details/${id}`).then(response => response.data);
  },

  getRequirementDescriptionById (id) {
    return http.get(`/requirement/description/${id}`).then(response => response.data);
  },
  getAllRequirementTagListForDropdown () {
    return http.get("/requirement/requirementTags/dropdown/list").then(response => response.data);
  },
  saveRequirement (id, model) {
    if (id) {
      return http.put(`/requirement/${id}`, model).then(response => response.data);
    } else {
      return http.post("/requirement", model).then(response => response.data);
    }
  },
  saveTags (model) {
    return http.post("/requirement/tags", model).then(response => response.data);
  },
  updateRequirementStatus (model) {
    return http.post("/requirement/updateRequirementStatus", model).then(response => response.data);
  },
  updateRequirementPriority (model) {
    return http.post("/requirement/updateRequirementPriority", model).then(response => response.data);
  },
  updateRequirementIsPinned (id, pinstatus) {
    return http.put(`/requirement/pinstatus/${id}/${pinstatus}`).then(response => response.data);
  },
  updateRequirementColor (id, requirementColor) {
    return http.put(`/requirement/requirementColor/${id}/${requirementColor}`).then(response => response.data);
  },
  deleteRequirement (id) {
    return http.delete(`/requirement/${id}`).then(response => response.data);
  }
};
