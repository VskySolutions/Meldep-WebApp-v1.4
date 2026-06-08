import { http } from "boot/axios";

export default {
  getAllMovementRegisters (model) {
    return http.post("/movementRegister/list", model).then(response => response.data);
  },
  getMovementRegistersForDashboard (model) {
    return http.post("/movementRegister/dashboardlist", model).then(response => response.data);
  },
  getMovementRegisterDetails (id, detailId) {
    return http.get(`/movementRegister/details/${id}/${detailId}`).then(response => response.data);
  },
  getMovementRegisterDateRange () {
    return http.get("/movementRegister/daterange").then(response => response.data);
  },
  saveMovementRegister (detailId, model) {
    if (detailId) {
      return http.put(`/movementRegister/${detailId}`, model).then(response => response.data);
    } else {
      return http.post("/movementRegister", model).then(response => response.data);
    }
  }
};
