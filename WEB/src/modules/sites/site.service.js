import { http } from "boot/axios";

export default {
  getSites (model) {
    return http.post("/Sites/list", model).then(response => response.data);
  },

  getSubscriptions () {
    return http.get("/subscriptions").then(response => response.data);
  },

  getOrganization (id) {
    return http.get(`/Sites/${id}`).then(response => response.data);
  },

  getRestaurantsList () {
    return http.get("/restaurants/list").then(response => response.data);
  },
  getGlobalSiteData (siteId) {
    return http.get("/Sites/GetGlobalSiteData?siteId=" + siteId).then(response => response.data);
  },
  getDepartmentes () {
    return http.get("/departments").then(response => response.data);
  },

  getOrganizationlogo (logoId) {
    return http.get(`/Sites/logoId/?logoId=${logoId}`).then(response => response.data);
  },

  getModules (siteId) {
    return http.get(`/modules?siteId=${siteId}`).then(response => response.data);
  },

  getAllSitesRoleListForDropdown (siteId) {
    return http.get(`/Sites/dropdown/list/${siteId}`).then(response => response.data);
  },

  getAllSiteModuleListForDropdown (siteId) {
    return http.get(`/Sites/site-module-dropdown/list/${siteId}`).then(response => response.data);
  },

  // getAllSiteModuleMenuListForDropdown (siteId, moduleIds) {
  //   return http.get(`/Sites/site-module-menus-dropdown/list/${siteId}/${moduleIds}`).then(response => response.data);
  // },
  getAllSiteModuleMenuListForDropdown(siteId, moduleIds) {
    return http.get(
      `/Sites/site-module-menus-dropdown/list?siteId=${siteId}&moduleIds=${moduleIds || ""}`
    ).then(response => response.data);
  },

  generateDropdown (id) {
    return http.put(`/Sites/generateDropdown/${id}`).then(response => response.data);
  },
  generateMasterNotifications (siteId) {
    return http.put(`/Sites/generateMasterNotifications/${siteId}`).then(response => response.data);
  },
  getSiteModifiedLogs (id, columnNames) {
    return http.get(`/Sites/?subModuleId=${id}&columnNames=${columnNames}`).then(response => response.data);
  },
  getAllTimeZoneListForDropdown () {
    return http.get("/Sites/timezone/dropdown/list").then(response => response.data);
  },

  addModuleToSite (model) {
    return http.post("/modules/savesitemodules", model).then(response => response.data);
  },

  saveSite (id, model) {
    if (id) {
      return http.put(`/Sites/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/Sites", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },

  deleteOrganization (id) {
    return http.delete(`/Sites/${id}`).then(response => response.data);
  },
};
