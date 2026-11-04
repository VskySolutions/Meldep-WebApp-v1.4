import { formatDayLabel, formatLocalMinute } from "modules/analytics/composables/useDateRangePresets";

// The vocabulary of the Timesheet Register: one entry per cell state the API returns. Every state carries an
// icon or glyph AND a label, so a cell is never read by colour alone (the colours are the fixed status palette:
// good / warning / serious / critical, plus the brand blue for "due" and neutrals for non-working days).
export const STATES = {
  OnTime: {
    key: "OnTime",
    label: "On time",
    icon: "o_check_circle",
    glyph: "",
    colour: "#0ca30c",
    description: "The expected hours were entered by the cutoff (shift end plus grace)."
  },
  Delayed: {
    key: "Delayed",
    label: "Delayed",
    icon: "o_schedule",
    glyph: "",
    colour: "#a66f00",
    description: "The expected hours were reached, but only after the cutoff. The tooltip and the day panel show how late."
  },
  Partial: {
    key: "Partial",
    label: "Partial",
    icon: "o_donut_large",
    glyph: "",
    colour: "#d9541e",
    description: "Some hours were logged, fewer than expected, and the cutoff has passed."
  },
  Pending: {
    key: "Pending",
    label: "Pending",
    icon: "o_cancel",
    glyph: "",
    colour: "#d03b3b",
    description: "Nothing was logged and the cutoff has passed."
  },
  Due: {
    key: "Due",
    label: "Due",
    icon: "o_hourglass_empty",
    glyph: "",
    colour: "#1b75ab",
    description: "The expected hours are not reached yet, but the cutoff has not passed either (usually today)."
  },
  Leave: {
    key: "Leave",
    label: "Leave",
    icon: "",
    glyph: "L",
    colour: "#6f42c1",
    description: "A full-day leave covers the date: applied, forwarded or approved. Nothing is expected."
  },
  Holiday: {
    key: "Holiday",
    label: "Holiday",
    icon: "",
    glyph: "H",
    colour: "#898781",
    description: "Sunday, or a date in the site's leave schedule (public holidays, Saturdays off)."
  },
  Future: {
    key: "Future",
    label: "Future",
    icon: "",
    glyph: "–",
    colour: "#b0b4b8",
    description: "A day that has not come yet."
  },
  NotEmployed: {
    key: "NotEmployed",
    label: "Not employed",
    icon: "",
    glyph: "",
    colour: "#c8ccd0",
    description: "Before the employee's joining date or after the release date."
  }
};

// The order the legend and the state filter list them in.
export const STATE_ORDER = ["OnTime", "Delayed", "Partial", "Pending", "Due", "Leave", "Holiday", "Future", "NotEmployed"];

// States a reader can filter employees by (a row qualifies when any of its days is in the state).
export const FILTERABLE_STATES = ["OnTime", "Delayed", "Partial", "Pending", "Due", "Leave"];

// States that count as working days the employee was expected to fill.
export const EXPECTED_STATES = ["OnTime", "Delayed", "Partial", "Pending", "Due"];

export const FULL_DAY_LEAVE = "Full";

// The period-total columns of the grid: short form in the header, full form + description in its tooltip.
// The same `value` reads a row's totals and the footer's totals.
export const TOTAL_COLUMNS = [
  {
    key: "hours",
    abbr: "Hrs",
    full: "Logged hours",
    desc: "Hours entered in the period across all days, including hours logged on holidays and leave days.",
    cls: "",
    value: (t) => fmtHours(t.loggedHours)
  },
  {
    key: "onTime",
    abbr: "OT",
    full: "On time",
    desc: "Days whose expected hours were completed by the cutoff (shift end plus grace).",
    cls: "tsr-td--ok",
    value: (t) => t.onTime || ""
  },
  {
    key: "delayed",
    abbr: "Dly",
    full: "Delayed",
    desc: "Days whose expected hours were reached only after the cutoff.",
    cls: "tsr-td--dly",
    value: (t) => t.delayed || ""
  },
  {
    key: "partial",
    abbr: "Par",
    full: "Partial",
    desc: "Days with some hours logged but fewer than expected once the cutoff had passed.",
    cls: "tsr-td--par",
    value: (t) => t.partial || ""
  },
  {
    key: "pending",
    abbr: "Pnd",
    full: "Pending",
    desc: "Working days with nothing logged once the cutoff had passed.",
    cls: "tsr-td--pnd",
    value: (t) => t.pending || ""
  }
];

