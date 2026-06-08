import { http } from "boot/axios";

export default {
  saveWebsiteDemo (model) {
    return http.post("/websitedemos", model).then(response => response.data);
  },

  // contact-us
  getAllContactUs (model) {
    return http.post("/contact/contact-us-list", model).then(response => response.data);
  },
  saveContactUs (model) {
    return http.post("/contact/save-contact-us", model).then(response => response.data);
  }
};
