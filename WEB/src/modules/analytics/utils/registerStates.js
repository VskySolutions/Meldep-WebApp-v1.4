import { formatAppDate, formatDayLabel, formatLocalMinute } from "modules/analytics/composables/useDateRangePresets";

// The vocabulary of the Analytics registers: what a cell state is called, how it looks and what it says. The
// register components (grid, cell, legend, tiles) are shared; each page hands them its vocabulary object.
//
//   #region Shared                           helpers both registers use
//   #region Timesheet Register               /analytics/timesheet-register   → TIMESHEET_VOCAB
//   #region TimeInTimeOut-MovementRegister   /analytics/attendance-register  → ATTENDANCE_VOCAB
//
// Every state carries an icon or glyph AND a label, so a cell is never read by colour alone (the colours are
// the fixed status palette: good / warning / serious / critical, the brand blue for "not judged yet", and
// neutrals for non-working days).

// #region Shared

export const FULL_DAY_LEAVE = "Full";

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

// #endregion Shared

// #region Timesheet Register

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

// The key under the grid: the states in order, then the markers a cell can carry.
const TIMESHEET_LEGEND = [
  ...STATE_ORDER
    .filter((k) => k !== "NotEmployed")
    .map((k) => ({ key: k, label: STATES[k].label, full: STATES[k].label, icon: STATES[k].icon, glyph: STATES[k].glyph, colour: STATES[k].colour, description: STATES[k].description })),
  {
    key: "half",
    label: "Half-day leave",
    full: "½ · Half-day leave",
    glyph: "½",
    plain: true,
    description: "A half-day leave covers the day, so 4 hours are expected instead of 8. The cell is still judged on time, delayed, partial or pending against those 4 hours."
  },
  {
    key: "prefilled",
    label: "Pre-filled",
    full: "Pre-filled",
    dot: true,
    plain: true,
    description: "The hours were entered before the day had started. Counts as on time, but worth a look."
  },
  {
    key: "offday",
    label: "Hours on an off day",
    full: "+8 · Hours on an off day",
    glyph: "+8",
    small: true,
    plain: true,
    description: "Hours were logged on a holiday, a leave day or a future day. The day keeps its H, L or future state; the hint shows how many hours."
  }
];

// The "at a glance" strip: how many employee-days were expected in the range and how they went. Every tile but
// the first is a filter toggle: click it to keep only the employees it counts, click again to release.
function timesheetTiles (summary, rules) {
  const s = summary || {};
  const expected = s.expectedDays || 0;
  const defaultShift = rules?.defaultShift || "the default shift";
  const pct = (n) => (expected ? `${Math.round((n / expected) * 100)}% of expected days` : "");

  return [
    {
      key: "expected",
      label: "Expected days",
      value: expected,
      caption: `${s.employees || 0} employees · ${fmtHours(s.loggedHours)} of ${fmtHours(s.expectedHours)} h logged`,
      icon: "o_event_available",
      colour: "#647a82",
      clickable: false,
      hint: "Working days the listed employees were expected to fill (past days and today) once Sundays, holidays and full-day leaves are taken out."
    },
    {
      key: "OnTime",
      label: STATES.OnTime.label,
      value: s.onTime || 0,
      caption: pct(s.onTime || 0),
      icon: STATES.OnTime.icon,
      colour: STATES.OnTime.colour,
      clickable: true,
      hint: STATES.OnTime.description
    },
    {
      key: "Delayed",
      label: STATES.Delayed.label,
      value: s.delayed || 0,
      caption: pct(s.delayed || 0),
      icon: STATES.Delayed.icon,
      colour: STATES.Delayed.colour,
      clickable: true,
      hint: STATES.Delayed.description
    },
    {
      key: "Partial",
      label: STATES.Partial.label,
      value: s.partial || 0,
      caption: pct(s.partial || 0),
      icon: STATES.Partial.icon,
      colour: STATES.Partial.colour,
      clickable: true,
      hint: STATES.Partial.description
    },
    {
      key: "Pending",
      label: STATES.Pending.label,
      value: s.pending || 0,
      caption: pct(s.pending || 0),
      icon: STATES.Pending.icon,
      colour: STATES.Pending.colour,
      clickable: true,
      hint: STATES.Pending.description
    },
    {
      key: "Due",
      label: STATES.Due.label,
      value: s.due || 0,
      caption: "cutoff not passed yet",
      icon: STATES.Due.icon,
      colour: STATES.Due.colour,
      clickable: true,
      hint: STATES.Due.description
    },
    {
      key: "Leave",
      label: "On leave",
      value: s.leaveDays || 0,
      caption: s.halfDays ? `full days · ${s.halfDays} half days besides` : "full days",
      icon: "o_beach_access",
      colour: STATES.Leave.colour,
      clickable: true,
      hint: "Employee-days covered by a full-day leave (applied, forwarded or approved). Half-day leaves keep the day expected at 4 hours."
    },
    {
      key: "noShift",
      label: "No shift",
      value: s.employeesWithoutShift || 0,
      caption: `employees, judged as ${defaultShift}`,
      icon: "o_person_off",
      colour: "#898781",
      clickable: true,
      hint: "Employees whose designation record has no shift. Their cutoff uses the default shift until a shift is assigned on the employee record."
    }
  ];
}