export function stateOf (cell) {
  return STATES[cell?.state] || STATES.Future;
}

/** "8", "8.5", "0.25" — hours without trailing zeros. */
export function fmtHours (value) {
  const n = Number(value || 0);
  if (!Number.isFinite(n)) return "0";
  const rounded = Math.round(n * 100) / 100;
  return Number.isInteger(rounded) ? String(rounded) : String(rounded).replace(/0+$/, "");
}

/** Minutes → "2 d 3 h", "3 h 20 m", "45 m". */
export function fmtLate (minutes) {
  const m = Math.max(0, Math.round(Number(minutes || 0)));
  if (!m) return "";
  const days = Math.floor(m / 1440);
  const hours = Math.floor((m % 1440) / 60);
  const mins = m % 60;
  const parts = [];
  if (days) parts.push(`${days} d`);
  if (hours) parts.push(`${hours} h`);
  if (!days && mins) parts.push(`${mins} m`);
  return parts.join(" ");
}

/** The shift a cell was judged with: its own when a designation change made it differ, else the row's. */
export function shiftOf (cell, row) {
  return {
    shift: cell?.shift ?? row?.shift ?? "",
    assigned: cell?.shiftAssigned ?? row?.shiftAssigned ?? false
  };
}

export function isHalfDay (cell) {
  return !!cell?.leave && cell.leave.kind !== FULL_DAY_LEAVE;
}

/** Short text shown inside a cell in the "hours" density. */
export function cellText (cell) {
  const state = cell?.state;
  const logged = Number(cell?.logged || 0);
  switch (state) {
    case "OnTime":
    case "Delayed":
    case "Partial":
      return fmtHours(logged);
    case "Due":
      return logged > 0 ? fmtHours(logged) : "due";
    case "Pending":
      return "—";
    default:
      return "";
  }
}

/** Hours logged on a day where nothing was expected (holiday, leave, future): shown as a small "+8" hint. */
export function cellExtra (cell) {
  const state = cell?.state;
  const logged = Number(cell?.logged || 0);
  if (logged > 0 && (state === "Holiday" || state === "Leave" || state === "Future")) return `+${fmtHours(logged)}`;
  return "";
}

/** One sentence that explains the state of this cell, for tooltips and the day panel. */
export function explainCell (cell, day = null) {
  const s = stateOf(cell);
  const expected = fmtHours(cell?.expected);
  const logged = fmtHours(cell?.logged);
  switch (cell?.state) {
    case "OnTime":
      return `${logged} h logged; the ${expected} h expected were completed by the cutoff${cell.preFilled ? " (entered before the day started)" : ""}.`;
    case "Delayed":
      return `${logged} h logged; the ${expected} h expected were completed ${fmtLate(cell.lateMinutes) || "shortly"} after the cutoff.`;
    case "Partial":
      return `Only ${logged} h of the ${expected} h expected were logged and the cutoff has passed.`;
    case "Pending":
      return `Nothing logged for the ${expected} h expected and the cutoff has passed.`;
    case "Due":
      return logged ? `${logged} h of ${expected} h logged so far; the cutoff has not passed yet.` : `Nothing logged yet; the cutoff has not passed.`;
    case "Leave":
      return `Full-day leave (${cell.leave?.status || "applied"}${cell.leave?.category ? ", " + cell.leave.category : ""}); nothing expected.`;
    case "Holiday":
      return day?.holidayTitle || "Holiday";
    case "Future":
      return cell.logged > 0 ? `Not yet due; ${logged} h already entered.` : "Not yet due.";
    case "NotEmployed":
      return "Outside the employment period (before joining or after release).";
    default:
      return "";
  }
}

