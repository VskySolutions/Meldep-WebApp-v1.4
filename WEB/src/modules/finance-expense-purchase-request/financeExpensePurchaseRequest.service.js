import { http } from "boot/axios";

export default {
  getAllPurchaseExpenseRequests (model) {
    return http.post("/purchase-expense-request/list", model).then(response => response.data);
  },

  getAllApprovePurchaseExpenseRequests (model) {
    return http.post("/purchase-expense-request/approve-purchase-expense-list", model).then(response => response.data);
  },

  getById (id) {
    return http.get(`/purchase-expense-request/${id}`).then(response => response.data);
  },

  getPurchaseExpenseDetailsById (id) {
    return http.get(`/purchase-expense-request/get-purchase-expense-details/${id}`).then(response => response.data);
  },

  savePurchaseExpenseRequest (id, model) {
    if (id) {
      return http.put(`/purchase-expense-request/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/purchase-expense-request", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },

  delete (ExpenseId) {
    return http.delete(`/purchase-expense-request/${ExpenseId}`).then(response => response.data);
  },

  forwardPurchaseExpenseToApprovers (model) {
    return http.post("/purchase-expense-request/forward-purchase-expense-to-approvers", model).then(response => response.data);
  },

  getPurchaseExpenseRequestList (statusName) {
    return http.get(`/purchase-expense-request/purchase-expense-dropdown-list/${statusName}`).then(response => response.data);
  }

};
