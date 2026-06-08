import { http2 } from "boot/axios";
import { PublicClientApplication, BrowserCacheLocation, LogLevel } from "@azure/msal-browser";

export default {
  login (model) {
    return http2.post("/auth/login", model).then(response => response.data);
  },
  mslogin (Authorization) { // Microsoft login Manish Dhuri
    return http2.post("/auth/mslogin", Authorization).then(response => response.data);
  },
  register (model) {
    return http2.post("/auth/register", model).then(response => response.data);
  },

  forgotPassword (model) {
    return http2.post("/auth/forgot-password", model).then(response => response.data);
  },

  resetPassword (model) {
    return http2.post("/auth/reset-password", model).then(response => response.data);
  },
  getUser (userid) {
    return http2.get(`/auth/user/?userid=${userid}`).then(response => response.data);
  }
};

// Logger callback function
const loggerCallback = (level, message, containsPii) => {
  if (containsPii) return;
  switch (level) {
  case LogLevel.Error:
    break;
  case LogLevel.Info:
    break;
  case LogLevel.Verbose:
    break;
  case LogLevel.Warning:
    break;
  default:
    break;
  }
};

// MSAL configuration
const msalConfig = {
  auth: {
    clientId: process.env.MS_Client_Id, // Replace with your actual Client ID
    authority: process.env.MS_Authority_Url, // Replace with your Tenant ID
    redirectUri: process.env.MS_Login_BASE_URL, // Ensure this matches your Azure AD redirect URI
    postLogoutRedirectUri: process.env.MS_Logout_BASE_URL // Redirect URI after logout
  },
  cache: {
    cacheLocation: BrowserCacheLocation.LocalStorage // Use LocalStorage for caching
  },
  system: {
    allowNativeBroker: false, // Disables WAM Broker
    loggerOptions: {
      loggerCallback,
      logLevel: LogLevel.Info,
      piiLoggingEnabled: false // Personal Identifiable Information logging
    }
  }
};

// Factory function to create the MSAL instance
export async function createMSALInstance () {
  const msalInstance = new PublicClientApplication(msalConfig);
  await msalInstance.initialize();
  msalInstance.clearCache();
  return msalInstance;
}

// Export MSAL instance for use
export const msalInstancePromise = createMSALInstance();

// Alternative logout with redirect
export async function msLoggedOutRedirect () {
  try {
    const msalInstance = await msalInstancePromise;
    await msalInstance.logoutRedirect();
    msalInstance.clearCache();
  } catch (error) {
    console.error("Logout with redirect failed:", error);
    throw error;
  }
}

export async function msLoggedOut () {
  try {
    const msalInstance = await msalInstancePromise;
    await msalInstance.logoutPopup();
    msalInstance.clearCache();
  } catch (error) {
    console.error("Logout failed:", error);
    throw error;
  }
}
