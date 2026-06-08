import { http } from "boot/axios";

export default {
  // getUsers () {
  //   return http.post("/users/list").then(response => response.data);
  // },
  getUsers (model) {
    return http.post("/users/list", model).then(response => response.data);
  },

  getUser (id, siteId) {
    return http.get(`/users/${id}/${siteId}`).then(response => response.data);
  },

  saveUser (id, model) {
    if (id) {
      return http.put(`/users/${id}`, model).then(response => response.data);
    } else {
      return http.post("/users", model).then(response => response.data);
    }
  },

  getResetPassword (id) {
    return http.get(`/users/${id}/reset-password`).then(response => response.data);
  },

  updateUserStatus (id) {
    return http.put(`/users/updateUserStatus/${id}`).then(response => response.data);
  },

  deleteUser (id) {
    return http.delete(`/users/${id}`).then(response => response.data);
  },

  getAllUserFirstNameListForDropdown () {
    return http.get("/users/dropdown/firstnamelist").then(response => response.data);
  },

  getAllUserLastNameListForDropdown () {
    return http.get("/users/dropdown/lastnamelist").then(response => response.data);
  },

  getAllUserListForDropdown (siteId, flag) {
    return http.get(`/users/dropdown/userlist/${siteId}/${flag}`).then(response => response.data);
  },

  getAllUserListByRoleForDropdown (roleName) {
    return http.get(`/users/dropdown/list?roleName=${roleName}`).then(response => response.data);
  },

  getSupportTeamUsersDataForDropdown (roleName) {
    return http.get(`/users/team-workload-dropdown/list?roleName=${roleName}`).then(response => response.data);
  },

  sendUserLogin (id) {
    return http.post(`/users/${id}/send-user-login`).then(response => response.data);
  }
};
