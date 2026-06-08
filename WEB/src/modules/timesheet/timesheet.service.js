import { http } from "boot/axios";

export default {
  getTimesheets (model) {
    return http.post("/Timesheet/list", model).then(response => response.data);
  },

  getTimesheetsForDashboard (model) {
    return http.post("/Timesheet/Dashboardlist", model).then(response => response.data);
  },
  getAllTimesheetByWeek (model) {
    return http.post("/Timesheet/timesheetWeeklyList", model).then(response => response.data);
  },
  getTimesheetTotalHoursByWeekAndMonth (model) {
    return http.post("/Timesheet/total-hours-week-and-month", model).then(response => response.data);
  },

  getTimesheetDetails (id) {
    return http.get(`/Timesheet/${id}`).then(response => response.data);
  },

  getTimesheetLinesDetailsByIds (ids) {
    const idsQuery = ids.join(",");
    return http.get(`/Timesheet/timesheet-lines-detailsbyids?ids=${idsQuery}`).then(response => response.data);
  },

  saveTimesheet (id, model) {
    if (id) {
      return http.put(`/Timesheet/${id}`, model).then(response => response.data);
    } else {
      return http.post("/Timesheet", model).then(response => response.data);
    }
  },

  deleteTimesheet (id) {
    return http.delete(`/Timesheet/${id}`).then(response => response.data);
  },
  deleteWeeklyTimesheets (ids) {
    return http.delete('/Timesheet/delete-weekly-timesheets', { data: ids }).then(response => response.data);
  },
  deleteWeeklyTimesheetById (id) {
    return http.delete(`/Timesheet/delete-weekly-timesheet-by-id/${id}`).then(response => response.data);
  },
  exportTimesheet (payload2) {
    return http.post("/Timesheet/export", payload2, { responseType: "blob" }).then(response => response.data);
  },
  exportBillingTimesheet (payload2) {
    return http.post("/Timesheet/exportBillingTimesheet", payload2, { responseType: "blob" }).then(response => response.data);
  },

  // billing processing
  getBillableTimesheets (model) {
    return http.post("/Timesheet/timesheetBillingList", model).then(response => response.data);
  },
  getGroupedBillingTimesheet (model) {
    return http.post("/Timesheet/groupedBillingTimesheetList", model).then(response => response.data);
  },
  updateBillableHrs (id, billableHrs) {
    return http.put(`/Timesheet/${id}/${billableHrs}`).then(response => response.data);
  }
};
