import { http } from "boot/axios";

export default {
  getAllVendorList (model) {
    return http.post("/expense-vendor/list", model).then(response => response.data);
  },

  getVendor (id) {
    return http.get(`/expense-vendor/${id}`).then(response => response.data);
  },

  getAllExpenseVendorListForDropdown (isOwnerName = false) {
    return http.get(`/expense-vendor/dropdown/list?isOwnerName=${isOwnerName}`).then(response => response.data);
  },

  getAllVendorsAccountListForDropdown (vendorId) {
    return http.get(`/expense-vendor/accountsdropdown/list/${vendorId}`).then(response => response.data);
  },

  saveVendor (id, model) {
    if (id) {
      return http.put(`/expense-vendor/${id}`, model).then(response => response.data);
    } else {
      return http.post("/expense-vendor", model).then(response => response.data);
    }
  },

  deleteVendor (id) {
    return http.delete(`/expense-vendor/${id}`).then(response => response.data);
  }
};
