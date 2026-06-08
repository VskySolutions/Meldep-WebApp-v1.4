import { http } from "boot/axios";

export default {
  getAllProjectActivities (model) {
    return http.post("/project-activities/list", model).then(response => response.data);
  },
  getAllProjectActivitiesForExpandCollapse (model) {
    return http.post("/project-activities/list-expand-collapse", model).then(response => response.data);
  },
  getAllProjectActivityListForDropdown () {
    return http.get("/project-activities/dropdown/list").then(response => response.data);
  },
  getProjectActivity (id) {
    return http.get(`/project-activities/${id}`).then(response => response.data);
  },
  getProjectActivityByTaskId (id, targetMonth) {
    return http.get(`/project-activities/task/${id}?TargetMonthStr=${targetMonth}`).then(response => response.data);
  },
  getProjectActivityDescriptionById (id) {
    return http.get(`/project-activities/description/${id}`).then(response => response.data);
  },
  saveProjectActivity (id, model) {
    if (id) {
      return http.put(`/project-activities/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/project-activities", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },
  saveProjectActivityFiles (model) {
    return http.post("/project-activities/add-activity-files", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },
  updateTaskActivityStatus (model) {
    return http.put("/project-activities/updateTaskActivityStatus", model).then(response => response.data);
  },
  updateActivityActiveStatus (model) {
    return http.put("/project-activities/updateActivityActiveStatus", model).then(response => response.data);
  },
  updateDescription (id, model) {
    return http.put(`/project-activities/description/${id}`, model).then(response => response.data);
  },
  deleteProjectActivity (id) {
    return http.delete(`/project-activities/${id}`).then(response => response.data);
  },
  deleteFile (id) {
    return http.delete(`/project-activities/taskActivity-file/${id}`).then(response => response.data);
  },
  getActivitiesByTask (taskId, pageName = "", isShowCloseStatus = false) {
    return http.get(`/project-activities/task/?taskId=${taskId}&pageName=${pageName}&isShowCloseStatus=${isShowCloseStatus}`).then(response => response.data);
  },
  saveBulkActivities (model) {
    return http.post("/project-activities/saveBulkActivities", model).then(response => response.data);
  },
  updateBulkActivities (model) {
    return http.post("/project-activities/updateBulkActivities", model).then(response => response.data);
  },
  getProjectTaskActivityListForDropdown (projectId, projectModuleId, taskId) {
    return http.get(`/project-activities/dailytimesheetdropdown/list/${projectId}/${projectModuleId}/${taskId}`).then(response => response.data);
  },
  getProjectTasksActivitiesDetailsByIds (ids) {
    const idsQuery = ids.join(",");
    return http.get(`/project-activities/project-activity-detailsbyids?ids=${idsQuery}`).then(response => response.data);
  },
  getActivitiesDetailsByIds (ids, targetMonthStr) {
    const idsQuery = ids.join(",");
    return http.get(`/project-activities/activity-detailsbyids?ids=${idsQuery}&targetMonthStr=${targetMonthStr}`).then(response => response.data);
  },
  cloneProjectTaskActivity (model) {
    return http.post("/project-activities/clone-task-activities", model).then(response => response.data);
  },
  sendTaskTimerToTimesheet (model) {
    return http.post("/Timesheet/send-task-timer-to-timesheet", model).then(response => response.data);
  },
  addProjectActivity (model) {
    return http.post("/project-activities/add-single-activity", model).then(response => response.data);
  }
};
