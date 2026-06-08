import { http } from "boot/axios";

export default {
  getAllTestCase (model) {
    return http.post("/test-case/list", model).then(response => response.data);
  },

  getTestCase (id) {
    return http.get(`/test-case/${id}`).then(response => response.data);
  },

  getTestCaseDetails (id) {
    return http.get(`/test-case/details/${id}`).then(response => response.data);
  },

  saveTestCase (id, model) {
    if (id) {
      return http.put(`/test-case/${id}`, model).then(response => response.data);
    } else {
      return http.post("/test-case", model).then(response => response.data);
    }
  },

  updateTestCaseStatus (id, statusId) {
    return http.put(`/test-case/updateTestCaseStatus/${id}/${statusId}`, statusId).then(response => response.data);
  },
  deleteTestCase (id) {
    return http.delete(`/test-case/${id}`).then(response => response.data);
  }

  // getAllProjectModuleListForDropdown (projectId) {
  //   return http.get(`/project-modules/dropdown/list?ProjectId=${projectId}`).then(response => response.data);
  // },

  // getAllModuleListByProject (projectId) {
  //   return http.get(`/project-modules/listbyproject/?projectId=${projectId}`).then(response => response.data);
  // }
};
