import { http } from "boot/axios";

export default {
  getDropDownValues (model) {
    return http.post("/drop-downs/list", model).then(response => response.data);
  },
  getDropDownValue (id) {
    return http.get(`/drop-downs/${id}`).then(response => response.data);
  },
  saveDropDown (id, model) {
    if (id) {
      return http.put(`/drop-downs/${id}`, model).then(response => response.data);
    } else {
      return http.post("/drop-downs", model).then(response => response.data);
    }
  },
  deleteDropDown (id) {
    return http.delete(`/drop-downs/${id}`).then(response => response.data);
  }
};