function timesheetFootText (t, day) {
  if (!t || !t.expected) return day?.isHoliday ? (day.holidayTitle || "Holiday") : "No one was expected to fill this day.";
  return [
    `${t.expected} expected`,
    `${t.onTime} on time`,
    `${t.delayed} delayed`,
    `${t.partial} partial`,
    `${t.pending} pending`,
    t.due ? `${t.due} due` : "",
    t.leave ? `${t.leave} on leave` : ""
  ].filter(Boolean).join(" · ");
}

// Everything the shared register components need to draw the Timesheet Register.
export const TIMESHEET_VOCAB = {
  name: "timesheet",
  // cell
  stateOf,
  isHalfDay,
  cellText: (cell) => cellText(cell),
  cellExtra,
  // Top-right dot: the hours were entered before the day had started.
  cellDot: (cell) => !!cell?.preFilled && cell.state !== "Future",
  cellFlag: () => false,
  cellTags: () => [],
  offStates: ["Holiday", "Future", "NotEmployed"],
  // grid
  cellHover: (row, day, cell) => cellHover(row, day, cell),
  totalColumns: TOTAL_COLUMNS,
  sheet: {
    icon: "o_receipt_long",
    tooltip: "View this employee's timesheet for the period or any week",
    aria: (row) => `View the timesheet of ${row.fullName}`
  },
  foot: {
    label: "Complete / expected",
    text: "Per day: employees whose expected hours were reached (on time or late), out of those expected to fill the day. Days before joining, holidays and full-day leaves are not expected.",
    value: (t) => (t && t.expected ? `${t.complete}/${t.expected}` : ""),
    detail: timesheetFootText
  },
  todayText: "Today. Days are “due” until their cutoff passes.",
  shiftWindow: (rule) => `${rule.start}–${rule.end}${rule.endsNextDay ? " (next day)" : ""}, cutoff ${rule.cutoff}`,
  // legend and tiles
  legendItems: TIMESHEET_LEGEND,
  tiles: timesheetTiles
};

// #endregion Timesheet Register

// #region TimeInTimeOut-MovementRegister (Attendance Register)

