import { http } from "boot/axios";

export default {
  getNotifications (model) {
    return http.post("/notification/list", model).then(response => response.data);
  },

  getNotificationList (Id, flag) {
    return http.post(`notification/?Id=${Id}&&Flag=${flag}`).then(response => response.data);
  },

  getNotificationCount () {
    return http.get("/notification/notificationcount").then(response => response.data);
  },

  // permissions data
  getAllNotificationPermissions (model) {
    return http.post("/notification/notificationPermissionList", model).then(response => response.data);
  },
  updateNotificationPerssion (id, active, userId) {
    return http.put(`/notification/on-off-status/${id}/${active}/${userId}`).then(response => response.data);
  },
  updateAllPermissions (active, userId) {
    return http.put(`/notification/all-permissions/${active}/${userId}`).then(response => response.data);
  },

  // Site Email Notifications
  getAllSitesEmailNotificationsPermissions (model) {
    return http.post("/email-Notification/emailNotificationsPermissionsList", model).then(response => response.data);
  },
  updateEmailNotificationPermission (id, active, userId) {
    return http.put(`/email-Notification/on-off-status/${id}/${active}/${userId}`).then(response => response.data);
  },
  updateAllEmailNotificationPermissions (active, userId) {
    return http.put(`/email-Notification/all-permissions/${active}/${userId}`).then(response => response.data);
  },
  getEmailPreview (id) {
    return http.get(`/email-Notification/emailPreview/${id}`).then(response => response.data);
  }
};
