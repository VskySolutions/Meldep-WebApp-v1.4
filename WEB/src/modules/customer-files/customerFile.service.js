import { http } from "boot/axios";

export default {
  getCustomerFiles (model) {
    return http.post("/customer-file/list", model).then(response => response.data);
  },
  getCustomerFilesFromVW (model) {
    return http.post("/customer-file/listVW", model).then(response => response.data);
  },
  getCustomerFileDetailsById (id) {
    return http.get(`/customer-file/${id}`).then(response => response.data);
  },
  // getCustomerFileDetails (id) {
  //   return http.get(`/customer-file/${id}`).then(response => response.data);
  // },
  // saveCustomerFiles (model) {
  //   return http.post("/customer-file", model).then(response => response.data);
  // },

  saveCustomerFiles (id, model) {
    if (id) {
      return http.put(`/customer-file/${id}`, model).then(response => response.data);
    } else {
      return http.post("/customer-file", model).then(response => response.data);
    }
  },

  getCustomerFileDetailsByYearAndId (year, customerId) {
    return http.get(`/customer-file/get-customerfiles-by-year-id/${year}/${customerId}`).then(response => response.data);
  },

  deleteCustomerFile (id) {
    return http.delete(`/customer-file/${id}`).then(response => response.data);
  }
};