export const ATTENDANCE_STATES = {
  OnTime: {
    key: "OnTime",
    label: "On time",
    icon: "o_check_circle",
    glyph: "",
    colour: "#0ca30c",
    description: "Timed in by the shift start plus grace and worked the expected hours."
  },
  Late: {
    key: "Late",
    label: "Late",
    icon: "o_schedule",
    glyph: "",
    colour: "#a66f00",
    description: "The expected hours were worked, but the first time-in came after the shift start plus grace. A “T” beside it means a Time Adjustment was entered for the day."
  },
  Short: {
    key: "Short",
    label: "Short hours",
    icon: "o_donut_large",
    glyph: "",
    colour: "#d9541e",
    description: "Timed in and out, but the productive hours (time in to time out, minus the breaks and the time adjustments) are fewer than required."
  },
  NoTimeOut: {
    key: "NoTimeOut",
    label: "No time-out",
    icon: "o_timer_off",
    glyph: "",
    colour: "#b5347f",
    description: "A time-in was never closed, or was closed so much later that the time-out was plainly forgotten. The day counts as attended; its hours are not counted."
  },
  Absent: {
    key: "Absent",
    label: "Absent",
    icon: "o_cancel",
    glyph: "",
    colour: "#d03b3b",
    description: "A working day with no time-in, no Work From Home entry and no leave."
  },
  InNow: {
    key: "InNow",
    label: "In now",
    icon: "o_login",
    glyph: "",
    colour: "#1b75ab",
    description: "Timed in and not out yet. The hours shown are the hours so far."
  },
  Awaited: {
    key: "Awaited",
    label: "Not in yet",
    icon: "o_hourglass_empty",
    glyph: "",
    colour: "#647a82",
    description: "Today only: no time-in yet, and the shift start plus grace has not passed. It turns to absent once that passes."
  },
  Wfh: {
    key: "Wfh",
    label: "WFH, no time-in",
    icon: "",
    glyph: "W",
    colour: "#0b8a8a",
    description: "A Work From Home entry covers the day and there is no time-in. With a time-in the day is judged like any other and carries a small “W”."
  },
  Leave: {
    key: "Leave",
    label: "Leave",
    icon: "",
    glyph: "L",
    colour: "#6f42c1",
    description: "A full-day leave covers the date: applied, forwarded or approved. Nobody is expected."
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

export const ATTENDANCE_STATE_ORDER = ["OnTime", "Late", "Short", "NoTimeOut", "Absent", "InNow", "Awaited", "Wfh", "Leave", "Holiday", "Future", "NotEmployed"];

// Working days on which the employee was expected.
export const ATTENDANCE_EXPECTED_STATES = ["OnTime", "Late", "Short", "NoTimeOut", "Absent", "InNow", "Awaited", "Wfh"];

// Expected days with a time-in.
export const ATTENDANCE_PRESENT_STATES = ["OnTime", "Late", "Short", "NoTimeOut", "InNow"];

// What a reader can filter employees by: a row qualifies when any of its days matches. "Late" and "WFH" are
// facts about a day rather than its one state (a late day can also be short), so they have their own tests.
export const ATTENDANCE_DAY_FILTERS = [
  { value: "OnTime", label: "On time" },
  { value: "Late", label: "Late" },
  { value: "LateUndeclared", label: "Late, no time adjustment" },
  { value: "Short", label: "Short hours" },
  { value: "NoTimeOut", label: "No time-out" },
  { value: "Absent", label: "Absent" },
  { value: "InNow", label: "In now" },
  { value: "Wfh", label: "Work From Home" },
  { value: "Leave", label: "Leave" }
];

// The Movement Register entry types, as a filter on the same rows.
export const ATTENDANCE_MOVEMENT_FILTERS = [
  { value: "Break", label: "Break" },
  { value: "Adjust", label: "Time Adjustment" },
  { value: "Wfh", label: "Work From Home" }
];

// How a record looks wherever it is listed, keyed by the kind the API sends (AttendanceMovementKind by name);
// "Session" is a TimeInTimeOut row.
export const RECORD_LOOK = {
  Session: { icon: "o_login", colour: "#0ca30c" },
  Break: { icon: "o_free_breakfast", colour: "#8a6d3b" },
  TimeAdjustment: { icon: "o_more_time", colour: "#a66f00" },
  WorkFromHome: { icon: "o_home_work", colour: "#0b8a8a" },
  Other: { icon: "o_event_note", colour: "#647a82" }
};

// The three Movement Register types an employee's totals can be opened by (the WFH, Brk and TA columns):
// the filter key, the kind the API sends, and what the date-wise panel calls it.
export const ATTENDANCE_MOVEMENT_KINDS = {
  Wfh: { key: "Wfh", kind: "WorkFromHome", label: "Work From Home", entry: "Work From Home entry", entries: "Work From Home entries" },
  Break: { key: "Break", kind: "Break", label: "Breaks", entry: "break", entries: "breaks" },
  Adjust: { key: "Adjust", kind: "TimeAdjustment", label: "Time adjustments", entry: "time adjustment", entries: "time adjustments" }
};

export function attendanceStateOf (cell) {
  return ATTENDANCE_STATES[cell?.state] || ATTENDANCE_STATES.Future;
}

/** Does this employee-day match a tile, a day-state filter or a movement filter? */
export function attendanceCellHas (cell, key) {
  if (!cell) return false;
  switch (key) {
    case "Late":
      return cell.lateMinutes > 0;
    case "LateUndeclared":
      return cell.lateMinutes > 0 && !cell.adjustments;
    case "Wfh":
      return !!cell.wfh;
    case "Break":
      return cell.breaks > 0;
    case "Adjust":
      return cell.adjustments > 0;
    case "Leave":
      return cell.state === "Leave" || !!cell.leave;
    default:
      return cell.state === key;
  }
}

/**
 * Minutes → hours with one decimal, cut rather than rounded so a day a few minutes short never reads as the
 * full hours: 517 → "8.6", 477 → "7.9", 480 → "8".
 */
export function fmtWorked (minutes) {
  const hours = Math.floor(Math.max(0, Number(minutes || 0)) / 6) / 10;
  return Number.isInteger(hours) ? String(hours) : hours.toFixed(1);
}

/** Minutes → "8 h 37 m", "45 m", "9 h"; "0 m" for nothing. */
export function fmtMinutes (minutes) {
  const m = Math.max(0, Math.round(Number(minutes || 0)));
  const hours = Math.floor(m / 60);
  const mins = m % 60;
  if (!hours) return `${mins} m`;
  return mins ? `${hours} h ${mins} m` : `${hours} h`;
}

/** A 24-hour clock text in the 12-hour form the register shows: "14:13" → "2:13 PM", "00:04" → "12:04 AM". */
export function fmtTime12 (hhmm) {
  const m = /^(\d{1,2}):(\d{2})/.exec(String(hhmm || ""));
  if (!m) return String(hhmm || "");
  const h = Number(m[1]);
  return `${h % 12 === 0 ? 12 : h % 12}:${m[2]} ${h < 12 ? "AM" : "PM"}`;
}

/**
 * "2026-10-09T14:13" → "2:13 PM". With the day it belongs to, a time on another date says so: "12:04 AM next
 * day", or the date itself when it is further away.
 */
export function fmtClock (value, dayIso = "") {
  const m = /^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})/.exec(String(value || ""));
  if (!m) return "";
  const time = fmtTime12(m[2]);
  if (!dayIso || m[1] === dayIso) return time;
  const gap = Math.round((Date.parse(`${m[1]}T00:00:00Z`) - Date.parse(`${dayIso}T00:00:00Z`)) / 86400000);
  if (gap === 1) return `${time} next day`;
  return `${time} on ${formatAppDate(m[1])}`;
}

/** "2026-10-09T17:10" → "10/09/2026 5:10 PM": a moment that needs its date (when an entry was made). */
export function fmtStamp (value) {
  const m = /^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})/.exec(String(value || ""));
  return m ? `${formatAppDate(m[1])} ${fmtTime12(m[2])}` : "";
}

