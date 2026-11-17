import { boot } from "quasar/wrappers";
import DOMPurify from "dompurify";

// v-safe-html replaces v-html. Rich text comes from users and public forms, and a script hidden in
// it could steal the login token from browser storage, so it's always sanitized before display.
export default boot(({ app }) => {
  function render (el, binding) {
    if (binding.value === binding.oldValue) return;
    el.innerHTML = DOMPurify.sanitize(binding.value ?? "", { ADD_ATTR: ["target"] });
  }

  app.directive("safe-html", { mounted: render, updated: render });
});
