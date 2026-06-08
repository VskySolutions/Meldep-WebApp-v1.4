import { http } from "boot/axios";

export default {
  // getCompanys () {
  //   return http.get("/company").then(response => response.data);
  // },
  getCompanyContactList (model) {
    return http.post("/company-contact/list", model).then(response => response.data);
  },

  getCompany (id) {
    return http.get(`/company/${id}`).then(response => response.data);
  },

  getCompanyContactDetails (id) {
    return http.get(`/company-contact/${id}/companycontactdetails`).then(response => response.data);
  },

  getCompanyContacts (id) {
    return http.get(`/company/contacts?companyId=${id}`).then(response => response.data);
  },

  saveCompany (id, model) {
    if (id) {
      return http.put(`/company/${id}`, model).then(response => response.data);
    } else {
      return http.post("/company", model).then(response => response.data);
    }
  },

  saveCompanyAndContacts (id, model) {
    if (id) {
      return http.put(`/company/${id}`, model).then(response => response.data);
    } else {
      return http.post("/company", model).then(response => response.data);
    }
  },

  saveCompanyContacts (id, model) {
    if (id) {
      return http.put(`/company-contact/${id}`, model).then(response => response.data);
    } else {
      return http.post("/company-contact", model).then(response => response.data);
    }
  },

  deleteCompanyContact (id) {
    return http.delete(`/company-contact/${id}`).then(response => response.data);
  },
  getAllClientListForDropdown () {
    return http.get("/company/client/dropdownlist").then(response => response.data);
  },
  getCompanyDropdownList () {
    return http.get("/company/dropdownlist").then(response => response.data);
  }
};
