import { http } from "boot/axios";

export default {
  getModules (payload) {
    return http.get("/modules", { params: payload }).then(response => response.data);
  },
  getModulesList (payload) {
    return http.get("/modules/moduleslist", { params: payload }).then(response => response.data);
  },
  getModule (id) {
    return http.get(`/modules/${id}`).then(response => response.data);
  },

  saveModule (id, model) {
    if (id) {
      return http.put(`/modules/${id}`, model).then(response => response.data);
    } else {
      return http.post("/modules", model).then(response => response.data);
    }
  },

  deleteModule (id) {
    return http.delete(`/modules/${id}`).then(response => response.data);
  },

  getSiteModule (roleId) {
    return http.get(`/modules/modulesbysite?roleId=${roleId}`).then(response => response.data);
  },

  getModuleMenus (moduleId) {
    return http.get(`/menus/modulemenus?moduleId=${moduleId}`).then(response => response.data);
  },

  getMenu (id) {
    return http.get(`/menus/${id}`).then(response => response.data);
  },

  saveMenu (id, model) {
    if (id) {
      return http.put(`/menus/${id}`, model).then(response => response.data);
    } else {
      return http.post("/menus", model).then(response => response.data);
    }
  },

  deleteMenu (id) {
    return http.delete(`/menus/${id}`).then(response => response.data);
  },
  getMenuManagePermission (menu) {
    return http.get(`/modules/manage-permission/${menu}`).then(response => response.data);
  },
  getSiteModuleMenuPermission (siteId, siteRoleId) {
    return http.get(`/modules/sitesmenusmanage-permission/${siteId}/${siteRoleId}`).then(response => response.data);
  },
  getSiteMenuRolePermissions (model) {
    return http.post("/modules/menu-role-permissions/list", model).then(response => response.data);
  },
  addSitesModuleMenusPermission (model) {
    return http.post("/menus/savesitemenupermission", model).then(response => response.data);
  },
  assignRolesToMenu(model) {
    return http.post("/menus/assignRolesToMenu", model)
      .then(response => response.data);
  },
  getSiteActiveModulesMenus () {
    return http.get("/modules/activemodulesmenusbysite").then(response => response.data);
  },
  getAllModuleMenusForDashboard () {
    return http.get("/menus/dashboard-module-menus").then(response => response.data);
  },

  deleteModuleMenuRoleAccess (siteId, moduleMenuId, roleId) {
    return http.delete(`/menus/delete-role-access/${siteId}/${moduleMenuId}/${roleId}`).then(response => response.data);
  },
};
