import { http } from "boot/axios";

export default {
  getAllSOPTemplateList (model) {
    return http.post("/sop-template/list", model).then(response => response.data);
  },

  // getSOPTemplateById (id) {
  //   return http.get(`/sop-template/${id}`).then(response => response.data);
  // },

  getAllSOPTemplateListForDropdown () {
    return http.get("/sop-template/dropdown/list").then(response => response.data);
  },

  getSOPTemplateByIdInDetail (id) {
    return http.get(`/sop-template/details/${id}`).then(response => response.data);
  },

  createUpdateSOPTemplate (model) {
    return http.post("/sop-template/createupdate", model).then(response => response.data);
  },

  deleteSOPTemplate (id) {
    return http.delete(`/sop-template/${id}`).then(response => response.data);
  },
};
