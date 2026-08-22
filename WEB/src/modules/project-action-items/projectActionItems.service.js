import { http } from "boot/axios";

export default {
  getAllProjectActionItems (model) {
    return http.post("/project-action-items/list", model).then(response => response.data);
  },
  getAllProjectActionItemsForDashboard (model) {
    return http.post("/project-dashboard/projectActionItemsList", model).then(response => response.data);
  },
  getProjectActionItemById (id) {
    return http.get(`/project-action-items/${id}`).then(response => response.data);
  },
  getProjectActionItemDetailsById (id) {
    return http.get(`/project-action-items/details/${id}`).then(response => response.data);
  },
  saveProjectActionItems (model) {
    return http.post("/project-action-items/save-project-action-items", model).then(response => response.data);
  },
  deleteProjectActionItem (id) {
    return http.delete(`/project-action-items/${id}/delete-project-action-items`).then(response => response.data);
  }
};
