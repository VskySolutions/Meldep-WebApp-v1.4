import { LocalStorage } from "quasar";

// Per-page UI preferences (visible columns, column order/widths, page size) kept in LocalStorage — the same
// store the rest of Meldep uses, wiped by auth.logout() with everything else. Scoped by the active site so
// a user who works in two tenants keeps a column layout per tenant, like useSiteTableState does.
const siteId = () => LocalStorage.getItem("user")?.siteId || "default";

export function usePreferences (pageKey) {
  const storageKey = () => `pref:${siteId()}:${pageKey}`;

  const all = () => {
    const raw = LocalStorage.getItem(storageKey());
    return raw && typeof raw === "object" ? raw : {};
  };

  const writeAll = (prefs) => LocalStorage.set(storageKey(), prefs);

  const get = (key, fallback = null) => {
    const prefs = all();
    return Object.prototype.hasOwnProperty.call(prefs, key) ? prefs[key] : fallback;
  };

  const set = (key, value) => {
    const prefs = all();
    prefs[key] = value;
    writeAll(prefs);
  };

  const merge = (partial) => writeAll({ ...all(), ...partial });

  const remove = (key) => {
    const prefs = all();
    delete prefs[key];
    writeAll(prefs);
  };

  const clear = () => LocalStorage.remove(storageKey());

  return { all, get, set, merge, remove, clear };
}
