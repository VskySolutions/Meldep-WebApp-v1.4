import { http } from "boot/axios";

const data = response => response.data;

export default {
  getOverview: () => http.get("/security-admin/overview").then(data),

  getUsers: params => http.get("/security-admin/users", { params }).then(data),
  getUserSites: userId => http.get(`/security-admin/users/${userId}/sites`).then(data),
  getUserRoles: userId => http.get(`/security-admin/users/${userId}/roles`).then(data),
  setUserRoles: (userId, siteRoleIds) => http.put(`/security-admin/users/${userId}/roles`, { siteRoleIds }).then(data),

  getRoles: () => http.get("/security-admin/roles").then(data),

  getGroups: () => http.get("/security-admin/groups").then(data),
  createGroup: model => http.post("/security-admin/groups", model).then(data),
  updateGroup: (id, model) => http.put(`/security-admin/groups/${id}`, model).then(data),
  deleteGroup: id => http.delete(`/security-admin/groups/${id}`).then(data),
  getGroupMembers: id => http.get(`/security-admin/groups/${id}/members`).then(data),
  addGroupMembers: (id, userIds) => http.post(`/security-admin/groups/${id}/members`, { userIds }).then(data),
  removeGroupMember: (id, userId) => http.delete(`/security-admin/groups/${id}/members/${userId}`).then(data),

  getGroupPermissions: id => http.get(`/security-admin/groups/${id}/permissions`).then(data),
  setGroupPermission: (id, model) => http.put(`/security-admin/groups/${id}/permissions`, model).then(data),
  getUserPermissions: id => http.get(`/security-admin/users/${id}/permissions`).then(data),
  setUserPermission: (id, model) => http.put(`/security-admin/users/${id}/permissions`, model).then(data),

  getEffective: userId => http.get(`/security-admin/effective/${userId}`).then(data),

  getAudit: params => http.get("/security-admin/audit", { params }).then(data)
};

// The log stores UTC without a marker, so it's added before converting to the viewer's time.
export function formatUtc (value) {
  if (!value) return "";
  // The API's standard format, such as "11/05/2026 05:22 PM", is already in the site's time zone.
  if (/^\d{2}\/\d{2}\/\d{4}/.test(value)) return value;
  const text = /(Z|[+-]\d\d:\d\d)$/.test(value) ? value : `${value}Z`;
  return new Date(text).toLocaleString();
}
