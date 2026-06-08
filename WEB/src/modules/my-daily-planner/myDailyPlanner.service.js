import { http } from "boot/axios";

export default {
  getDailyPlanners (model) {
    return http.post("/daily-planner/list", model).then(response => response.data);
  },

  getDailyPlannersForDashboard (model) {
    return http.post("/daily-planner/Dashboardlist", model).then(response => response.data);
  },

  getDailyPlanner (id) {
    return http.get(`/daily-planner/${id}`).then(response => response.data);
  },

  getDailyPlannerDetails (id) {
    return http.get(`/daily-planner/${id}/dailyplannerdetails`).then(response => response.data);
  },

  saveDailyPlanner (id, model) {
    if (id) {
      return http.put(`/daily-planner/${id}`, model).then(response => response.data);
    } else {
      return http.post("/daily-planner", model).then(response => response.data);
    }
  },

  deleteDailyplanner (id) {
    return http.delete(`/daily-planner/${id}`).then(response => response.data);
  }
};
