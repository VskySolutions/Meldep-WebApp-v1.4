import { http } from "boot/axios";

export default {
  // apply-leave
  getEmployeeLeaves (model) {
    return http.post("/employee-leave/list", model).then(response => response.data);
  },

  getEmployeeLeaveBalance (employeeId) {
    return http.get(`/employee-leave/leavebalance/${employeeId}`).then(response => response.data);
  },

  getLeaveBalanceDeatilsByEmployeeId (employeeId, year) {
    return http.get(`/employee-leave/leavebalancedetails/${employeeId}/${year}`).then(response => response.data);
  },

  isSandwichLeave (fromDate, toDate) {
    const startDate = fromDate ? fromDate.replace(/\//g, "-") : null;
    const endDate = toDate ? toDate.replace(/\//g, "-") : null;
    return http.get(`/employee-leave/is-sandwich-leave/${startDate}/${endDate}`).then(response => response.data);
  },

  saveEmployeeLeave (model) {
    return http.post("/employee-leave", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },

  deleteEmployeeLeave (id) {
    return http.delete(`/employee-leave/${id}`).then(response => response.data);
  },

  // approve-leave
  getEmployeeLeavesForApprove (model) {
    return http.post("/employee-leave/forward/list", model).then(response => response.data);
  },

  getEmployeeLeave (id) {
    return http.get(`/employee-leave/${id}`).then(response => response.data);
  },

  saveEmployeeLeaveDetails (ids, model) {
    if (ids) {
      return http.put(`/employee-leave/${ids}`, model).then(response => response.data);
    } else {
      return http.post("/employee-leave", model).then(response => response.data);
    }
  },

  getFiveEmployeeLeaveForApprove () {
    return http.post("/employee-leave/dashBoardLeave/list").then(response => response.data);
  },

  getEmployeeLeaveListForDashboard () {
    return http.post("/employee-leave/approvedLeave/list").then(response => response.data);
  },
  getEmployeeLeaveListForMovReg (dateStr) {
    const movRegDateStr = dateStr ? dateStr.replace(/\//g, "-") : null;
    return http.post(`/employee-leave/leaveListForMovReg/list/${movRegDateStr}`).then(response => response.data);
  },

  // forward-leave
  cancelEmployeeLeave (id) {
    return http.post(`/employee-leave/cancelleave/${id}`).then(response => response.data);
  },

  // leave-credits
  getLeaveCredits (model) {
    return http.post("/leaveCredits/list", model).then(response => response.data);
  },

  getLeaveCredit (id) {
    return http.get(`/leaveCredits/${id}`).then(response => response.data);
  },

  getLeaveCreditByEmployeeId (id, year) {
    return http.get(`/leaveCredits/details/${id}/${year}`).then(response => response.data);
  },

  saveLeaveCredit (model) {
    return http.post("/leaveCredits", model).then(response => response.data);
  },

  saveEmployeeLeaveCredits (model) {
    return http.post("/leaveCredits/credits", model).then(response => response.data);
  }
};
