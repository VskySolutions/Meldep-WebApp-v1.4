import { boot } from "quasar/wrappers";
import { LocalStorage } from "quasar";
import { notifyError } from "assets/utils";
import { http2, http } from "boot/axios";
import { useAuthStore } from "stores/auth";
import { usePermissionStore } from "stores/permissions";
import { isRouteAllowed } from "src/router/permissionGuard";

// One denied page can fire many requests at once, so show one message and start one recovery.
const DENIED_TOAST_INTERVAL_MS = 3000;
let lastDeniedToastAt = 0;
let handlingSessionEnd = false;
let recoveringSite = false;
let renewingSession = false;

export default boot(({ store, router }) => {
  // request http2 axios interceptors (anonymous request)
  http2.interceptors.request.use(
    (config) => {
      return config;
    },
    (error) => {
      notifyError({
        timeout: 10000,
        message: getErrorMessage(error)
      });
      return Promise.reject(error);
    }
  );

  // response http2 axios interceptors (anonymous response)
  http2.interceptors.response.use(
    (response) => {
      return response;
    },
    (error) => {
      notifyError({
        timeout: 10000,
        message: getErrorMessage(error)
      });
      return Promise.reject(error);
    }
  );

  // request http axios interceptors
  http.interceptors.request.use(
    (config) => {
      return config;
    },
    (error) => {
      notifyError({
        timeout: 10000,
        message: getErrorMessage(error)
      });
      return Promise.reject(error);
    }
  );

  // response http axios interceptors
  http.interceptors.response.use(
    (response) => {
      renewSessionIfDue();
      return response;
    },
    (error) => {
      const status = error.response?.status;

      if (status === 401) {
        handleSessionEnd();
      } else if (status === 403) {
        handleForbidden(error);
      } else {
        notifyError({
          timeout: 10000,
          message: getErrorMessage(error)
        });
      }
      return Promise.reject(error);
    }
  );

  // Active users stay signed in: once two thirds of the token's life has passed, get a new one.
  async function renewSessionIfDue () {
    if (renewingSession) return;

    const token = LocalStorage.getItem("token");
    const claims = readTokenClaims(token);
    if (!claims?.iat || !claims?.exp) return;

    const now = Date.now() / 1000;
    if (now < claims.iat + (claims.exp - claims.iat) * 2 / 3) return;

    renewingSession = true;
    try {
      const { data } = await http.post("/auth/refresh");
      // Another tab or a logout may have changed the session while this was running.
      if (data?.token && LocalStorage.getItem("token") === token) {
        useAuthStore(store).setToken(data.token);
      }
    } catch {
      // The current token still works. A real expiry is handled by the 401 path.
    } finally {
      renewingSession = false;
    }
  }

  function readTokenClaims (token) {
    try {
      return JSON.parse(atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
    } catch {
      return null;
    }
  }

  function handleSessionEnd () {
    if (handlingSessionEnd) return;
    handlingSessionEnd = true;

    const currentPath = router.currentRoute.value.fullPath;
    useAuthStore(store).logout();

    router.push({ name: "login", query: { redirect: currentPath } }).finally(() => {
      handlingSessionEnd = false;
    });
  }

  function handleForbidden (error) {
    if (error.response.data?.code === "SITE_NOT_MEMBER") {
      recoverSite();
      return;
    }

    // These explain which rule stopped an admin's change, so the admin needs the exact message.
    if (error.response.data?.code === "SECURITY_ADMIN_DENIED") {
      notifyError({ message: error.response.data.message });
      return;
    }

    // Stays on the page: one denied widget or button must not eject the user from it.
    // Whether the page itself is allowed is decided by the route guard.
    const now = Date.now();
    if (now - lastDeniedToastAt >= DENIED_TOAST_INTERVAL_MS) {
      lastDeniedToastAt = now;
      notifyError({ timeout: 5000, message: getErrorMessage(error) });
      refreshPermissions();
    }
  }

  // Permissions may have just been removed. Leave the page only if the page itself is now denied.
  async function refreshPermissions () {
    const permissions = usePermissionStore(store);
    try {
      await permissions.load(true);
    } catch {
      return;
    }

    const route = router.currentRoute.value;
    if (!route.matched.some(record => record.meta.public) && !isRouteAllowed(route, permissions)) {
      window.location.reload();
    }
  }

  // The active site is no longer valid: move to another site, or explain there is none.
  async function recoverSite () {
    if (recoveringSite) return;
    recoveringSite = true;

    try {
      const { data: sites } = await http.get("/authorization/my-sites");
      if (sites.length === 0) {
        await router.push({ name: "no_site_access" });
        recoveringSite = false;
        return;
      }

      const site = await useAuthStore(store).switchSite(sites[0].siteId);
      window.location.assign(site.landingPage || "/dashboard");
    } catch {
      await router.push({ name: "no_site_access" });
      recoveringSite = false;
    }
  }

  function getErrorMessage (error) {
    const errMgs = error?.response?.data?.message || (typeof error?.response?.data === "string" && error.response.data) || "An error occurred while processing your request.";
    console.error("Request Error:- " + errMgs);
    return errMgs;
  }
});
