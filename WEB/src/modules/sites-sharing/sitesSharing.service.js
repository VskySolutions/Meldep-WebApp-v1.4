import { http } from "boot/axios";

export default {
  getAllSiteShares (model) {
    return http.post("/person-site-mapping/list", model).then(response => response.data);
  },

  getAllSharedSitesByLoggedUserId () {
    return http.get("/person-site-mapping/my-share-sites-list").then(response => response.data);
  },

  updateLastUsedSite(userId, personId, siteId) {
    return http.post(
      `/person-site-mapping/update-last-used-site?userId=${userId}&personId=${personId}&siteId=${siteId}`
    ).then(response => response.data);
  },

  saveSiteShare (model) {
    // return http.post(`/person-site-mapping/save-site-share-user?email=${email}`).then(response => response.data);
    return http.post("/person-site-mapping/save-site-share-user", model).then(response => response.data);
  },

  deleteSiteShare (id) {
    return http.delete(`/person-site-mapping/${id}`).then(response => response.data);
  }
};
