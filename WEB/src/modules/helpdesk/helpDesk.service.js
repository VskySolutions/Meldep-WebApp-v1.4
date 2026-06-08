import { http } from "boot/axios";

export default {
  getAllHelpDesks (model) {
    return http.post("/help-desk/list", model).then(response => response.data);
  },
  getRequesterDropdown () {
    return http.get("/help-desk/requesterDropdown/list").then(response => response.data);
  },
  getCompanyDropdown () {
    return http.get("/help-desk/companyDropdown/list").then(response => response.data);
  },
  getAllHelpDeskTopicListForDropdown () {
    return http.get("/help-desk/topicdropdown/list").then(response => response.data);
  },
  getAllHelpDeskTopicQuestionsListForDropdown (topicId) {
    return http.get(`/help-desk/questionsdropdown/list?TopicId=${topicId}`).then(response => response.data);
  },
  getAllHelpDeskEmailRepliesMappingList (helpDeskId, skipIndex, takeCount, isSystemEmail) {
    return http.get("/help-desk/emailReplies/list?HelpDeskId=" + helpDeskId + "&skipIndex=" + skipIndex + "&takeCount=" + takeCount + "&isSystemEmail=" + isSystemEmail).then(response => response.data);
  },

  getHelpDesk (id) {
    return http.get(`/help-desk/${id}`).then(response => response.data);
  },

  updateHelpDeskStatus (id, statusId) {
    return http.put(`/help-desk/${id}/${statusId}`).then(response => response.data);
  },
  updateAssignedTo (id, assignedToId) {
    return http.put(`/help-desk/assignedTo/${id}/${assignedToId}`).then(response => response.data);
  },
  addorUpdateHelpDeskStatusComment (id, model) {
    return http.put(`/help-desk/comment/${id}`, model).then(response => response.data);
  },
  updateCompanyClient (id, companyId) {
    return http.put(`/help-desk/company/${id}/${companyId}`).then(response => response.data);
  },
  updateHelpDeskPriority (id, priorityId) {
    return http.put(`/help-desk/priority/${id}/${priorityId}`).then(response => response.data);
  },
  saveHelpDesk (id, model) {
    if (id) {
      return http.put(`/help-desk/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/help-desk", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },
  saveHelpDeskFiles (model) {
    return http.post("/help-desk/add-help-desk-files", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },
  saveEmailReplies (model) {
    return http.post("/help-desk/save-email-replies", model).then(response => response.data);
  },

  deleteHelpDesk (id) {
    return http.delete(`/help-desk/${id}`).then(response => response.data);
  }
};
