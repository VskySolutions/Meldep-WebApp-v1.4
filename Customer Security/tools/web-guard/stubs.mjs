// Stand-ins for pinia, quasar's LocalStorage and boot/axios, with the same behaviour the guard relies on.
export const API = process.env.API_URL || "http://localhost:5086";

const storage = new Map();
export const LocalStorage = {
  getItem: key => (storage.has(key) ? storage.get(key) : null),
  set: (key, value) => storage.set(key, value),
  remove: key => storage.delete(key),
  clear: () => storage.clear()
};

// One store instance per "page load". newPageLoad() is a browser refresh or a URL typed into the address bar.
let instances = new Map();
export function newPageLoad () { instances = new Map(); }
export function defineStore (id, definition) {
  return function useStore () {
    if (!instances.has(id)) {
      const store = definition.state();
      for (const [name, action] of Object.entries(definition.actions || {})) store[name] = action.bind(store);
      store.$reset = () => Object.assign(store, definition.state());
      instances.set(id, store);
    }
    return instances.get(id);
  };
}

// Same headers as boot/axios: the token and the active site from the stored user.
export const http = {
  async get (url) {
    const headers = { "Content-Type": "application/json" };
    const token = LocalStorage.getItem("token");
    const user = LocalStorage.getItem("user");
    if (token) headers.Authorization = `Bearer ${token}`;
    if (user?.siteId) headers["X-Site-Id"] = user.siteId;
    const response = await fetch(API + url, { headers });
    const data = await response.json().catch(() => null);
    if (!response.ok) {
      const error = new Error(`HTTP ${response.status}`);
      error.response = { status: response.status, data };
      throw error;
    }
    return { data };
  }
};

// The guard reads localStorage through the global too (router/index.js does).
globalThis.localStorage = { getItem: key => LocalStorage.getItem(key) };