/** The multi-line tooltip of a cell (native title: zero cost for thousands of cells). */
export function cellTitle (row, day, cell) {
  if (!cell) return "";
  const lines = [];
  lines.push(`${row?.fullName || ""} · ${formatDayLabel(day?.date)}`.trim());
  const half = isHalfDay(cell) ? ` · half-day leave (${cell.leave.kind}, ${cell.leave.status || "applied"})` : "";
  lines.push(`${stateOf(cell).label}${half}`);
  lines.push(explainCell(cell, day));
  if (cell.cutoff && EXPECTED_STATES.includes(cell.state)) {
    const { shift, assigned } = shiftOf(cell, row);
    lines.push(`Cutoff ${formatLocalMinute(cell.cutoff)} (${assigned ? shift : shift + ", not assigned"})`);
  }
  if (cell.completedAt && (cell.state === "OnTime" || cell.state === "Delayed")) lines.push(`Completed ${formatLocalMinute(cell.completedAt)}`);
  else if (cell.firstEntryAt && cell.logged > 0) lines.push(`First entry ${formatLocalMinute(cell.firstEntryAt)}`);
  if (cell.timesheetStatus) lines.push(`Week status: ${cell.timesheetStatus}`);
  return lines.filter(Boolean).join("\n");
}

/** What the grid's hover card shows for a cell: full state name, who/when, the explanation and the facts. */
export function cellHover (row, day, cell) {
  const s = stateOf(cell);
  const facts = [];
  const working = EXPECTED_STATES.includes(cell.state);
  const logged = Number(cell.logged || 0);

  if (working || logged > 0) {
    const lines = cell.lines ? ` in ${cell.lines} ${cell.lines === 1 ? "line" : "lines"}` : "";
    const expected = working ? ` of ${fmtHours(cell.expected)} h expected` : "";
    facts.push(`Logged ${fmtHours(logged)} h${lines}${expected}`);
  }
  if (cell.cutoff && working) {
    const { shift, assigned } = shiftOf(cell, row);
    facts.push(`Cutoff ${formatLocalMinute(cell.cutoff)} · ${assigned ? shift : `${shift}, not assigned (default)`}`);
  }
  if (cell.completedAt && (cell.state === "OnTime" || cell.state === "Delayed")) {
    facts.push(`Reached the expected hours ${formatLocalMinute(cell.completedAt)}${cell.preFilled ? " (before the day)" : ""}`);
  } else if (cell.firstEntryAt && logged > 0) {
    facts.push(`First entry ${formatLocalMinute(cell.firstEntryAt)}`);
  }
  if (cell.state === "Delayed" && cell.lateMinutes) facts.push(`Late by ${fmtLate(cell.lateMinutes)}`);
  if (cell.leave) {
    facts.push(`Leave: ${cell.leave.kind}${cell.leave.status ? ` · ${cell.leave.status}` : ""}${cell.leave.category ? ` · ${cell.leave.category}` : ""}`);
  }
  if (cell.timesheetStatus) facts.push(`Week status: ${cell.timesheetStatus}`);

  return {
    colour: s.colour,
    icon: s.icon,
    glyph: s.glyph,
    title: `${s.label}${isHalfDay(cell) ? " · half-day leave" : ""}`,
    subtitle: `${row?.fullName || ""} · ${formatDayLabel(day?.date)}`.trim(),
    text: explainCell(cell, day),
    facts
  };
}

/** Text of a cell in the CSV export: "On time 8", "Delayed 8 (late 2 d 3 h)", "H", "L", "½ On time 4". */
export function exportCellText (cell) {
  if (!cell) return "";
  const s = stateOf(cell);
  const half = isHalfDay(cell) ? "½ " : "";
  switch (cell.state) {
    case "Holiday":
      return cell.logged > 0 ? `H (+${fmtHours(cell.logged)})` : "H";
    case "Leave":
      return cell.logged > 0 ? `L (+${fmtHours(cell.logged)})` : "L";
    case "Future":
      return cell.logged > 0 ? `– (+${fmtHours(cell.logged)})` : "–";
    case "NotEmployed":
      return "";
    case "Delayed":
      return `${half}${s.label} ${fmtHours(cell.logged)} (late ${fmtLate(cell.lateMinutes)})`;
    case "Pending":
      return `${half}${s.label}`;
    default:
      return `${half}${s.label} ${fmtHours(cell.logged)}`;
  }
}
