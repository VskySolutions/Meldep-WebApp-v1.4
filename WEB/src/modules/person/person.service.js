import { http } from "boot/axios";

export default {
  getPersons (model) {
    return http.post("/person/list", model).then(response => response.data);
  },

  getPerson (id) {
    return http.get(`/person/${id}`).then(response => response.data);
  },

  savePerson (id, model) {
    if (id) {
      return http.put(`/person/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/person", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },

  deletePerson (id) {
    return http.delete(`/person/${id}`).then(response => response.data);
  },

  getAllPersonListForDropdown (id) {
    return http.get(`/person/dropdown/list?siteId=${id}`).then(response => response.data);
  },
  getAllPersonPrimaryEmailAddressListForDropdown (siteId) {
    return http.get(`/person/dropdown/primaryEmailAddressList?siteId=${siteId}`).then(response => response.data);
  },
  getAllIsSharedPersonListForDropdown (id) {
    return http.get(`/person/dropdown/isSharedPersonList?siteId=${id}`).then(response => response.data);
  },
  convertPersonToCustomer (id) {
    return http.put(`/person/convertPersonToCustomer/${id}`).then(response => response.data);
  }
};
