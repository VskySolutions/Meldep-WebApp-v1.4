import { http } from "boot/axios";

export default {

  getCustomers (model) {
    return http.post("/customer/list", model).then(response => response.data);
  },

  getCompanyDropdownList () {
    return http.get("/company/dropdownlist").then(response => response.data);
  },
  saveCustomer (id, model) {
    if (id) {
      return http.put(`/customer/${id}`, model).then(response => response.data);
    } else {
      return http.post("/customer", model).then(response => response.data);
    }
  },
  updateCustomerAdvocate (id, assignedToId) {
    return http.put(`/customer/customer-advocate/${id}/${assignedToId}`).then(response => response.data);
  },
  deleteCustomer (id) {
    return http.delete(`/customer/${id}`).then(response => response.data);
  },

  getAllCustomerContactListForDropdown (siteId) {
    return http.get(`/customer/contactdropdown/list?siteId=${siteId}`).then(response => response.data);
  },

  getAllCustomerListForDropdown () {
    return http.get("/customer/customerdropdown/list").then(response => response.data);
  },

  getAllContactListForDropdown (customerId) {
    return http.get(`/customer/companycontactdropdown/list?customerId=${customerId}`).then(response => response.data);
  },

  getAllContactListByCompanyIdForDropdown (companyId) {
    return http.get(`/customer/contactlistbycompanyidFordropdown/list?companyId=${companyId}`).then(response => response.data);
  },

  getAllCompanyContactList (customerId) {
    return http.get(`/customer/companyContactlistFordropdown/list?customerId=${customerId}`).then(response => response.data);
  },

  getAllCompanyListForCustomerDropdown () {
    return http.get("/customer/companyCustomerDropdownList").then(response => response.data);
  },

  getAllClientListForDropdown () {
    return http.get("/customer/customerdropdown/list").then(response => response.data);
  },
  getCustomerDetails (id) {
    return http.get(`/customer/${id}/customerdetails`).then(response => response.data);
  },
  getParentCustomerDropdownList (customerId) {
    const url = customerId
      ? `/customer/parentCustomerList/dropdownlist/${customerId}` // edit mode: exclude current customer
      : "/customer/parentCustomerList/dropdownlist"; // add mode: get all
    return http.get(url).then(response => response.data);
  },
  getAllParentCustomerList () {
    return http.get("/customer/parentCustomerList/list").then(response => response.data);
  }
};