/** The length of a Movement Register entry: the duration picked for work from home, the minutes otherwise. */
export function movementLength (entry) {
  if (!entry) return "";
  if (entry.kind === "WorkFromHome") return entry.duration && entry.duration !== "Unspecified" ? entry.duration : "";
  return entry.minutes > 0 ? fmtMinutes(entry.minutes) : (entry.duration || "");
}

// How a TimeInTimeOut row ended, in words (AttendanceSessionStatus by name).
export const SESSION_STATUS_TEXT = {
  Closed: "",
  Live: "Still in",
  NoTimeOut: "No time-out; hours not counted",
  TooLong: "Timed out too late to count; hours not counted"
};

/** The clock time after which a first time-in is late for the shift the day was judged with, e.g. "11:15 AM". */
export function lateAfterOf (cell, row, rules) {
  const { shift } = shiftOf(cell, row);
  const lateAfter = (rules?.shifts || []).find((s) => s.name === shift)?.lateAfter || "";
  return lateAfter ? fmtTime12(lateAfter) : "";
}

/** "Full Day" → "", "1st Half" → "1st half", ...: the length of a Work From Home entry when it is not the whole day. */
function wfhPart (wfh) {
  if (!wfh || wfh === "Full Day" || wfh === "Unspecified") return "";
  return String(wfh).toLowerCase();
}

/** Text shown inside a cell: the worked hours ("hours") or the first time-in and last time-out ("inout"). */
export function attendanceCellText (cell, mode = "hours") {
  const state = cell?.state;
  const first = fmtClock(cell?.firstIn);

  if (mode === "inout") {
    switch (state) {
      case "OnTime":
      case "Late":
      case "Short":
        return `${first}–${fmtClock(cell.lastOut)}`;
      case "InNow":
        return `${first}–…`;
      case "NoTimeOut":
        return `${first}–?`;
      case "Absent":
        return "—";
      case "Wfh":
        return wfhPart(cell.wfh);
      default:
        return "";
    }
  }

  switch (state) {
    case "OnTime":
    case "Late":
    case "Short":
      return fmtWorked(cell.worked);
    case "InNow":
      return `in ${first}`;
    case "NoTimeOut":
      return "?";
    case "Absent":
      return "—";
    case "Wfh":
      return wfhPart(cell.wfh);
    default:
      return "";
  }
}

/** Hours worked on a day where nobody was expected (holiday, leave, future): a small "+8.6" hint. */
export function attendanceCellExtra (cell) {
  const state = cell?.state;
  if (cell?.worked > 0 && (state === "Holiday" || state === "Leave" || state === "Future")) return `+${fmtWorked(cell.worked)}`;
  return "";
}

/** The Movement Register letters beside the hours: W (work from home with a time-in), B (break), T (time adjustment). */
export function attendanceCellTags (cell) {
  const tags = [];
  if (cell?.wfh && cell.state !== "Wfh") tags.push("W");
  if (cell?.breaks > 0) tags.push("B");
  if (cell?.adjustments > 0) tags.push("T");
  return tags;
}

/** "3 breaks (1 h 15 m)", "1 time adjustment (30 m)", "Work From Home (Full Day)" — what the Movement Register holds for the day. */
export function movementFacts (cell) {
  const facts = [];
  // An entry saved without a length (0 minutes) is counted but has no minutes to show.
  const length = (minutes) => (minutes > 0 ? ` (${fmtMinutes(minutes)})` : "");
  if (cell?.wfh) facts.push(`Work From Home${cell.wfh !== "Unspecified" ? ` (${cell.wfh})` : ""}`);
  if (cell?.breaks > 0) facts.push(`${cell.breaks} ${cell.breaks === 1 ? "break" : "breaks"}${length(cell.breakMinutes)}`);
  if (cell?.adjustments > 0) facts.push(`${cell.adjustments} ${cell.adjustments === 1 ? "time adjustment" : "time adjustments"}${length(cell.adjustMinutes)}`);
  return facts;
}

/** "3 h 1 m after the shift start, with a time adjustment entered" */
function lateText (cell) {
  if (!(cell?.lateMinutes > 0)) return "";
  return `${fmtLate(cell.lateMinutes)} after the shift start, ${cell.adjustments > 0 ? "with a time adjustment entered" : "with no time adjustment entered"}`;
}

/**
 * The arrival is judged against the shift start unless a half-day leave may cover the start of the day (the
 * API applies the same rule: only a leave on the second half leaves the morning arrival as it was).
 */
function arrivalJudged (cell) {
  return !cell?.leave || String(cell.leave.kind || "").trim().toLowerCase() === "2nd half";
}

