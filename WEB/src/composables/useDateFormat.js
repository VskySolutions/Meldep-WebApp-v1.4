// Calendar-date helpers. Meldep's API exchanges calendar dates as "MM/DD/YYYY" strings (the *Str fields)
// and renders timestamps server-side in the site's time zone, so nothing here goes through a Date object:
// "2026-12-31" parsed as a date is UTC midnight and reads back a day early anywhere west of Greenwich.

const ISO_RE = /^(\d{4})-(\d{2})-(\d{2})/;
const DISPLAY_RE = /^(\d{2})\/(\d{2})\/(\d{4})$/;

/** "YYYY-MM-DD" → "MM/DD/YYYY" (string-reformatted). */
export function formatDateOnly (value, placeholder = "—") {
  if (!value) return placeholder;
  const m = ISO_RE.exec(String(value));
  return m ? `${m[2]}/${m[3]}/${m[1]}` : String(value);
}

/** "MM/DD/YYYY" → "YYYY-MM-DD", or null when the text is not a real calendar date. */
export function toIsoDate (text) {
  const m = DISPLAY_RE.exec(String(text || "").trim());
  if (!m) return null;
  const [, mm, dd, yyyy] = m;
  const month = Number(mm);
  const day = Number(dd);
  if (month < 1 || month > 12 || day < 1) return null;
  // Day 0 of the NEXT month is the last day of this one — the only leap-year rule worth writing.
  const lastDay = new Date(Date.UTC(Number(yyyy), month, 0)).getUTCDate();
  return day > lastDay ? null : `${yyyy}-${mm}-${dd}`;
}

/** True for a value already in "YYYY-MM-DD" form. */
export function isIsoDate (value) {
  return ISO_RE.test(String(value || ""));
}

/** Today as "YYYY-MM-DD" in the browser's calendar. */
export function todayIso () {
  const d = new Date();
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const dd = String(d.getDate()).padStart(2, "0");
  return `${d.getFullYear()}-${mm}-${dd}`;
}
