import { http } from "boot/axios";

export default {
  getAllCustomerProjectsList (model) {
    return http.post("/all-project-planner/get-all-vw-customer-list", model).then(response => response.data);
  },
  getAllProjectsPlannerList (model) {
    return http.post("/all-project-planner/get-all-project-planner-list", model).then(response => response.data);
  },
  getAllProjectsSwimlanePlannerList (model) {
    return http.post("/all-project-planner/get-all-project-swimlane-planner-list", model).then(response => response.data);
  },
  getAllProjectsModulesPlannerList (model) {
    return http.post("/all-project-planner/get-all-project-module-planner-list", model).then(response => response.data);
  },
  getAllProjectsTaskPlannerList (model) {
    return http.post("/all-project-planner/get-all-project-task-planner-list", model).then(response => response.data);
  },
  getAllProjectsActivityPlannerList (model) {
    return http.post("/all-project-planner/get-all-project-activity-planner-list", model).then(response => response.data);
  }
};
