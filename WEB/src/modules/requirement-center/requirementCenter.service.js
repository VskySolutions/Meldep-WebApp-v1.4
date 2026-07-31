import { http } from "boot/axios";

export default {
  getTestCasesByRequirementId (requirementId) {
    return http.get(`/requirement-dashboard/test-case-list?requirementId=${requirementId}`).then(response => response.data);
  },
  getIssuesByRequirementId (requirementId) {
    return http.get(`/requirement-dashboard/issue-list?requirementId=${requirementId}`).then(response => response.data);
  },
  getTasksByRequirementId (requirementId) {
    return http.get(`/requirement-dashboard/task-list?requirementId=${requirementId}`).then(response => response.data);
  },
  getTimesheetByRequirementId (requirementId) {
    return http.get(`/requirement-dashboard/timesheet-list?requirementId=${requirementId}`).then(response => response.data);
  },
  getGroupedTimesheetsByRequirementId(requirementId, groupBy) {
    return http.get(
      `/requirement-dashboard/timesheet-groups?requirementId=${requirementId}&groupBy=${groupBy}`
    ).then(r => r.data);
  },
  getTimesheetDetails(requirementId, groupBy, groupId) {
    return http
      .get(
        `/requirement-dashboard/timesheet-details?requirementId=${requirementId}&groupBy=${groupBy}&groupId=${groupId}`
      )
      .then(response => response.data);
  },
};