/** One sentence that explains the state of this cell, for the hover card and the day panel. */
export function explainAttendance (cell, day = null) {
  const first = fmtClock(cell?.firstIn, day?.date);
  const worked = fmtMinutes(cell?.worked);
  const expected = fmtMinutes(cell?.expected);
  switch (cell?.state) {
    case "OnTime":
      return arrivalJudged(cell)
        ? `Timed in at ${first}, on time, and worked ${worked} of the ${expected} expected.`
        : `Timed in at ${first} and worked ${worked} of the ${expected} expected; the arrival is not judged under a half-day leave on the first half.`;
    case "Late":
      return `Timed in at ${first}, ${lateText(cell)}; worked ${worked} of the ${expected} expected.`;
    case "Short":
      return `Worked ${worked} of the ${expected} expected${cell.lateMinutes > 0 ? `, and timed in ${lateText(cell)}` : ""}.`;
    case "NoTimeOut":
      return `Timed in at ${first} with no usable time-out, so the day's hours are not counted${cell.lateMinutes > 0 ? `. The time-in was ${lateText(cell)}` : ""}.`;
    case "Absent":
      return cell.leave
        ? "No time-in and no Work From Home entry for the working half of a half-day leave."
        : "No time-in, no Work From Home entry and no leave on a working day.";
    case "InNow":
      return `Timed in at ${first} and still in; ${worked} worked so far${cell.lateMinutes > 0 ? `. The time-in was ${lateText(cell)}` : ""}.`;
    case "Awaited":
      return "Not timed in yet; the shift start plus grace has not passed.";
    case "Wfh":
      return `Work From Home${cell.wfh && cell.wfh !== "Unspecified" ? ` (${cell.wfh})` : ""} with no time-in.`;
    case "Leave":
      return `Full-day leave (${cell.leave?.status || "applied"}${cell.leave?.category ? ", " + cell.leave.category : ""}); nobody expected.`;
    case "Holiday":
      return day?.holidayTitle || "Holiday";
    case "Future":
      return cell.wfh ? "Not yet due; a Work From Home entry is already filed for the day." : "Not yet due.";
    case "NotEmployed":
      return "Outside the employment period (before joining or after release).";
    default:
      return "";
  }
}

/** What the grid's hover card shows for a cell: the state, who/when, the explanation and the facts behind it. */
export function attendanceCellHover (row, day, cell, rules = null) {
  const s = attendanceStateOf(cell);
  const facts = [];
  const judged = ATTENDANCE_EXPECTED_STATES.includes(cell.state);

  if (cell.firstIn) {
    const out = cell.lastOut ? `out ${fmtClock(cell.lastOut, day?.date)}` : (cell.state === "InNow" ? "still in" : "no time-out");
    const sessions = cell.sessions > 1 ? ` · ${cell.sessions} time-ins` : "";
    facts.push(`In ${fmtClock(cell.firstIn, day?.date)} · ${out}${sessions}`);
  }
  if (cell.clocked > 0) {
    const breaks = cell.breakMinutes > 0 ? ` − breaks ${fmtMinutes(cell.breakMinutes)}` : "";
    const adjusts = cell.adjustMinutes > 0 ? ` − time adjustments ${fmtMinutes(cell.adjustMinutes)}` : "";
    const expected = judged ? ` of ${fmtMinutes(cell.expected)} required` : "";
    facts.push(`Clocked ${fmtMinutes(cell.clocked)}${breaks}${adjusts} = productive ${fmtMinutes(cell.worked)}${expected}`);
  }
  if (judged) {
    const { shift, assigned } = shiftOf(cell, row);
    const lateAfter = lateAfterOf(cell, row, rules);
    facts.push(`${assigned ? shift : `${shift}, not assigned (default)`}${lateAfter ? ` · late after ${lateAfter}` : ""}`);
  }
  facts.push(...movementFacts(cell));
  if (cell.leave) {
    facts.push(`Leave: ${cell.leave.kind}${cell.leave.status ? ` · ${cell.leave.status}` : ""}${cell.leave.category ? ` · ${cell.leave.category}` : ""}`);
  }

  return {
    colour: s.colour,
    icon: s.icon,
    glyph: s.glyph,
    title: `${s.label}${isHalfDay(cell) ? " · half-day leave" : ""}`,
    subtitle: `${row?.fullName || ""} · ${formatDayLabel(day?.date)}`.trim(),
    text: explainAttendance(cell, day),
    facts
  };
}

/** Text of a cell in the CSV export: "On time 14:13-23:28 (8 h) B75 T30", "Late 14:27-23:27 (8.2 h, late 3 h 27 m) T60", "Absent", "H", "L", "WFH Full Day". */
export function attendanceExportCellText (cell) {
  if (!cell) return "";
  if (cell.state === "NotEmployed") return "";

  const s = attendanceStateOf(cell);
  const half = isHalfDay(cell) ? "½ " : "";
  const marks = [];
  if (cell.wfh && cell.state !== "Wfh") marks.push(`WFH ${cell.wfh}`);
  if (cell.breaks > 0) marks.push(`B${cell.breakMinutes || 0}`);
  if (cell.adjustments > 0) marks.push(`T${cell.adjustMinutes || 0}`);
  const tail = marks.length ? ` ${marks.join(" ")}` : "";
  const first = fmtClock(cell.firstIn);
  const late = cell.lateMinutes > 0 ? `, late ${fmtLate(cell.lateMinutes)}` : "";

  switch (cell.state) {
    case "Holiday":
    case "Leave":
    case "Future": {
      const glyph = cell.state === "Future" ? "–" : s.glyph;
      return `${glyph}${cell.worked > 0 ? ` (+${fmtWorked(cell.worked)} h)` : ""}${tail}`;
    }
    case "OnTime":
    case "Late":
    case "Short":
      return `${half}${s.label} ${first}-${fmtClock(cell.lastOut)} (${fmtWorked(cell.worked)} h${late})${tail}`;
    case "InNow":
      return `${half}${s.label} ${first} (${fmtWorked(cell.worked)} h so far${late})${tail}`;
    case "NoTimeOut":
      return `${half}${s.label} ${first}${late ? ` (${late.slice(2)})` : ""}${tail}`;
    case "Wfh":
      return `${half}WFH${cell.wfh && cell.wfh !== "Unspecified" ? ` ${cell.wfh}` : ""}, no time-in${tail}`;
    default:
      return `${half}${s.label}${tail}`;
  }
}

