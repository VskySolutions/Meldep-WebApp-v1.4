import { http } from "boot/axios";

export default {
  getAllRequirementStatusTracking (model) {
    return http.post("/requirement-status-tracking/list", model).then(response => response.data);
  },

  getRequirementStatusTrackingById (recordId) {
    return http.get(`/requirement-status-tracking/${recordId}`).then(response => response.data);
  },

  // Whole batch in one request; there is intentionally no per-row save.
  bulkAddRequirementStatusTracking (model) {
    return http.post("/requirement-status-tracking/bulk-add", model).then(response => response.data);
  },

  bulkSaveRequirementStatusTracking (model) {
    return http.post("/requirement-status-tracking/bulk-save", model).then(response => response.data);
  },

  deleteRequirementStatusTracking (recordId) {
    return http.delete(`/requirement-status-tracking/${recordId}`).then(response => response.data);
  },

  bulkDeleteRequirementStatusTracking (recordIds) {
    return http.post("/requirement-status-tracking/bulk-delete", { recordIds: [...new Set(recordIds)] }).then(response => response.data);
  },

  bulkEditRequirementStatusTracking (recordIds, fields) {
    return http.post("/requirement-status-tracking/bulk-edit", { recordIds: [...new Set(recordIds)], fields }).then(response => response.data);
  }
};
