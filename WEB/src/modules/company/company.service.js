import { http } from "boot/axios";

export default {
  // getCompanys () {
  //   return http.get("/company").then(response => response.data);
  // },
  getCompanys (model) {
    return http.post("/company/list", model).then(response => response.data);
  },

  getCompany (id) {
    return http.get(`/company/${id}`).then(response => response.data);
  },

  getCompanyDetails (id) {
    return http.get(`/company/${id}/companydetails`).then(response => response.data);
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

  updateCompanyStatus (id, statusId) {
    return http.put(`/company/updateCompanyStatus/${id}/${statusId}`, statusId).then(response => response.data);
  },
  deleteCompany (id) {
    return http.delete(`/company/${id}`).then(response => response.data);
  },

  getAllClientListForDropdown () {
    return http.get("/company/client/dropdownlist").then(response => response.data);
  },

  getCompanyDropdownList () {
    return http.get("/company/dropdownlist").then(response => response.data);
  },

  getPrimaryEmployeeDropdownList () {
    return http.get("/company/primaryemployeedropdownlist").then(response => response.data);
  }
};
