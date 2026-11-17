// Lets Node load the web app's real router guard, permission store and route files unchanged.
// Only the framework imports (pinia, quasar, boot/axios) are replaced, by the stubs in this folder.
import { registerHooks } from "node:module";
import { pathToFileURL, fileURLToPath } from "node:url";
import path from "node:path";

const here = path.dirname(fileURLToPath(import.meta.url));
const src = path.resolve(here, "../../../WEB/src");
const stubs = { pinia: "stubs.mjs", quasar: "stubs.mjs", "boot/axios": "stubs.mjs" };

registerHooks({
  resolve (specifier, context, next) {
    if (stubs[specifier]) return { url: pathToFileURL(path.join(here, stubs[specifier])).href, shortCircuit: true };
    const alias = ["stores/", "modules/", "router/"].find(prefix => specifier.startsWith(prefix));
    if (alias) {
      const file = path.join(src, specifier.endsWith(".js") ? specifier : specifier + ".js");
      return { url: pathToFileURL(file).href, format: "module", shortCircuit: true };
    }
    return next(specifier, context);
  },
  load (url, context, next) {
    // WEB/src has no package.json "type": "module", so mark its .js files as ES modules explicitly.
    if (url.includes("/WEB/src/") && url.endsWith(".js")) return { ...next(url, { ...context, format: "module" }), format: "module" };
    return next(url, context);
  }
});
