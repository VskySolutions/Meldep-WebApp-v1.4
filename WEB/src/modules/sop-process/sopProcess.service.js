import { http } from "boot/axios";

export default {
  getAllSOPProcessList (model) {
    return http.post("/sop-process/list", model).then(response => response.data);
  },

  getSOPProcessByIdInDetail (id) {
    return http.get(`/sop-process/details/${id}`).then(response => response.data);
  },

  saveSOPProcess (id, model) {
    if (id) {
      return http.put(`/sop-process/${id}`, model).then(response => response.data);
    } else {
    return http.post("/sop-process", model).then(response => response.data);
    }
  },

  updateSOPProcessStatus (id, statusId) {
    return http.put(`/sop-process/${id}/${statusId}`).then(response => response.data);
  },

  updateSOPProcessActiveStatus (id, isActive) {
    return http.put(`/sop-process/update-active-status/${id}/${isActive}`).then(response => response.data);
  },

  deleteSOPProcess (id) {
    return http.delete(`/sop-process/${id}`).then(response => response.data);
  },
};
