import { http } from "boot/axios";

export default {
  getTestCasesByRequirementId (model) {
    return http.post("/requirement-dashboard/test-case-list", model).then(response => response.data);
  },
  getIssuesByRequirementId (model) {
    return http.post("/requirement-dashboard/issue-list", model).then(response => response.data);
  },
  getTasksByRequirementId (model) {
    return http.post("/requirement-dashboard/task-list", model).then(response => response.data);
  },
  getTimesheetByRequirementId (requirementId) {
    return http.get(`/requirement-dashboard/timesheet-list?requirementId=${requirementId}`).then(response => response.data);
  },
  getGroupedTimesheetsByRequirementId (model) {
    return http.post("/requirement-dashboard/timesheet-groups", model).then(response => response.data);
  },
  getProjectQAByRequirementId (model) {
    return http.post("/requirement-dashboard/project-QA", model).then(response => response.data);
  },
  getProjectActionItemsByRequirementId (model) {
    return http.post("/requirement-dashboard/project-action-items", model).then(response => response.data);
  },
  getTimesheetDetails(searchModel) {
    return http
      .post("/requirement-dashboard/timesheet-details", searchModel)
      .then(response => response.data);
  }
};
