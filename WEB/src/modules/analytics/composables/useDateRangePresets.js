// Date-range presets for the analytics pages. Weeks run Monday to Sunday. Every value is an ISO
// "YYYY-MM-DD" string built from local calendar parts, never through Date.toISOString(), which is UTC and
// reads back a day early west of Greenwich.

export const PRESETS = [
  { value: "lastWeek", label: "Last Week", icon: "o_history" },
  { value: "thisWeek", label: "This Week", icon: "o_today" },
  { value: "lastMonth", label: "Last Month", icon: "o_event_repeat" },
  { value: "thisMonth", label: "This Month", icon: "o_calendar_month" },
  { value: "byWeek", label: "By Weeks", icon: "o_view_week" },
  { value: "custom", label: "Custom", icon: "o_date_range" }
];

// Longest range one request may cover; the API enforces the same limit.
export const MAX_RANGE_DAYS = 93;

const DAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const ISO_RE = /^(\d{4})-(\d{2})-(\d{2})/;

const pad = (n) => String(n).padStart(2, "0");

export function toIso (date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

export function fromIso (iso) {
  const m = ISO_RE.exec(String(iso || ""));
  return m ? new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3])) : null;
}

export function todayIso () {
  return toIso(new Date());
}

export function addDays (iso, n) {
  const d = fromIso(iso);
  if (!d) return "";
  d.setDate(d.getDate() + n);
  return toIso(d);
}

/** Inclusive day count between two ISO dates; 0 or negative when "to" is before "from". */
export function daysBetween (fromDate, toDate) {
  const a = fromIso(fromDate);
  const b = fromIso(toDate);
  if (!a || !b) return 0;
  return Math.round((b - a) / 86400000) + 1;
}

/** The Sunday that ends the week containing the date (a Sunday is its own week end). */
export function sundayOf (iso) {
  const d = fromIso(iso);
  return d ? addDays(iso, (7 - d.getDay()) % 7) : "";
}

/** Monday to Sunday of the week that ends on the given Sunday. */
export function weekRangeEnding (sundayIso) {
  const end = sundayOf(sundayIso);
  return end ? { from: addDays(end, -6), to: end } : null;
}

/** { from, to } for a preset, or null for "byWeek" (needs a Sunday), "custom" and unknown values. */
export function rangeForPreset (preset, today = todayIso()) {
  const d = fromIso(today);
  if (!d) return null;
  const mondayOffset = (d.getDay() + 6) % 7; // Monday = 0 ... Sunday = 6

  switch (preset) {
    case "thisWeek": {
      const from = addDays(today, -mondayOffset);
      return { from, to: addDays(from, 6) };
    }
    case "lastWeek": {
      const from = addDays(today, -mondayOffset - 7);
      return { from, to: addDays(from, 6) };
    }
    case "thisMonth": {
      const from = toIso(new Date(d.getFullYear(), d.getMonth(), 1));
      const to = toIso(new Date(d.getFullYear(), d.getMonth() + 1, 0));
      return { from, to };
    }
    case "lastMonth": {
      const from = toIso(new Date(d.getFullYear(), d.getMonth() - 1, 1));
      const to = toIso(new Date(d.getFullYear(), d.getMonth(), 0));
      return { from, to };
    }
    default:
      return null;
  }
}

/** "2026-11-08" → "11/08/2026", the date format used across the application. */
export function formatAppDate (iso) {
  const m = ISO_RE.exec(String(iso || ""));
  return m ? `${m[2]}/${m[3]}/${m[1]}` : "";
}

/** "Mon 11/02/2026": the weekday in front of the application's date format. */
export function formatDayLabel (iso) {
  const d = fromIso(iso);
  return d ? `${DAYS[d.getDay()]} ${formatAppDate(iso)}` : "";
}

/** "11/02/2026 – 11/08/2026 · 7 days" */
export function describeRange (fromDate, toDate) {
  const a = formatAppDate(fromDate);
  const b = formatAppDate(toDate);
  if (!a || !b) return "";
  const n = daysBetween(fromDate, toDate);
  return `${a} – ${b} · ${n} ${n === 1 ? "day" : "days"}`;
}

/**
 * "2026-11-08T10:12" → "11/08/2026 10:12 AM": the timestamp format the API renders everywhere in the
 * application. The value is already site-local text, so no time-zone maths happens here.
 */
export function formatLocalMinute (value) {
  if (!value) return "";
  const m = /^(\d{4})-(\d{2})-(\d{2})(?:[T ](\d{2}):(\d{2}))?/.exec(String(value));
  if (!m) return String(value);
  const [, y, mo, d, hh, mi] = m;
  const date = `${mo}/${d}/${y}`;
  if (hh === undefined) return date;
  const h = Number(hh);
  const h12 = h % 12 === 0 ? 12 : h % 12;
  return `${date} ${String(h12).padStart(2, "0")}:${mi} ${h >= 12 ? "PM" : "AM"}`;
}

export function useDateRangePresets () {
  return { PRESETS, MAX_RANGE_DAYS, todayIso, addDays, daysBetween, rangeForPreset, sundayOf, weekRangeEnding, formatAppDate, formatDayLabel, describeRange, formatLocalMinute };
}
