import { http } from "boot/axios";

export default {
  getAllBankAccountList (model) {
    return http.post("/expense-bankAccount/list", model).then(response => response.data);
  },

  getBankAccountById (id) {
    return http.get(`/expense-bankAccount/GetByBankAccountId/${id}`).then(response => response.data);
  },

  saveBankAccount (id, model) {
    if (id) {
      return http.put(`/expense-bankAccount/${id}`, model).then(response => response.data);
    } else {
      return http.post("/expense-bankAccount", model).then(response => response.data);
    }
  },

  deactivateBankAccount (id) {
    return http.put(`/expense-bankAccount/Deactivate/${id}`).then(response => response.data);
  },

  deleteBankAccount (id) {
    return http.delete(`/expense-bankAccount/Delete/${id}`).then(response => response.data);
  }
};
