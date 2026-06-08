import { http } from "boot/axios";

export default {
  getAllTimeInTimeOut (model) {
    return http.post("/time-in-time-out/list", model).then(response => response.data);
  },
  getTimeInTimeOut (id) {
    return http.get(`/time-in-time-out/${id}`).then(response => response.data);
  },
  getTimeInTimeOutDetails (id) {
    return http.get(`/time-in-time-out/details/${id}`).then(response => response.data);
  },
  getTimeInTimeOutDetailsByEmployeeId (employeeId) {
    return http.get(`/time-in-time-out/${employeeId}`).then(response => response.data);
  },
  getMovementRegByEmployeeId (employeeId) {
    return http.get(`/movementRegister/latest-record/${employeeId}`).then(response => response.data);
  },
  saveTimeInTimeOut (id) {
    if (id) {
      return http.put(`/time-in-time-out/${id}`).then(response => response.data);
    } else {
      return http.post("/time-in-time-out").then(response => response.data);
    }
  },
  saveBreak (id, model) {
    return http.put(`/time-in-time-out/${id}/addUpdateBreak`, model).then(response => response.data);
  },
  deleteTimeInTimeOut (id) {
    return http.delete(`/time-in-time-out/${id}`).then(response => response.data);
  }
};