/** Minutes → "156 h 15 m" for a period total; blank when there are none. */
function hoursOf (minutes) {
  return minutes > 0 ? fmtMinutes(minutes) : "";
}

// The period-total columns of the attendance grid (same contract as TOTAL_COLUMNS).
export const ATTENDANCE_TOTAL_COLUMNS = [
  // The hours story, left to right: Req, then Clk − Brk − TA = Prod, then what Prod is short of Req. All are
  // kept in minutes and shown as hours and minutes: 45 → "45 m", 585 → "9 h 45 m" (never decimal hours).
  {
    key: "required",
    abbr: "Req",
    full: "Required hours",
    desc: "The hours the employee had to work in the period: 8 a working day, 4 under a half-day leave, nothing on a holiday or a full-day leave. Today counts once the employee has timed out.",
    cls: "",
    value: (t) => hoursOf(t.requiredMinutes)
  },
  {
    key: "clocked",
    abbr: "Clk",
    full: "Clocked hours",
    desc: "Time in to time out, added over the period. A day with a missing time-out, or with no time-in at all (absent, work from home), adds nothing.",
    cls: "",
    value: (t) => hoursOf(t.clockedMinutes)
  },
  // The Movement Register columns open their entries date by date: `action` is the key of
  // ATTENDANCE_MOVEMENT_KINDS the grid hands back when a value is clicked.
  {
    key: "breaks",
    abbr: "Brk",
    full: "Break hours",
    desc: "Total length of the Break entries in the Movement Register over the period, in hours and minutes. Click an employee's value to see the breaks date by date.",
    cls: "",
    action: "Break",
    actionHint: "Show the breaks, date by date",
    // An entry saved without a length still counts: it shows as 0 rather than blank, and can be opened.
    value: (t) => (t.breaks ? fmtMinutes(t.breakMinutes) : "")
  },
  {
    key: "adjust",
    abbr: "TA",
    full: "Time Adjustment hours",
    desc: "Total length of the Time Adjustment entries in the Movement Register over the period, in hours and minutes. Click an employee's value to see them date by date.",
    cls: "",
    action: "Adjust",
    actionHint: "Show the time adjustments, date by date",
    value: (t) => (t.adjustments ? fmtMinutes(t.adjustMinutes) : "")
  },
  {
    key: "productive",
    abbr: "Prod",
    full: "Productive hours",
    desc: "Clocked hours minus the Break and the Time Adjustment hours, day by day. Hours on a holiday or a leave day are included.",
    cls: "tsr-td--strong",
    value: (t) => hoursOf(t.workedMinutes)
  },
  {
    key: "shortfall",
    abbr: "Short",
    full: "Hours short of required",
    desc: "Required hours minus productive hours over the period, when the productive hours are fewer. A longer day offsets a shorter one, so this is the balance of the whole period. Blank means the required hours were met.",
    cls: "tsr-td--pnd",
    value: (t) => hoursOf(t.shortMinutes)
  },
  // The other side of the same balance: an employee has a value under Short or under Extra, never both.
  {
    key: "extra",
    abbr: "Extra",
    full: "Hours above required",
    desc: "Productive hours minus required hours over the period, when the productive hours are more. An employee shows a value here or under Short, never both; both blank means the hours match exactly.",
    cls: "tsr-td--ok tsr-td--strong",
    value: (t) => hoursOf(t.extraMinutes)
  },
  {
    key: "late",
    abbr: "Lt",
    full: "Late days",
    desc: "Days whose first time-in came after the shift start plus grace, including days that also ended short or without a time-out.",
    cls: "tsr-td--dly",
    value: (t) => t.late || ""
  },
  {
    key: "absent",
    abbr: "Ab",
    full: "Absent days",
    desc: "Working days with no time-in, no Work From Home entry and no leave.",
    cls: "tsr-td--pnd",
    value: (t) => t.absent || ""
  },
  {
    key: "wfh",
    abbr: "WFH",
    full: "Work From Home days",
    desc: "Days with a Work From Home entry in the Movement Register, with or without a time-in. They add productive hours only when the employee also timed in. Click an employee's value to see the dates.",
    cls: "",
    action: "Wfh",
    actionHint: "Show the Work From Home days, date by date",
    value: (t) => t.wfh || ""
  }
];

