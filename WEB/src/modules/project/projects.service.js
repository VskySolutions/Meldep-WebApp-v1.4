import { http } from "boot/axios";

export default {
  getProjects (model) {
    return http.post("/projects/list", model).then(response => response.data);
  },
  getProject (id) {
    return http.get(`/projects/${id}`).then(response => response.data);
  },
  getAllProjectsForNotes (model) {
    return http.post("/projects/projectListForNotes", model).then(response => response.data);
  },
  saveProject (id, model) {
    if (id) {
      return http.put(`/projects/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/projects", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },
  saveProjectFiles (model) {
    return http.post("/projects/add-project-files", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },
  saveTags (model) {
    return http.post("/projects/projectTags", model).then(response => response.data);
  },
  updateProjectStatus (id, statusId) {
    return http.put(`/projects/${id}/${statusId}`, statusId).then(response => response.data);
  },
  updateProjectPriority (id, priorityId) {
    return http.put(`/projects/priority/${id}/${priorityId}`).then(response => response.data);
  },
  updateProjectType (id, typeId) {
    return http.put(`/projects/type/${id}/${typeId}`).then(response => response.data);
  },
  deleteProject (id) {
    return http.delete(`/projects/${id}`).then(response => response.data);
  },
  deleteFile (id, type) {
    return http.delete(`/projects/project-file/${id}/${type}`).then(response => response.data);
  },
  getAllProjectListForDropdown (statuses) {
    if (statuses) {
      const query = statuses.map(s => `statuses=${encodeURIComponent(s)}`).join("&");
      return http.get(`/projects/dropdown/list?${query}`).then(response => response.data);
    } else { return http.get("/projects/dropdown/list").then(response => response.data); }
  },
  getAllProjectCategoriesForDropdown () {
    return http.get("/projects/projectCategoriesdropdown/list").then(response => response.data);
  },
  getAllProjectSubCategoriesByCategoryIdForDropdown (projectCategoryId) {
    return http.get(`/projects/projectSubCategoriesByCategoryIdForDropdown/list?projectCategoryId=${projectCategoryId}`).then(response => response.data);
  },
  // getProjectsListForDropdown (isTemplate = false, IsActive = true, isAllProject = false) {
  //   const ActiveStatus = IsActive === null ? 'all' : IsActive;
  //   return http.get(`/projects/allDropdown/list/${isTemplate}/${ActiveStatus}/${isAllProject}`).then(response => response.data);
  // },
  getProjectsListForDropdown(isTemplate = false, IsActive = true, isAllProject = false) {
    const ActiveStatus = IsActive === null ? 'all' : IsActive;
    return http.get(`/projects/allDropdown/list?isTemplate=${isTemplate}&ActiveStatus=${ActiveStatus}&isAllProject=${isAllProject}`
    ).then(response => response.data);
  },
  getAllProjectTagListForDropdown () {
    return http.get("/projects/projectTags/dropdown/list").then(response => response.data);
  },
  getProjectDetailsByIds (ids) {
    const idsQuery = ids.join(",");
    return http.get(`/projects/projectdetailsbyids?ids=${idsQuery}`).then(response => response.data);
  },
  getProjectsAndCharterListForDashboard (model) {
    return http.post("/projects/Dashboardlist", model).then(response => response.data);
  },
  getAllProjectModulesForDashboard (model) {
    return http.post("/project-dashboard/projectModulelist", model).then(response => response.data);
  },
  getAllProjectTasksForDashboard (model) {
    return http.post("/project-dashboard/projectTaskList", model).then(response => response.data);
  },
  getAllIssuesForDashboard (model) {
    return http.post("/project-dashboard/issueList", model).then(response => response.data);
  },
  getAllProjectActivitiesForDashboard (model) {
    return http.post("/project-dashboard/projectActivityList", model).then(response => response.data);
  },
  getOrganizationForDashboard (id) {
    return http.get(`/project-dashboard/siteInfobyid/${id}`).then(response => response.data);
  },
  getAllNotesByProjectId (model) {
    return http.post("/project-dashboard/notesList", model).then(response => response.data);
  },
  getAllFilesByProjectId (model) {
    return http.post("/projects/filesList", model).then(response => response.data);
  },
  getAllTestPlanForDashboard (model) {
    return http.post("/project-dashboard/testPlanList", model).then(response => response.data);
  },
  getAllTestCasesForDashboard (model) {
    return http.post("/project-dashboard/testCasesList", model).then(response => response.data);
  },
  getAllRequirementGroupsForDashboard (model) {
    return http.post("/project-dashboard/requirementGroupList", model).then(response => response.data);
  },
  getAllRequirementsForDashboard (model) {
    return http.post("/project-dashboard/requirementList", model).then(response => response.data);
  },
  getProjectMessages (id) {
    return http.get(`/projects/projectmessages/list/${id}`).then(response => response.data);
  },
  sentMessage (model) {
    return http.post("/projects/SentMessage", model).then(response => response.data);
  },
  updateMessage (id, model) {
    return http.post(`/projects/UpdateMessage/${id}`, model).then(response => response.data);
  },
  deleteMessage (id) {
    return http.delete(`/projects/DeleteMessage/${id}`).then(response => response.data);
  },
  getProjectEmployees (id) {
    return http.get(`/projects/ProjectEmployees/list?id=${id}`).then(response => response.data);
  },
  getProjectCharterEmployeesWithWeeklyPlanHoursByProjectId (id) {
    return http.get(`/projects/ProjectCharterEmployeesWithWeeklyPlanHours/list?id=${id}`).then(response => response.data);
  },
  getProjectSwimlanesById (id) {
    return http.get(`project-swimlane/get-project-swimlanes-by-id/${id}`).then(response => response.data);
  },
  saveProjectSwimLane (projectSwlimLaneModelList) {
    return http.post("/project-swimlane/addedit-project-swimlanes", projectSwlimLaneModelList).then(response => response.data);
  },
  deleteProjectSublaneTasks (id) {
    return http.put(`/project-swimlane/delete-project-sublane-task/${id}`).then(response => response.data);
  },
  updateProjectIsPinned (id, pinstatus) {
    return http.put(`/projects/pinstatus/${id}/${pinstatus}`).then(response => response.data);
  },
  updateProjectColor (id, model) {
    return http.post("/projects/projectColor-projectStatus", model).then(response => response.data);
  },
  updateProjectEndDate (id, goLiveDate) {
    const endDate = goLiveDate.replace(/\//g, "-");
    return http.put(`/projects/end-date/${id}/${endDate}`).then(response => response.data);
  },
  // Project Workboard Ver 2
  getWorkBoardByProjectId (projectId) {
    return http.get(`/project-swimlane/get-workboard?projectId=${projectId}`).then(response => response.data);
  },
  getProjectWorkBoardById (projectId) {
    return http.get(`/projects/workboard?projectId=${projectId}`).then(response => response.data);
  },
  saveWorkboard (model) {
    return http.post("/project-swimlane/save-project-workboard", model).then(response => response.data);
  },
  addProjectSwimlane (model) {
    return http.post("/project-swimlane/add-project-swimlane", model).then(response => response.data);
  },
  duplicateSwimLane (projectId, projectSwimlaneId) {
    return http.post(`/project-swimlane/duplicate-swimlane?projectId=${projectId}&projectSwimlaneId=${projectSwimlaneId}`).then(response => response.data);
  },
  duplicateList (projectId, projectSwimlaneId, listId) {
    return http.post(`/project-swimlane/duplicate-list?projectId=${projectId}&projectSwimlaneId=${projectSwimlaneId}&listId=${listId}`).then(response => response.data);
  },
  duplicateTask (taskId) {
    return http.post(`/project-swimlane/duplicate-task?taskId=${taskId}`).then(response => response.data);
  },
  getProjectSwimlaneById (id) {
    return http.get(`/project-swimlane/${id}`).then(response => response.data);
  },
  copyProjectAsTemplate (model) {
    return http.post("/projects/copy-project-as-template", model).then(response => response.data);
  },

  // Project Weekly/MOnthly Plan
  getWeeklyProjects (model) {
    return http.post("/projects/get-project-weekly-plan", model).then(response => response.data);
  },
  getWeeklyProjectPlanInDetail (id, planTypeId, skipIndex, takeCount, weekEndDate) {
    const endDate = weekEndDate && weekEndDate !== "undefined" ? weekEndDate : "";
    return http.post("/projects/get-project-weekly-plan-details?projectId=" + id + "&planTypeId=" + planTypeId + "&skipIndex=" + skipIndex + "&takeCount=" + takeCount + "&weekEndDate=" + endDate).then(response => response.data);
  },
  addProjectPlanApprover (projectId, planApproverId) {
    return http.post("/projects/add-Project-Plan-Approver?projectId=" + projectId + "&planApproverId=" + planApproverId).then(response => response.data);
  },
  approveThisWeeklyPlan (id, isLock) {
    return http.post("/projects/approve-this-Plan?planDateId=" + id + "&isLock=" + isLock).then(response => response.data);
  },
  addPlanCompletionPercentage (id, completionPercentage) {
    return http.post("/projects/add-Plan-Completion-Percentage?planDateId=" + id + "&completionPercentage=" + completionPercentage).then(response => response.data);
  },
  addProjectWeeklyPlanDates (model) {
    return http.post("/projects/add-project-weeklyplan-date", model).then(response => response.data);
  },
  saveProjectWeeklyPlanDatesLine (model) {
    return http.post("/projects/save-project-weeklyplan-date-line", model).then(response => response.data);
  },
  deleteProjectWeeklyPlanDatesLine (model) {
    return http.post("/projects/delete-project-weeklyplan-date-line", model).then(response => response.data);
  },
  addProjectToWeeklyPlan (model) {
    return http.post("/projects/add-project-to-weekly-plan", model).then(response => response.data);
  },
  getProjectWeeklyPlanTypeId (type, value) {
    return http.post("/projects/get-project-weekly-type-id?type=" + type + "&value=" + value).then(response => response.data);
  },
  addResourceToWeeklyPlanLine (model) {
    return http.post("/projects/add-project-weeklyplan-date-line-resource", model).then(response => response.data);
  },
  deleteResourceToWeeklyPlanLine (resourceId) {
    return http.post("/projects/delete-project-weeklyplan-date-line-resource?Id=" + resourceId).then(response => response.data);
  },
  checkIfEmployeeAlreadyExistsInPlanLine (lineId, employeeId) {
    return http.post("/projects/check-if-employee-already-exists-in-plan-line?Id=" + lineId + "&EmployeeId=" + employeeId).then(response => response.data);
  },
  getResourceSummaryForWeekPlanById (planTypeId, planDateId) {
    return http.post("/projects/get-resource-summary-for-weekPlan-by-id?planTypeId=" + planTypeId + "&planDateId=" + planDateId).then(response => response.data);
  },
  getAllEmployeesWithHoursForWeeklyMonthlyPlanning (projectId, planTypeId, date) {
    return http.post("/projects/get-all-employees-esthrs-as-dropdown-list?projectId=" + projectId + "&planTypeId=" + planTypeId + "&weekDate=" + date).then(response => response.data);
  },
  linkRequirementTaskIssueToWeeklyPlanDate (model) {
    return http.post("/projects/link-RequirementTaskIssue-To-WeeklyPlan-Date", model).then(response => response.data);
  },
  deleteProjectWeeklyPlanDateMapping (mappingId) {
    return http.post(`/projects/delete-RequirementTaskIssue-To-WeeklyPlan-Date?MappingId=${mappingId}`).then(response => response.data);
  },

  // project users
  getProjectUsers (model) {
    return http.post("/project-users/list", model).then(response => response.data);
  },
  getProjectUserByProjectId (id) {
    return http.get(`/project-users/user/${id}`).then(response => response.data);
  },
  saveProjectUser (id, model) {
    if (id) {
      return http.put(`/project-users/${id}`, model).then(response => response.data);
    }
  },
  assignBulk (ids, model) {
    if (ids) {
      return http.put(`/project-users/savebulk/${ids}`, model).then(response => response.data);
    }
  }
};
