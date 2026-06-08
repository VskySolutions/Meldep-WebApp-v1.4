import { http } from "boot/axios";

export default {
  getAllAdvanceExpenseRequests (model) {
    return http.post("/advance-expense-request/list", model).then(response => response.data);
  },

  getAllApproveAdvanceExpenseRequests (model) {
    return http.post("/advance-expense-request/approve-advance-expense-list", model).then(response => response.data);
  },

  getById (id) {
    return http.get(`/advance-expense-request/${id}`).then(response => response.data);
  },

  getAdvanceExpenseDetailsById (id) {
    return http.get(`/advance-expense-request/get-advance-expense-details/${id}`).then(response => response.data);
  },

  saveAdvanceExpenseRequest (id, model) {
    if (id) {
      return http.put(`/advance-expense-request/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/advance-expense-request", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },

  delete (ExpenseId) {
    return http.delete(`/advance-expense-request/${ExpenseId}`).then(response => response.data);
  },

  forwardAdvanceExpenseToApprovers (model) {
    return http.post("/advance-expense-request/forward-advance-expense-to-approvers", model).then(response => response.data);
  },

  getAdvanceExpenseRequestList (statusName) {
    return http.get(`/advance-expense-request/advance-expense-dropdown-list/${statusName}`).then(response => response.data);
  }
};
