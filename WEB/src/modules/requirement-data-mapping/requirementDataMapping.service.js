import { http } from "boot/axios";

export default {

  getRequirementDataMapping (requirementId) {
    return http.get(`/requirement-data-mappings/get-data-mappings/${requirementId}`).then(response => response.data);
  },

  getRequirementDataMapping (requirementId) {
    return http.get(`/requirement-data-mappings/get-data-mappings/${requirementId}`).then(response => response.data);
  },

  getAllRequirementDataMappingNotes (requirementDataMappingId) {
    return http.get(`/requirement-data-mappings/get-data-mapping-notes/${requirementDataMappingId}`).then(response => response.data);
  },

  saveRequirementDataMapping (model) {
    return http.post("/requirement-data-mappings/save-data-mappings", model).then(response => response.data);
  },

  saveNote (model) {
    return http.post("/requirement-data-mappings/save-note", model).then(response => response.data);
  },

  deleteDataMapping (id) {
    return http.delete(`/requirement-data-mappings/${id}`).then(response => response.data);
  },

  deleteNote (id) {
    return http.delete(`/requirement-data-mappings/delete-note/?id=${id}`).then(response => response.data);
  },
  
  deleteDataMapping (id) {
    return http.delete(`/requirement-data-mappings/${id}`).then(response => response.data);
  }
};
