import { defineStore } from "pinia";
import { LocalStorage } from "quasar";
import { http } from "boot/axios";

// Held in memory only and never saved, so editing browser storage can't change it. This is for
// usability: every request is still authorized by the API.
export const usePermissionStore = defineStore("permissions", {
  state: () => ({
    loadedFor: null,
    loadFailed: false,
    isSiteMember: true,
    isSiteAdmin: false,
    isSystemAdmin: false,
    mode: "Audit",
    siteGateMode: "Audit",
    enforceAllLinks: false,
    enforcedLinks: [],
    menus: {},
    deniedPath: null
  }),

  actions: {
    async load (force = false) {
      const user = LocalStorage.getItem("user");
      const key = `${user?.userId}:${user?.siteId}`;
      if (!force && this.loadedFor === key) return;

      try {
        const { data } = await http.get("/authorization/my-permissions");

        this.isSiteMember = data.isSiteMember;
        this.isSiteAdmin = data.isSiteAdmin;
        this.isSystemAdmin = data.isSystemAdmin;
        this.mode = data.mode;
        this.siteGateMode = data.siteGateMode;
        this.enforceAllLinks = data.enforceAllLinks;
        this.enforcedLinks = data.enforcedLinks || [];
        this.menus = Object.fromEntries((data.menus || []).map(menu => [menu.link, menu]));
        this.loadedFor = key;
        this.loadFailed = false;
      } catch (error) {
        this.loadedFor = null;
        this.loadFailed = true;
        throw error;
      }
    },

    reset () {
      this.$reset();
    },

    // Matches the API's rule: any listed menu can grant the level, and a missing menu grants nothing.
    allows (links, level = "view") {
      return [].concat(links).some((link) => {
        const menu = this.menus[normalizeLink(link)];
        return !!menu && (level === "manage" ? menu.manage : menu.view);
      });
    },

    // In Audit mode the API allows everything, so Vue must not block anything either.
    isEnforced (links) {
      if (this.mode !== "Enforce") return false;
      if (this.enforceAllLinks) return true;
      return [].concat(links).some(link => this.enforcedLinks.includes(normalizeLink(link)));
    },

    can (links, level = "view") {
      return !this.isEnforced(links) || this.allows(links, level);
    }
  }
});

function normalizeLink (link) {
  return String(link).trim().split(/[?#]/)[0].replace(/\/+$/, "").toLowerCase();
}
