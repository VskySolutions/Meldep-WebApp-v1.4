import { boot } from "quasar/wrappers";
import { LocalStorage } from "quasar";
import { useAuthStore } from "stores/auth";

export default boot(({ router, store }) => {
  router.beforeEach(async (to, from, next) => {
    // console.log("boot.auth.to", to);
    // console.log("boot.auth.from", from);
    // console.log("boot.auth.next", next);
    if (to.matched.some(record => record.meta.requiresAuth)) {
      // Token from URL (cross-site SSO) is captured in the router guard
      // (src/router/index.js) before this runs, so the token is already
      // persisted — no need to wait/poll for it here.
      const token = LocalStorage.getItem("token");
      // console.log("🔍 boot.auth.token", token);

      if (!token) {
        next({ name: "login", query: { redirect: to.fullPath } }); // 🔹 Store original URL
      } else {
        const authStore = useAuthStore(store);
        // console.log("🔍 boot.auth.authStore", authStore);
        const user = authStore.user;
        // console.log("👤 boot.auth.user", user);
        const administrator = user?.roles?.includes("superadmin") ?? false;

        if (to.matched.some(record => record.meta.requiresAdmin) && !administrator) {
          // console.log("🚫 boot.auth.not_authorized");
          next({ name: "not_authorized" });
        } else {
          // console.log("👤 boot.auth.user.next", to.fullPath);
          next(); // Keep as is, should not redirect to "/"
        }
      }
    } else {
      // console.log("✅ boot.auth.next");
      next();
    }
  });
});
