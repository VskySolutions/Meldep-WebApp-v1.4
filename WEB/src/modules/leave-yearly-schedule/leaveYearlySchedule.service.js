import { http } from "boot/axios";

export default {
  getLeaveEvents (model) {
    return http.post("/yearly-leave-schedule/list", model).then(response => response.data);
  },

  getEvents (id) {
    return http.get(`/yearly-leave-schedule/${id}/details`).then(response => response.data);
  },

  saveLeaveEvents (id, model) {
    if (id) {
      return http.put(`/yearly-leave-schedule/${id}`, model).then(response => response.data);
    } else {
      return http.post("/yearly-leave-schedule", model).then(response => response.data);
    }
  },

  saveLeaveSaturdayEvents (id, model) {
    if (id) {
      return http.put(`/yearly-leave-schedule/yearly-saturdayoff/${id}`, model).then(response => response.data);
    } else {
      return http.post("/yearly-leave-schedule/yearly-saturdayoff", model).then(response => response.data);
    }
  },

  deleteEvent (id) {
    return http.delete(`/yearly-leave-schedule/${id}`).then(response => response.data);
  },

  deleteSaturdayOffEvent (id) {
    return http.delete(`/yearly-leave-schedule/delete-saturdayoff/${id}`).then(response => response.data);
  },

  getYearlyLeaveListForDashboard () {
    return http.post("/yearly-leave-schedule/yealyLeaves/list").then(response => response.data);
  }
};
