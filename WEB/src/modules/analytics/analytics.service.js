import { http } from "boot/axios";

export default {
  // The register matrix: { fromDate, toDate } as "YYYY-MM-DD", optional employeeIds.
  getTimesheetRegister (model) {
    return http.post("/Analytics/timesheet-register", model).then((response) => response.data);
  },

  // One employee on one day, with the timesheet lines behind the cell.
  getTimesheetRegisterDay (employeeId, date) {
    return http.get("/Analytics/timesheet-register/day", { params: { employeeId, date } }).then((response) => response.data);
  }
};
