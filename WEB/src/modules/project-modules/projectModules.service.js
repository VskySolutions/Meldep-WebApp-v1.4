import { http } from "boot/axios";

export default {
  getProjectModules (model) {
    return http.post("/project-modules/list", model).then(response => response.data);
  },

  getProjectModule (id) {
    return http.get(`/project-modules/${id}`).then(response => response.data);
  },
  checkModuleCanBeDeleted (id) {
    return http.get(`/project-modules/CheckModuleCanBeDeleted/${id}`).then(response => response.data);
  },
  getNextSortOrderOfProjectModuleAndTask (projectId, moduleId) {
    return http.get(`/project-modules/sort-order?projectId=${projectId}&moduleId=${moduleId}`).then(response => response.data);
  },

  getProjectModuleDetails (id) {
    return http.get(`/project-modules/details/${id}`).then(response => response.data);
  },

  // displayWarningForSortOrder (projectId, sortOrder, moduleId) {
  //   return http.get(`/project-modules/warning?projectId=${projectId}&sortOrder=${sortOrder}&moduleId=${moduleId}`).then(response => response.data);
  // },

  // saveProjectModule (id, model) {
  //   if (id) {
  //     return http.put(`/project-modules/${id}`, model).then(response => response.data);
  //   } else {
  //     return http.post("/project-modules", model).then(response => response.data);
  //   }
  // },

  saveProjectModule (id, model) {
    if (id) {
      return http.put(`/project-modules/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/project-modules", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },

  saveProjectModuleFiles (model) {
    return http.post("/project-modules/add-module-files", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },

  updateProjectModuleStatus (id, projectModuleStatusId) {
    return http.put(`/project-modules/${id}/${projectModuleStatusId}`, projectModuleStatusId).then(response => response.data);
  },

  updateModuleColor (id, model) {
    return http.post("/project-modules/update-module-color", model).then(response => response.data);
  },

  moveProjectModuleAsProject (model) {
    return http.post("/projects/move-module-as-project-with-data", model).then(response => response.data);
  },
  deleteProjectModule (id) {
    return http.delete(`/project-modules/${id}`).then(response => response.data);
  },

  deleteFile (id) {
    return http.delete(`/project-modules/module-file/${id}`).then(response => response.data);
  },
  // getAllProjectModuleListForDropdown () {
  //   return http.get("/project-modules/dropdown/list").then(response => response.data);
  // },

  getAllProjectModuleListForDropdown (isTemplate = false, showTaskCount = false, projectId) {
    return http.get(`/project-modules/dropdown/list?isTemplate=${isTemplate}&ProjectId=${projectId}&showTaskCount=${showTaskCount}`).then(response => response.data);
  },

  getAllModuleListByProject (projectId) {
    return http.get(`/project-modules/listbyproject/?projectId=${projectId}`).then(response => response.data);
  },

  // project module user
  getModuleUsersByProjectId (projectId) {
    return http.get(`/project-module-users/user/${projectId}`).then(response => response.data);
  },
  assignBulk (ids, model) {
    if (ids) {
      return http.put(`/project-module-users/savebulk/${ids}`, model).then(response => response.data);
    }
  },
  updateUserProjectModuleAccess (id, model) {
    if (id) {
      return http.put(`/project-module-users/${id}`, model).then(response => response.data);
    }
  },
  deleteProjectModuleUser (id) {
    return http.delete(`/project-module-users/${id}`).then(response => response.data);
  }
};
