import { http } from "boot/axios";

export default {
  getAllTestPlan (model) {
    return http.post("/test-plan/list", model).then(response => response.data);
  },

  getTestPlan (id) {
    return http.get(`/test-plan/${id}`).then(response => response.data);
  },

  getTestPlanDetails (id) {
    return http.get(`/test-plan/details/${id}`).then(response => response.data);
  },

  saveTestPlan (id, model) {
    if (id) {
      return http.put(`/test-plan/${id}`, model).then(response => response.data);
    } else {
      return http.post("/test-plan", model).then(response => response.data);
    }
  },

  deleteTestPlan (id) {
    return http.delete(`/test-plan/${id}`).then(response => response.data);
  },

  // getAllTestPlanListForDropdown () {
  //   return http.get("/test-plan/dropdown/list").then(response => response.data);
  // }

  getAllTestPlanListForDropdown (projectId) {
    return http.get(`/test-plan/dropdown/list?ProjectId=${projectId}`).then(response => response.data);
  },

  // Excel import (backend generates/parses/validates/persists; see TestPlanController import/*)
  downloadImportTemplate () {
    return http.get("/test-plan/import/template", { responseType: "blob" }).then(response => response.data);
  },

  previewImport (file) {
    const formData = new FormData();
    formData.append("File", file);
    return http.post("/test-plan/import/preview", formData, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },

  confirmImport (rows) {
    return http.post("/test-plan/import/confirm", { rows }).then(response => response.data);
  }

  // getAllModuleListByProject (projectId) {
  //   return http.get(`/project-modules/listbyproject/?projectId=${projectId}`).then(response => response.data);
  // }
};
