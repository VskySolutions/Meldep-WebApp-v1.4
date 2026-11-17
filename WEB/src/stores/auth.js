import { defineStore } from "pinia";
import { LocalStorage } from "quasar";
import { http, pinUser } from "boot/axios";
import authService from "modules/auth/auth.service";
import { usePermissionStore } from "stores/permissions";

// Maps an auth/user API response to the user object stored in state & LocalStorage
function mapUser(resp) {
  return {
    employeeId: resp.employeeId,
    userId: resp.userId,
    personId: resp.personId,
    username: resp.username,
    firstName: resp.firstName,
    lastName: resp.lastName,
    email: resp.email,
    userEmail: resp.userEmail,
    roles: resp.roles,
    siteId: resp.siteId,
    siteName: resp.siteName,
    siteTimeZone: resp.siteTimeZone,
    siteLandingPageLink: resp.siteLandingPageLink,
    isMsLogin: resp.isMsLogin,
    isFileUploadOrExternal: resp.isFileUploadOrExternal
  };
}

export const useAuthStore = defineStore("auth", {

  state: () => ({
    token: LocalStorage.getItem("token"),
    user: LocalStorage.getItem("user")
  }),

  getters: {
    loggedIn: (state) => !!state.token
  },

  actions: {
    async login(model) {
      try {
        const resp = await authService.login(model);
        if (resp.token) {
          LocalStorage.clear();
          const token = resp.token;
          const user = mapUser(resp);

          this.token = token;
          this.user = user;

          LocalStorage.set("token", token);
          LocalStorage.set("user", user);
          pinUser(user);

          return resp;
        }

      } catch (error) {
        console.error("Login failed:", error);
        throw error;
      }
    },

    async mslogin(model) {
      try {
        const resp = await authService.mslogin(model);
        if (resp.loginErrorMessage) {
          return resp;
        }

        if (resp.token) {
          LocalStorage.clear();

          const token = resp.token;
          const user = mapUser(resp);

          this.token = token;
          this.user = user;

          LocalStorage.set("token", token);
          LocalStorage.set("user", user);
          pinUser(user);

          return resp;
        }

      } catch (error) {
        console.error("MS Login failed:", error);
        throw error;
      }
    },

    mslogout () {
      return new Promise((resolve, reject) => {
        (async () => {
          try {
            // Dynamically import the MSAL instance
            const { msalInstancePromise } = await import("modules/auth/auth.service");
            const msalInstance = await msalInstancePromise; // Ensure the promise resolves
            const accounts = msalInstance.getAllAccounts();
            accounts.forEach(account => {
              // msalInstance.removeAccount(account);
              // msalInstance.getTokenCache().removeAccount(account);
              // msalInstance.logout({ account });
              msalInstance.clearCache(account);
            });
            msalInstance.clearCache();

            // Clear the session first: logoutRedirect leaves the page, so code after it may not run
            LocalStorage.clear();
            this.token = null;
            this.user = null;
            this.session = null;
            pinUser(null);

            // Remove authorization header
            delete http.defaults.headers.common.Authorization;

            // // Perform logout using redirect
            await msalInstance.logoutRedirect({
              postLogoutRedirectUri: process.env.MS_Logout_BASE_URL // Ensure this matches your MSAL config
            });
            await msalInstance.logout();

            // Resolve the promise to indicate successful logout
            resolve();
          } catch (error) {
            console.error("Logout failed:", error);
            reject(error); // Reject the promise to indicate logout failure
          }
        })();
      });
    },

    logout() {
      LocalStorage.clear();
      this.token = null;
      this.user = null;
      pinUser(null);
      usePermissionStore(this.$pinia).reset();
      delete http.defaults.headers.common.Authorization;
    },

    setUserInfo(payload) {
      this.user = {
        ...this.user,
        ...payload
      };
      LocalStorage.set("user", this.user);
      pinUser(this.user);
    },

    // 🔹 NEW FUNCTION: Store JWT Token from URL
    setTokenFromUrl (token) {
      if (token) {
        this.setToken(token);
      }
    },

    // 🔹 Helper function to store token in LocalStorage & set auth header
    setToken (token) {
      LocalStorage.set("token", token);
      this.token = token;
    },

    async getUserByToken (token) {
      try {
        const resp = await authService.getUserByToken(token);
        if (resp.status === 401 || resp.status === 400) {
          console.error("Unauthorized access. Redirecting to login.");
          this.logout();
          window.location.href = "/auth/login";
          return;
        }

        const user = mapUser(resp);

        this.token = token;
        this.user = user;

        LocalStorage.set("token", token);
        LocalStorage.set("user", user);
        pinUser(user);
      } catch (error) {
        console.error("Error fetching user:", error);

        // The token was stored before it was checked, so don't keep it after a failure
        this.logout();

        if (error.response && (error.response.status === 401 || error.response.status === 400)) {
          console.error("Unauthorized access. Redirecting to login.");
          window.location.href = "/auth/login";
        }
      }
    },

    changeTenant(siteId, siteTimeZone, name, landingPage, roles, isFileUploadOrExternal) {
      const updatedUser = {
        ...this.user,
        siteId: siteId,
        siteTimeZone: siteTimeZone,
        siteName: name,
        siteLandingPageLink: landingPage,
        roles: roles,
        isFileUploadOrExternal: isFileUploadOrExternal
      };
      this.user = updatedUser;
      LocalStorage.set("user", updatedUser);
      pinUser(updatedUser);
    },

    // The server validates the target site and updates the last-used site in one step.
    async switchSite(siteId) {
      const { data } = await http.post("/authorization/switch-site", { siteId });

      this.changeTenant(data.siteId, data.timeZone, data.name, data.landingPage, data.roles, data.isFileUploadOrExternal);

      // last_route belongs to the previous site and may not be available in this one
      localStorage.setItem("last_route", data.landingPage || "/dashboard");

      return data;
    }
  }
});
