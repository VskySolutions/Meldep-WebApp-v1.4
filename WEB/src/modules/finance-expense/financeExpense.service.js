import { http } from "boot/axios";

export default {
  getAllExpenseList (model) {
    return http.post("/Expense/list", model).then(response => response.data);
  },
  getAllApproveExpenseList (model) {
    return http.post("/Expense/approve-expense-list", model).then(response => response.data);
  },

  getExpenseById (id) {
    return http.get(`/Expense/GetExpenseId/${id}`).then(response => response.data);
  },

  getBankAccountNoBySelectItemList () {
    return http.get("/Expense/GetBankAccountNoBySelectItemList").then(response => response.data);
  },

  getExpenseCategoryList () {
    return http.get("/Expense/GetExpenseCategorySelectItemList").then(response => response.data);
  },

  getExpenseSubcategoryList (CategoryId) {
    return http.get(`/Expense/subCategory/list?categoryId=${CategoryId}`).then(response => response.data);
  },

  saveExpense (id, model) {
    if (id) {
      return http.put(`/Expense/Update/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/Expense/Create", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },

  deleteExpense (ExpenseId) {
    return http.delete(`/Expense/${ExpenseId}`).then(response => response.data);
  },

  forwardExpenseToApprovers (model) {
    return http.post("/Expense/forward-expense-to-approvers", model).then(response => response.data);
  }
};