const ATTENDANCE_LEGEND = [
  ...ATTENDANCE_STATE_ORDER
    .filter((k) => k !== "NotEmployed")
    .map((k) => {
      const s = ATTENDANCE_STATES[k];
      return { key: k, label: s.label, full: s.label, icon: s.icon, glyph: s.glyph, colour: s.colour, description: s.description };
    }),
  {
    key: "alsoLate",
    label: "Also late",
    full: "Amber corner · also late",
    flag: true,
    plain: true,
    description: "The first time-in was late on a day that shows another state (short hours, no time-out, in now). Late days are counted whatever else happened."
  },
  {
    key: "half",
    label: "Half-day leave",
    full: "½ · Half-day leave",
    glyph: "½",
    plain: true,
    description: "A half-day leave covers the day, so 4 hours are expected instead of 8. With the leave on the first half, the arrival is not judged late."
  },
  {
    key: "wfhTag",
    label: "WFH",
    full: "W · Work From Home",
    glyph: "W",
    small: true,
    plain: true,
    description: "The day has a Work From Home entry in the Movement Register and a time-in."
  },
  {
    key: "breakTag",
    label: "Break",
    full: "B · Break",
    glyph: "B",
    small: true,
    plain: true,
    description: "The day has Break entries in the Movement Register. Their minutes are taken out of the productive hours."
  },
  {
    key: "adjustTag",
    label: "Time adjustment",
    full: "T · Time Adjustment",
    glyph: "T",
    small: true,
    plain: true,
    description: "The day has a Time Adjustment entry in the Movement Register (a late arrival, an early login or a shift change the employee declared). Its minutes are taken out of the productive hours."
  },
  {
    key: "movementDot",
    label: "Movement (compact)",
    full: "Dot · Movement Register entries",
    dot: true,
    plain: true,
    description: "In the compact view the letters are replaced by one dot: the day has Movement Register entries. Hover the cell to see them."
  },
  {
    key: "offday",
    label: "Hours on an off day",
    full: "+8.6 · Hours on an off day",
    glyph: "+8",
    small: true,
    plain: true,
    description: "The employee timed in on a holiday or a leave day. The day keeps its H or L; the hint shows the hours worked."
  }
];

function attendanceTiles (summary, rules) {
  const s = summary || {};
  const expected = s.expectedDays || 0;
  const defaultShift = rules?.defaultShift || "the default shift";
  const pct = (n) => (expected ? `${Math.round((n / expected) * 100)}% of expected days` : "");
  const A = ATTENDANCE_STATES;

  const tiles = [
    {
      key: "expected",
      label: "Expected days",
      value: expected,
      caption: `${s.employees || 0} employees · ${fmtMinutes(s.workedMinutes)} of ${fmtMinutes(s.requiredMinutes)} productive`,
      icon: "o_event_available",
      colour: "#647a82",
      clickable: false,
      hint: "Working days the listed employees were expected (past days and today) once Sundays, holidays and full-day leaves are taken out."
    },
    {
      key: "OnTime",
      label: A.OnTime.label,
      value: s.onTime || 0,
      caption: pct(s.onTime || 0),
      icon: A.OnTime.icon,
      colour: A.OnTime.colour,
      clickable: true,
      hint: A.OnTime.description
    },
    {
      key: "Late",
      label: A.Late.label,
      value: s.late || 0,
      caption: s.late ? `${s.lateUndeclared || 0} with no time adjustment` : pct(0),
      icon: A.Late.icon,
      colour: A.Late.colour,
      clickable: true,
      hint: "Days whose first time-in came after the shift start plus grace, whatever else happened that day. The caption counts the ones with no Time Adjustment entry in the Movement Register."
    },
    {
      key: "Short",
      label: A.Short.label,
      value: s.short || 0,
      caption: pct(s.short || 0),
      icon: A.Short.icon,
      colour: A.Short.colour,
      clickable: true,
      hint: A.Short.description
    },
    {
      key: "NoTimeOut",
      label: A.NoTimeOut.label,
      value: s.noTimeOut || 0,
      caption: pct(s.noTimeOut || 0),
      icon: A.NoTimeOut.icon,
      colour: A.NoTimeOut.colour,
      clickable: true,
      hint: A.NoTimeOut.description
    },
    {
      key: "Absent",
      label: A.Absent.label,
      value: s.absent || 0,
      caption: pct(s.absent || 0),
      icon: A.Absent.icon,
      colour: A.Absent.colour,
      clickable: true,
      hint: A.Absent.description
    }
  ];

  // The hours balance beside the day counts: how far the productive hours fall below the required hours.
  tiles.splice(1, 0, {
    key: "ShortHours",
    label: "Hours short",
    value: fmtMinutes(s.shortMinutes),
    caption: `${s.employeesShort || 0} employees below required`,
    icon: "o_hourglass_bottom",
    colour: "#d03b3b",
    clickable: true,
    hint: "Required hours minus productive hours (clocked minus breaks and time adjustments), struck per employee over the period so a longer day offsets a shorter one, then added up. Click to keep the employees who are short."
  });

  // Only meaningful while the period includes today.
  if (s.inNow || s.awaited) {
    tiles.push({
      key: "InNow",
      label: A.InNow.label,
      value: s.inNow || 0,
      caption: s.awaited ? `${s.awaited} not in yet` : "timed in, not out yet",
      icon: A.InNow.icon,
      colour: A.InNow.colour,
      clickable: true,
      hint: "Employees who are timed in right now. “Not in yet” are today's employees whose shift start plus grace has not passed."
    });
  }

  tiles.push(
    {
      key: "Wfh",
      label: "Work From Home",
      value: s.wfh || 0,
      caption: s.wfhNoTimeIn ? `days · ${s.wfhNoTimeIn} with no time-in` : "days",
      icon: "o_home_work",
      colour: A.Wfh.colour,
      clickable: true,
      hint: "Days with a Work From Home entry in the Movement Register. The caption counts the ones with no time-in."
    },
    {
      key: "Leave",
      label: "On leave",
      value: s.leave || 0,
      caption: s.halfDay ? `full days · ${s.halfDay} half days besides` : "full days",
      icon: "o_beach_access",
      colour: A.Leave.colour,
      clickable: true,
      hint: "Employee-days covered by a full-day leave (applied, forwarded or approved). Half-day leaves keep the day expected at 4 hours."
    },
    {
      key: "Break",
      label: "Breaks",
      value: s.breaks || 0,
      caption: `entries · ${fmtMinutes(s.breakMinutes)}`,
      icon: "o_free_breakfast",
      colour: "#647a82",
      clickable: true,
      hint: "Break entries in the Movement Register and their total length. Break minutes are taken out of the productive hours."
    },
    {
      key: "Adjust",
      label: "Time adjustments",
      value: s.adjustments || 0,
      caption: `entries · ${fmtMinutes(s.adjustMinutes)}`,
      icon: "o_more_time",
      colour: "#647a82",
      clickable: true,
      hint: "Time Adjustment entries in the Movement Register: late arrivals, early logins and shift changes the employees declared. Their minutes are taken out of the productive hours."
    },
    {
      key: "noShift",
      label: "No shift",
      value: s.employeesWithoutShift || 0,
      caption: `employees, judged as ${defaultShift}`,
      icon: "o_person_off",
      colour: "#898781",
      clickable: true,
      hint: "Employees whose designation record has no shift. Their arrival is judged against the default shift until a shift is assigned on the employee record."
    }
  );

  return tiles;
}

