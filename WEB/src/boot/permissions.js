import { boot } from "quasar/wrappers";
import { usePermissionStore } from "stores/permissions";

// v-can hides an action unless the user has Manage on the current page's menu.
// v-can="'/test-case'" names the menu, and { menu, level } can require a different level.
export default boot(({ app, store, router }) => {
  function update (el, binding) {
    const value = binding.value;
    const isRule = value && typeof value === "object" && !Array.isArray(value);
    const menu = (isRule ? value.menu : value)
      ?? router.currentRoute.value.matched.flatMap(record => record.meta.permission?.menu || []);
    const level = (isRule && value.level) || "manage";

    // A page with no menu rule has nothing to check against, so its actions stay visible.
    const allowed = [].concat(menu).length === 0 || usePermissionStore(store).can(menu, level);
    el.style.display = allowed ? "" : "none";
  }

  app.directive("can", { mounted: update, updated: update });
});
