import { http } from "boot/axios";

export default {
  // #region Timesheet Register

  // The register matrix: { fromDate, toDate } as "YYYY-MM-DD", optional employeeIds.
  getTimesheetRegister (model) {
    return http.post("/Analytics/timesheet-register", model).then((response) => response.data);
  },

  // One employee on one day, with the timesheet lines behind the cell.
  getTimesheetRegisterDay (employeeId, date) {
    return http.get("/Analytics/timesheet-register/day", { params: { employeeId, date } }).then((response) => response.data);
  },

  // One employee over a range (at most a quarter): the register row, the days and every line.
  getTimesheetRegisterEmployee (employeeId, fromDate, toDate) {
    return http.get("/Analytics/timesheet-register/employee", { params: { employeeId, fromDate, toDate } }).then((response) => response.data);
  },

  // #endregion Timesheet Register

  // #region TimeInTimeOut-MovementRegister (Attendance Register)

  // The register matrix, TimeInTimeOut and the Movement Register merged: { fromDate, toDate } as
  // "YYYY-MM-DD", optional employeeIds.
  getAttendanceRegister (model) {
    return http.post("/Analytics/attendance-register", model).then((response) => response.data);
  },

  // One employee on one day, with the time-ins and the Movement Register entries behind the cell.
  getAttendanceRegisterDay (employeeId, date) {
    return http.get("/Analytics/attendance-register/day", { params: { employeeId, date } }).then((response) => response.data);
  },

  // One employee over a range (at most a quarter): the register row, the days, every time-in and every
  // Movement Register entry.
  getAttendanceRegisterEmployee (employeeId, fromDate, toDate) {
    return http.get("/Analytics/attendance-register/employee", { params: { employeeId, fromDate, toDate } }).then((response) => response.data);
  }

  // #endregion TimeInTimeOut-MovementRegister (Attendance Register)
};
