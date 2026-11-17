import { boot } from "quasar/wrappers";
import axios from "axios";
import { LocalStorage } from "quasar";

// Token based axios instance
const http = axios.create({
  baseURL: process.env.API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json"
  }
});

// Anonymous axios instance
const http2 = axios.create({
  baseURL: process.env.API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json"
  }
});

// This tab's user and site, held in memory. LocalStorage is shared by every tab, so reading the
// site from it per request would let a switch in one tab silently redirect another tab's writes.
let pinnedUser = null;
let reloadTimer = null;

export function pinUser (user) {
  pinnedUser = user || null;
}

function watchOtherTabs () {
  window.addEventListener("storage", (event) => {
    // key is null when another tab calls LocalStorage.clear()
    if (event.key !== null && event.key !== "user" && event.key !== "token") return;

    // Login clears and then rewrites storage, so wait for the final state before comparing.
    clearTimeout(reloadTimer);
    reloadTimer = setTimeout(() => {
      const storedUser = LocalStorage.getItem("user");
      const sessionEnded = !LocalStorage.getItem("token");
      const userChanged = storedUser?.userId !== pinnedUser?.userId;
      const siteChanged = storedUser?.siteId !== pinnedUser?.siteId;

      if (sessionEnded || userChanged || siteChanged) {
        window.location.reload();
      }
    }, 200);
  });
}

export default boot(({ app }) => {
  pinUser(LocalStorage.getItem("user"));
  watchOtherTabs();

  // Request interceptor
  http.interceptors.request.use(
    (config) => {

      const token = LocalStorage.getItem("token");
      const user = pinnedUser;

      // Authorization
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      } else {
        delete config.headers.Authorization;
      }

      // Tenant Header
      if (user?.siteId) {
        config.headers["X-Site-Id"] = user.siteId;
      }

      // Tenant Name
      if (user?.siteName) {
        config.headers["X-Site-Name"] = user.siteName;
      }

      // Landing Page
      if (user?.siteLandingPageLink) {
        config.headers["X-Site-LandingPage"] = user.siteLandingPageLink;
      }

      // Timezone Header
      if (user?.siteTimeZone) {
        config.headers["X-Site-Timezone"] = user.siteTimeZone;
      }

      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );

  app.config.globalProperties.$axios = axios;
  app.config.globalProperties.$http2 = http2;
  app.config.globalProperties.$http = http;
});

export { axios, http, http2 };
