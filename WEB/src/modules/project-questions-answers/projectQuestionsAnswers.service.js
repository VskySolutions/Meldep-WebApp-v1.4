import { http } from "boot/axios";

export default {
  getAllQuestionAnswers (model) {
    return http.post("/project-questions-answers/list", model).then(response => response.data);
  },

  getQuestionAnswersInDetailsById (id) {
    return http.get(`/project-questions-answers/details/${id}`).then(response => response.data);
  },

  saveQuestionAnswers (id, model) {
    if (id) {
      return http.put(`/project-questions-answers/${id}`, model).then(response => response.data);
    } else {
      return http.post("/project-questions-answers", model).then(response => response.data);
    }
  },

  deleteQuestionAnswers (id) {
    return http.delete(`/project-questions-answers/${id}`).then(response => response.data);
  }
};
