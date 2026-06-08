import { http } from "boot/axios";

export default {
  getAllHelpDeskTopicAndQuestionList (model) {
    return http.post("/helpDeskTopicsQuestions/topic-and-questions/list", model).then(response => response.data);
  },
  getAllHelpDeskTopicList () {
    return http.get("/helpDeskTopicsQuestions/topic/list").then(response => response.data);
  },
  getAllHelpDeskTopicQuestionList (topicId) {
    return http.get(`/helpDeskTopicsQuestions/questions/list/?topicId=${topicId}`).then(response => response.data);
  },
  getAllHelpDeskTopicListForDropdown () {
    return http.get("/helpDeskTopicsQuestions/topicdropdown/list").then(response => response.data);
  },
  getHelpDeskTopicById (id) {
    return http.get(`/helpDeskTopicsQuestions/${id}`).then(response => response.data);
  },
  getAllHelpDeskTopicQuestionsListForDropdown (topicId) {
    return http.get(`/help-desk/questionsdropdown/list?topicId=${topicId}`).then(response => response.data);
  },
  saveHelpDeskTopic (id, model) {
    if (id) {
      return http.put(`/helpDeskTopicsQuestions/${id}`, model).then(response => response.data);
    } else {
      return http.post("/helpDeskTopicsQuestions", model).then(response => response.data);
    }
  },
  deleteHelpDeskTopic (id) {
    return http.delete(`/helpDeskTopicsQuestions/${id}`).then(response => response.data);
  },
  saveHelpDeskQuestion (id, model) {
    if (id) {
      return http.put(`/helpDeskTopicsQuestions/helpDeskQuestion/${id}`, model).then(response => response.data);
    } else {
      return http.post("/helpDeskTopicsQuestions/helpDeskQuestion", model).then(response => response.data);
    }
  },
  deleteHelpDeskQuestion (id) {
    return http.delete(`/helpDeskTopicsQuestions/helpDeskQuestion/${id}`).then(response => response.data);
  },
  checkTopicCanBeDeleted (id) {
    return http.get(`/helpDeskTopicsQuestions/checkTopicCanBeDeleted/${id}`).then(response => response.data);
  }
};