function attendanceFootText (t, day) {
  if (!t || !t.expected) return day?.isHoliday ? (day.holidayTitle || "Holiday") : "No one was expected on this day.";
  return [
    `${t.expected} expected`,
    `${t.present} timed in`,
    `${t.onTime} on time`,
    `${t.late} late`,
    t.short ? `${t.short} short hours` : "",
    t.noTimeOut ? `${t.noTimeOut} no time-out` : "",
    t.inNow ? `${t.inNow} in now` : "",
    t.wfh ? `${t.wfh} WFH without a time-in` : "",
    `${t.absent} absent`,
    t.awaited ? `${t.awaited} not in yet` : "",
    t.leave ? `${t.leave} on leave` : ""
  ].filter(Boolean).join(" · ");
}

// Everything the shared register components need to draw the Attendance Register.
export const ATTENDANCE_VOCAB = {
  name: "attendance",
  // cell
  stateOf: attendanceStateOf,
  isHalfDay,
  cellText: attendanceCellText,
  cellExtra: attendanceCellExtra,
  // Top-right dot: in the compact view, the day has Movement Register entries (the letters need more room).
  cellDot: (cell, compact) => compact && attendanceCellTags(cell).length > 0,
  // Amber corner: a late first time-in on a day that shows another state.
  cellFlag: (cell) => cell?.lateMinutes > 0 && cell.state !== "Late",
  cellTags: attendanceCellTags,
  offStates: ["Holiday", "Future", "NotEmployed"],
  // Day-column width per cell mode (px): the hours plus the letters, or the in–out times plus the letters.
  cellWidth: { hours: 96, inout: 172 },
  // grid
  cellHover: attendanceCellHover,
  // The hover card lists more facts than the timesheet's, so it needs more room before it flips above the cell.
  hoverHeight: 250,
  totalColumns: ATTENDANCE_TOTAL_COLUMNS,
  sheet: {
    icon: "o_fact_check",
    tooltip: "View this employee's time-ins and Movement Register entries for the period or any week",
    aria: (row) => `View the attendance of ${row.fullName}`
  },
  foot: {
    label: "Present / expected",
    text: "Per day: employees who timed in or were on Work From Home, out of those expected that day. Days before joining, holidays and full-day leaves are not expected.",
    value: (t) => (t && t.expected ? `${t.present + t.wfh}/${t.expected}` : ""),
    detail: attendanceFootText
  },
  todayText: "Today. Employees who are timed in show as “in now”; the rest are “not in yet” until their shift start plus grace passes.",
  shiftWindow: (rule) => `${fmtTime12(rule.start)}–${fmtTime12(rule.end)}${rule.endsNextDay ? " (next day)" : ""}, late after ${fmtTime12(rule.lateAfter)}`,
  // legend and tiles
  legendItems: ATTENDANCE_LEGEND,
  tiles: attendanceTiles
};

// #endregion TimeInTimeOut-MovementRegister (Attendance Register)
