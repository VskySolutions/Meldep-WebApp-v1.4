import { http } from "boot/axios";

export default {
  getProjectTasks (model) {
    return http.post("/project-tasks/list", model).then(response => response.data);
  },

  getAllProjectTaskDetailsList (model) {
    return http.post("/project-tasks/taskDetailsList", model).then(response => response.data);
  },
  // getProjectTasks (payload) {
  //   return http.get("/project-tasks", { params: payload }).then(response => response.data);
  // },

  getProjectTask (id) {
    return http.get(`/project-tasks/${id}`).then(response => response.data);
  },

  getProjectTaskDetails (id) {
    return http.get(`/project-tasks/details/${id}`).then(response => response.data);
  },

  getTimesheetByTaskId (id) {
    return http.get(`/project-tasks/task-timesheet/${id}`).then(response => response.data);
  },
  displayWarningForSortOrder (projectId, sortOrder, moduleId, taskId) {
    return http.get(`/project-tasks/warning?projectId=${projectId}&sortOrder=${sortOrder}&moduleId=${moduleId}&taskId=${taskId}`).then(response => response.data);
  },

  checkTaskCanBeDeleted (id) {
    return http.get(`/project-tasks/CheckTaskCanBeDeleted/${id}`).then(response => response.data);
  },

  getAllTaskByProjectId (model) {
    return http.post("/project-tasks/project-calender", model).then(response => response.data);
  },
  getAllProjectTasksForNotes (model) {
    return http.post("/project-tasks/taskListForNotes", model).then(response => response.data);
  },
  // saveProjectTask (id, model) {
  //   if (id) {
  //     return http.put(`/project-tasks/${id}`, model).then(response => response.data);
  //   } else {
  //     return http.post("/project-tasks", model).then(response => response.data);
  //   }
  // },

  saveProjectTask (id, model) {
    const headers = { "Content-Type": "multipart/form-data" }; // Ensure proper handling of file uploads
    if (id) {
      return http.put(`/project-tasks/${id}`, model, { headers }).then(response => response.data);
    } else {
      return http.post("/project-tasks", model, { headers }).then(response => response.data);
    }
  },
  saveTags (model) {
    return http.post("/project-tasks/tags", model).then(response => response.data);
  },

  saveProjectTaskFiles (model) {
    return http.post("/project-tasks/add-task-files", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },

  // updateProjectTaskPriority (id, priorityId) {
  //   return http.put(`/project-tasks/priority/${id}/${priorityId}`).then(response => response.data);
  // },
  updateProjectTaskPriority (model) {
    return http.post("/project-tasks/projectTaskPriority", model).then(response => response.data);
  },
  updateProjectTaskEndDate (id, endDateStr, startDateStr = null) {
    const endDate = endDateStr.replace(/\//g, "-");
    const startDate = startDateStr ? startDateStr.replace(/\//g, "-") : null;
    return http.put(`/project-tasks/end-date/${id}/${endDate}/${startDate}`).then(response => response.data);
  },
  copyModuleToProjects (model) {
    return http.post("/project-tasks/copyModuleToTask", model).then(response => response.data);
  },
  copyTaskToProjects (model) {
    return http.post("/project-tasks/copyTaskToProjects", model).then(response => response.data);
  },
  updateTaskStatus (id, taskId, statusId) {
    return http.put(`/project-tasks/${id}/${taskId}/${statusId}`, statusId).then(response => response.data);
  },

  // updateProjectTaskStatus (id, statusId) {
  //   return http.put(`/project-tasks/${id}/${statusId}`, statusId).then(response => response.data);
  // },
  updateTaskOwner (id, assignedToId) {
    return http.put(`/project-tasks/task-owner/${id}/${assignedToId}`).then(response => response.data);
  },
  updateProjectTaskStatus (model) {
    return http.post("/project-tasks/projectTaskStatus", model).then(response => response.data);
  },
  deleteProjectTask (id) {
    return http.delete(`/project-tasks/${id}`).then(response => response.data);
  },
  deleteFile (id) {
    return http.delete(`/project-tasks/task-file/${id}`).then(response => response.data);
  },
  saveProjectTaskAndactivities (model) {
    return http.post("/project-tasks/task", model).then(response => response.data);
  },
  taskAssignToOwner (id, model) {
    return http.put(`/project-tasks/task-assignment/${id}`, model).then(response => response.data);
  },

  getProjectTaskListForDropdown (projectId, projectModuleId, employeeId) {
    return http.get(`/project-tasks/dropdown/list/${projectId}/${projectModuleId}/${employeeId}`).then(response => response.data);
  },
  getAllProjectMultiTaskListForDropdown (isTemplate = false, projectId, projectModuleId) {
    return http.get(`/project-tasks/dropdown/multiTaskList/${isTemplate}/${projectId}/${projectModuleId}`).then(response => response.data);
  },
  getAllProjectTaskWithProjectListForDropdown () {
    return http.get("/project-tasks/dropdown/task-with-project-list").then(response => response.data);
  },
  getAllTagsListForDropdown () {
    return http.get("/project-tasks/dropdown/tagslist").then(response => response.data);
  },
  getAllProjectTaskTagListForDropdown () {
    return http.get("/project-tasks/projectTaskTags/dropdown/list").then(response => response.data);
  },
  getTasksByProjectModule (projectModuleId, pageName = "", isShowCloseStatus = false) {
    return http.get(`/project-tasks/task/?projectModuleId=${projectModuleId}&pageName=${pageName}&isShowCloseStatus=${isShowCloseStatus}`).then(response => response.data);
  },
  saveBulkTasks (model) {
    return http.post("/project-tasks/saveBulkTasks", model).then(response => response.data);
  },
  updateBulkTasks (model) {
    return http.post("/project-tasks/updateBulkTasks", model).then(response => response.data);
  },

  getProjectTasksDetailsByIds (ids) {
    const idsQuery = ids.join(","); // Join ids into a comma-separated string
    return http.get(`/project-tasks/projectask-detailsbyids?ids=${idsQuery}`).then(response => response.data);
  },
  getAllHighPrioritiesTaskForDashboard (model) {
    return http.post("/project-tasks/taskListForDashboard", model).then(response => response.data);
  },
  getProjectTaskForCopy (model) {
    return http.post("/project-tasks/taskForCopy", model).then(response => response.data);
  },
  updateTaskColor (id, model) {
    return http.post("/project-tasks/update-task-color", model).then(response => response.data);
  },
  addNewTaskThroughDropdown (model) {
    return http.post("/project-tasks/add-new-task-through-dropdown", model).then(response => response.data);
  },
  getTasksAsEvents (model) {
    return http.post("/project-tasks/get-tasks-as-events", model).then(response => response.data);
  },
  getAllTasksByProjectId (model) {
    return http.post("/project-tasks/taskListByProjectId", model).then(response => response.data);
  }
};
