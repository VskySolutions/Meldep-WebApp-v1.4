import { http } from "boot/axios";

export default {
  getAllCandidateList (model) {
    return http.post("/candidate/list", model).then(response => response.data);
  },
  getCandidate (id) {
    return http.get(`/candidate/${id}`).then(response => response.data);
  },
  getCandidateActivityLogById (id) {
    return http.get(`/candidate/${id}/candidateActivity`).then(response => response.data);
  },
  getActivityLogById (id) {
    return http.get(`/candidate/${id}/Activitylog`).then(response => response.data);
  },
  getCandidateFeedbackDetailsById (id) {
    return http.get(`/candidate/${id}/candidateFeedbackLog`).then(response => response.data);
  },
  getCandidateActivities () {
    return http.get("/candidate/candidateactivities-list").then(response => response.data);
  },
  saveCandidate (id, model) {
    if (id) {
      return http.put(`/candidate/${id}`, model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    } else {
      return http.post("/candidate", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
    }
  },
  saveCandidateActivityLogs (model) {
    return http.post("/candidate/candidateActivityLogs", model).then(response => response.data);
  },
  saveCandidateFeedbacks (model) {
    return http.post("/candidate/createCandidateFeedback", model).then(response => response.data);
  },
  updateCandidateStatus (id, statusId) {
    return http.put(`/candidate/${id}/${statusId}`, statusId).then(response => response.data);
  },
  deleteCandidate (id) {
    return http.delete(`/candidate/${id}`).then(response => response.data);
  },
  deleteCandidateActivityLogs (Activityid) {
    return http.delete(`/candidate/${Activityid}/candidateactivity`).then(response => response.data);
  },
  deleteCandidateFeedbacks (feedbackId) {
    return http.delete(`/candidate/${feedbackId}/deleteCandidateFeedback`).then(response => response.data);
  }
};
