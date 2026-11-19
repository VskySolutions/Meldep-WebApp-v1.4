<template>
  <q-page class="tsr-page">
    <!-- One top card: the title bar, then (collapsible) the filters and the totals. -->
    <q-card flat bordered class="tsr-top">
      <div class="tsr-top__bar">
        <div class="tsr-top__title">
          <app-breadcrumbs :items="breadcrumbs" no-margin />
          <app-info-tip size="18px" color="primary" max-width="480px" anchor="bottom left" self="top left">
            Time In / Time Out and the Movement Register in one view: every active employee against every day of
            the chosen period. A cell says how the day went (on time, late, short hours, no time-out, absent) and
            carries its breaks, time adjustments and work from home. The columns on the right give the hours of
            the period: required, clocked, breaks, time adjustments, productive (clocked minus breaks and time
            adjustments) and how many hours short of required. Click a tile to keep only the employees it
            counts, a cell to open that day, the icon beside a name to open that employee's records, an
            employee's WFH, Brk or TA total to see those entries date by date, and "How to use" for the full
            guide. The arrow on the right folds the filters and totals away so the grid gets the whole screen.
          </app-info-tip>
          <div v-if="data" class="tsr-top__caption text-caption text-grey-7">
            {{ periodLabel }} · <b>{{ visibleRows.length }}</b>/{{ rows.length }} employees
            <template v-if="!collapsed"> · as of {{ formatLocalMinute(data.generatedAt) }}</template>
          </div>
        </div>

        <!-- Folded: the key counts stay on the bar, so nothing is lost by collapsing. -->
        <div v-if="collapsed && data" class="tsr-top__mini" role="group" aria-label="Totals">
          <button
            v-for="m in miniStats"
            :key="m.key"
            type="button"
            :class="['tsr-mini', { 'tsr-mini--on': quick === m.key }]"
            :style="{ '--cell-colour': m.colour }"
            :title="m.hint"
            @click="toggleQuick(m.key)"
          >
            <q-icon :name="m.icon" size="14px" />
            <span>{{ m.label }}</span>
            <b>{{ m.value }}</b>
          </button>
          <q-chip
            v-if="activeFilterCount"
            dense
            clickable
            color="blue-1"
            text-color="primary"
            icon="o_filter_alt"
            @click="collapsed = false"
          >
            {{ activeFilterCount === 1 ? "1 filter" : `${activeFilterCount} filters` }}
          </q-chip>
        </div>

        <div class="tsr-top__tools">
          <q-btn outline no-caps color="primary" icon="o_download" label="Export CSV" :disable="!visibleRows.length" @click="exportRegister">
            <q-tooltip>Download the grid as shown, one row per employee</q-tooltip>
          </q-btn>
          <q-btn outline no-caps color="primary" icon="o_help_outline" label="How to use" @click="openGuide" />
          <q-btn flat round dense color="grey-8" icon="o_refresh" aria-label="Reload" :loading="loading" @click="load">
            <q-tooltip>Reload</q-tooltip>
          </q-btn>
          <q-btn
            flat
            round
            dense
            color="grey-8"
            :icon="collapsed ? 'o_expand_more' : 'o_expand_less'"
            :aria-label="collapsed ? 'Show filters and totals' : 'Hide filters and totals'"
            :aria-expanded="String(!collapsed)"
            @click="collapsed = !collapsed"
          >
            <q-tooltip>{{ collapsed ? "Show filters and totals" : "Hide filters and totals to give the grid the whole screen" }}</q-tooltip>
          </q-btn>
        </div>
      </div>

      <q-slide-transition>
        <div v-show="!collapsed">
          <q-separator />
          <register-filters
            v-model:preset="preset"
            v-model:from="range.from"
            v-model:to="range.to"
            v-model:week-end="weekEnd"
            v-model:search="search"
            v-model:density="density"
            :filters="filters"
            :options="filterOptions"
            :loading="loading"
            :range-error="rangeError"
            :active-count="activeFilterCount"
            :density-options="DENSITIES"
            density-label="Cell shows"
            :extra-selects="EXTRA_SELECTS"
            @update:filters="filters = $event"
            @apply="load"
            @clear="clearFilters"
          />
          <q-separator />
          <register-summary
            class="tsr-top__tiles"
            :summary="summary"
            :rules="rules"
            :active="quick"
            :loading="loading && !data"
            :vocab="ATTENDANCE_VOCAB"
            @toggle="toggleQuick"
          />
        </div>
      </q-slide-transition>
    </q-card>

    <!-- The grid at its full height; the page scrolls. -->
    <q-card flat bordered class="tsr-card">
      <div v-if="loading && !data" class="row flex-center q-pa-xl">
        <q-spinner color="primary" size="40px" />
      </div>
      <app-empty-state
        v-else-if="!data && !loading"
        icon="o_date_range"
        title="Pick a period"
        text="Choose a preset or a custom range above to load the register."
      />
      <app-empty-state
        v-else-if="data && !rows.length"
        icon="o_badge"
        title="No employees to list"
        text="No active employee with a department record was found on this site."
      />
      <app-empty-state
        v-else-if="data && !visibleRows.length"
        icon="o_filter_alt_off"
        title="Nothing matches"
        text="No employee matches the search, the filters or the tile you picked."
      >
        <q-btn flat no-caps color="primary" label="Clear filters" @click="clearFilters" />
      </app-empty-state>
      <register-grid
        v-else-if="data"
        :days="days"
        :rows="visibleRows"
        :per-day="perDay"
        :rules="rules"
        :compact="compact"
        :mode="cellMode"
        :hide-holidays="filters.hideHolidays"
        :vocab="ATTENDANCE_VOCAB"
        @cell="openDay"
        @employee="openEmployee"
        @timesheet="openEmployeeAttendance({ row: $event })"
        @total="openMovement"
      />
      <q-separator />
      <register-legend class="tsr-card__legend" :vocab="ATTENDANCE_VOCAB" />
    </q-card>

    <q-dialog v-model="dayOpen" position="right" full-height class="app-side-dialog">
      <attendance-day-panel
        v-if="dayPayload"
        :payload="dayPayload"
        @close="dayOpen = false"
        @attendance="openEmployeeAttendance({ row: dayPayload.row, focusDate: dayPayload.day.date, mode: $event.mode })"
      />
    </q-dialog>

    <q-dialog v-model="employeeOpen" position="right" full-height class="app-side-dialog">
      <attendance-employee-panel
        v-if="employeePayload"
        :payload="employeePayload"
        @close="employeeOpen = false"
        @record="openEmployee"
      />
    </q-dialog>

    <!-- An employee's WFH, Brk or TA total, opened date by date. -->
    <q-dialog v-model="movementOpen" position="right" full-height class="app-side-dialog">
      <attendance-movement-panel
        v-if="movementPayload"
        :payload="movementPayload"
        @close="movementOpen = false"
        @record="openEmployee"
        @attendance="openEmployeeAttendance({ row: movementPayload.row })"
      />
    </q-dialog>

    <q-dialog v-model="guideOpen" position="right" full-height class="app-side-dialog">
      <attendance-guide-panel :rules="rules" @close="guideOpen = false" />
    </q-dialog>
  </q-page>
</template>

<script setup>
// Attendance Register (Analytics): TimeInTimeOut and the Movement Register merged into one view. Rows are
// active employees, columns are the days of the period, cells are the verdict the API gave each employee-day
// together with what the Movement Register holds for it. The API does the judging; this page picks the
// period, filters the rows in the browser, sums what is on screen and opens a day, an employee's records or
// the guide. Same shell as the Timesheet Register and the same shared components, drawn with the attendance
// vocabulary (utils/registerStates, region TimeInTimeOut-MovementRegister).
import { ref, computed, watch, onMounted } from "vue";
import { usePreferences } from "composables/usePreferences";
import { exportCsv } from "composables/useCsvExport";
import AppBreadcrumbs from "components/common/AppBreadcrumbs.vue";
import AppInfoTip from "components/common/AppInfoTip.vue";
import AppEmptyState from "components/common/AppEmptyState.vue";
import RegisterFilters from "modules/analytics/components/register/_registerFilters.vue";
import RegisterSummary from "modules/analytics/components/register/_registerSummary.vue";
import RegisterGrid from "modules/analytics/components/register/_registerGrid.vue";
import RegisterLegend from "modules/analytics/components/register/_registerLegend.vue";
import AttendanceDayPanel from "modules/analytics/components/register/_attendanceDayPanel.vue";
import AttendanceEmployeePanel from "modules/analytics/components/register/_attendanceEmployeePanel.vue";
import AttendanceMovementPanel from "modules/analytics/components/register/_attendanceMovementPanel.vue";
import AttendanceGuidePanel from "modules/analytics/components/register/_attendanceGuidePanel.vue";
import analyticsService from "modules/analytics/analytics.service";
import { initEmployeeDialogs, onEmployeeView } from "src/modules/employee/utils/dialogs.js";
import {
  ATTENDANCE_VOCAB,
  ATTENDANCE_STATES,
  ATTENDANCE_EXPECTED_STATES,
  ATTENDANCE_PRESENT_STATES,
  ATTENDANCE_DAY_FILTERS,
  ATTENDANCE_MOVEMENT_FILTERS,
  attendanceCellHas,
  attendanceExportCellText,
  fmtHours,
  fmtTime12
} from "modules/analytics/utils/registerStates";
import {
  MAX_RANGE_DAYS,
  todayIso,
  daysBetween,
  rangeForPreset,
  sundayOf,
  weekRangeEnding,
  describeRange,
  formatDayLabel,
  formatLocalMinute
} from "modules/analytics/composables/useDateRangePresets";

const NONE = "__none__";
const ISO_RE = /^\d{4}-\d{2}-\d{2}$/;

const breadcrumbs = [{ label: "Analytics" }, { label: "Attendance Register" }];

// What a cell shows: the worked hours, the first time-in and last time-out, or the icon alone.
const DENSITIES = [
  { value: "auto", label: "Auto", tooltip: "Hours for up to two weeks, compact beyond" },
  { value: "hours", label: "Hours", icon: "o_view_comfy", tooltip: "The worked hours in every cell" },
  { value: "inout", label: "In–Out", icon: "o_schedule", tooltip: "The first time-in and the last time-out in every cell" },
  { value: "compact", label: "Compact", icon: "o_view_compact", tooltip: "Icons only; fits a whole month" }
];

// The one filter the Timesheet Register does not have.
const EXTRA_SELECTS = [
  {
    key: "movements",
    label: "Movement",
    placeholder: "All entries",
    info: "Keeps the employees who have at least one Movement Register entry of the chosen types within the period."
  }
];

// ---- period -------------------------------------------------------------------------------------------
const prefs = usePreferences("analytics-attendance-register");
const today = todayIso();

const preset = ref(prefs.get("preset", "thisWeek"));
const savedCustom = prefs.get("customRange", null);
// "By Weeks": the Sunday that ends the chosen week (Monday to Sunday), remembered across visits.
const weekEnd = ref(sundayOf(prefs.get("weekEnd", "")) || sundayOf(today));
if (preset.value !== "custom" && preset.value !== "byWeek" && !rangeForPreset(preset.value, today)) preset.value = "thisWeek";
const initialRange = preset.value === "custom" && savedCustom?.from && savedCustom?.to
  ? savedCustom
  : preset.value === "byWeek"
    ? weekRangeEnding(weekEnd.value)
    : (rangeForPreset(preset.value, today) || rangeForPreset("thisWeek", today));
const range = ref({ from: initialRange.from, to: initialRange.to });

const rangeError = computed(() => {
  const { from, to } = range.value;
  if (!from || !to) return "Pick both dates.";
  if (!ISO_RE.test(from) || !ISO_RE.test(to)) return "Dates must be complete.";
  const n = daysBetween(from, to);
  if (n < 1) return "\"From\" is after \"To\".";
  if (n > MAX_RANGE_DAYS) return `At most ${MAX_RANGE_DAYS} days at a time (you picked ${n}).`;
  return "";
});

const periodLabel = computed(() => describeRange(range.value.from, range.value.to));

watch(preset, (value) => {
  prefs.set("preset", value);
  if (value === "custom") return;
  const next = value === "byWeek" ? weekRangeEnding(weekEnd.value) : rangeForPreset(value, today);
  if (next) {
    range.value = next;
    load();
  }
});

watch(weekEnd, (value) => {
  prefs.set("weekEnd", value);
  if (preset.value !== "byWeek") return;
  const next = weekRangeEnding(value);
  if (next) {
    range.value = next;
    load();
  }
});

// ---- data --------------------------------------------------------------------------------------------
const loading = ref(false);
const data = ref(null);
const days = computed(() => data.value?.days || []);
const rows = computed(() => data.value?.rows || []);
const rules = computed(() => data.value?.rules || null);

async function load () {
  if (rangeError.value) return;
  loading.value = true;
  try {
    data.value = await analyticsService.getAttendanceRegister({ fromDate: range.value.from, toDate: range.value.to });
    if (preset.value === "custom") prefs.set("customRange", { from: range.value.from, to: range.value.to });
  } catch {
    // boot/interceptors.js already toasts the failure; the previous register stays on screen.
  } finally {
    loading.value = false;
  }
}

// ---- collapse ----------------------------------------------------------------------------------------
const collapsed = ref(!!prefs.get("topCollapsed", false));
watch(collapsed, (value) => prefs.set("topCollapsed", value));

// ---- search, filters, quick tiles ---------------------------------------------------------------------
// Filters persist per user and site like the other lists; a first visit starts clean on This Week.
const BLANK_FILTERS = { departments: [], teamLeads: [], teamMembers: [], shifts: [], states: [], movements: [], hideHolidays: false };
const savedFilters = prefs.get("filters", null);
const stringList = (value) => (Array.isArray(value) ? value.filter((x) => typeof x === "string") : []);

const search = ref(String(prefs.get("search", "") || ""));
const filters = ref({
  departments: stringList(savedFilters?.departments),
  teamLeads: stringList(savedFilters?.teamLeads),
  teamMembers: stringList(savedFilters?.teamLeads).length ? stringList(savedFilters?.teamMembers) : [],
  shifts: stringList(savedFilters?.shifts),
  states: stringList(savedFilters?.states),
  movements: stringList(savedFilters?.movements),
  hideHolidays: !!savedFilters?.hideHolidays
});
const quick = ref(prefs.get("quick", null) || null);

watch(filters, (value) => prefs.set("filters", value), { deep: true });
watch(search, (value) => prefs.set("search", value));
watch(quick, (value) => prefs.set("quick", value));

const normalize = (text) => String(text || "").toLowerCase();

// A row answers to a tile, a day-state filter or a movement filter when any of its days does.
function rowHas (row, key) {
  if (key === "noShift") return !row.shiftAssigned;
  // The hours balance belongs to the period, not to one day.
  if (key === "ShortHours") return row.totals?.shortMinutes > 0;
  return row.cells.some((c) => attendanceCellHas(c, key));
}

// Team-lead semantics (as the org-structure reports): an employee under one of the chosen leads, a chosen
// lead themselves, or, when "No team lead (NA)" is chosen, an employee with no lead at all.
function underLeads (row, leads) {
  if (!leads.length) return true;
  return leads.includes(row.teamLeadId || NONE) || leads.includes(row.employeeId);
}

// Rows after search and the filters: what the tiles count.
const baseRows = computed(() => {
  const q = normalize(search.value).trim();
  const f = filters.value;
  return rows.value.filter((row) => {
    if (q && !(normalize(row.fullName).includes(q) || normalize(row.employeeCode).includes(q) || normalize(row.department).includes(q))) return false;
    if (f.departments.length && !f.departments.includes(row.department || NONE)) return false;
    if (!underLeads(row, f.teamLeads)) return false;
    if (f.teamMembers.length && !f.teamMembers.includes(row.employeeId)) return false;
    if (f.shifts.length && !f.shifts.includes(row.shiftAssigned ? row.shift : NONE)) return false;
    if (f.states.length && !f.states.some((s) => rowHas(row, s))) return false;
    if (f.movements.length && !f.movements.some((m) => rowHas(row, m))) return false;
    return true;
  });
});

// Rows on screen: the tile the reader clicked narrows the base rows further.
const visibleRows = computed(() => (quick.value ? baseRows.value.filter((row) => rowHas(row, quick.value)) : baseRows.value));

function toggleQuick (key) {
  if (key === "expected") return;
  quick.value = quick.value === key ? null : key;
}

// The tiles: every total of the listed employees, added up.
const summary = computed(() => {
  const s = { employees: 0, employeesWithoutShift: 0, employeesShort: 0 };
  for (const row of baseRows.value) {
    s.employees++;
    if (!row.shiftAssigned) s.employeesWithoutShift++;
    if (row.totals?.shortMinutes > 0) s.employeesShort++;
    for (const [key, value] of Object.entries(row.totals || {})) s[key] = (s[key] || 0) + Number(value || 0);
  }
  return s;
});

// The counts kept on the title bar while the top card is folded (each one still toggles its filter).
const miniStats = computed(() => {
  const s = summary.value;
  const A = ATTENDANCE_STATES;
  const stats = [
    { key: "OnTime", label: A.OnTime.label, value: s.onTime || 0, icon: A.OnTime.icon, colour: A.OnTime.colour, hint: A.OnTime.description },
    { key: "Late", label: A.Late.label, value: s.late || 0, icon: A.Late.icon, colour: A.Late.colour, hint: "Days whose first time-in came after the shift start plus grace" },
    { key: "Short", label: A.Short.label, value: s.short || 0, icon: A.Short.icon, colour: A.Short.colour, hint: A.Short.description },
    { key: "NoTimeOut", label: A.NoTimeOut.label, value: s.noTimeOut || 0, icon: A.NoTimeOut.icon, colour: A.NoTimeOut.colour, hint: A.NoTimeOut.description },
    { key: "Absent", label: A.Absent.label, value: s.absent || 0, icon: A.Absent.icon, colour: A.Absent.colour, hint: A.Absent.description }
  ];
  if (s.inNow || s.awaited) {
    stats.push({ key: "InNow", label: A.InNow.label, value: s.inNow || 0, icon: A.InNow.icon, colour: A.InNow.colour, hint: A.InNow.description });
  }
  stats.push(
    { key: "Wfh", label: "WFH", value: s.wfh || 0, icon: "o_home_work", colour: A.Wfh.colour, hint: "Days with a Work From Home entry in the Movement Register" },
    { key: "Leave", label: "Leave", value: s.leave || 0, icon: "o_beach_access", colour: A.Leave.colour, hint: "Employee-days covered by a full-day leave" }
  );
  return stats;
});

// Per-day footer, from the rows on screen.
const perDay = computed(() => days.value.map((day, i) => {
  const t = { expected: 0, present: 0, wfh: 0, absent: 0, awaited: 0, onTime: 0, late: 0, short: 0, noTimeOut: 0, inNow: 0, leave: 0 };
  for (const row of visibleRows.value) {
    const c = row.cells[i];
    if (!c) continue;
    if (ATTENDANCE_EXPECTED_STATES.includes(c.state)) t.expected++;
    if (ATTENDANCE_PRESENT_STATES.includes(c.state)) t.present++;
    if (c.lateMinutes > 0) t.late++;
    switch (c.state) {
      case "OnTime": t.onTime++; break;
      case "Short": t.short++; break;
      case "NoTimeOut": t.noTimeOut++; break;
      case "InNow": t.inNow++; break;
      case "Wfh": t.wfh++; break;
      case "Absent": t.absent++; break;
      case "Awaited": t.awaited++; break;
      case "Leave": t.leave++; break;
      default: break;
    }
  }
  return t;
}));

const uniqueSorted = (values) => [...new Set(values)].sort((a, b) => String(a).localeCompare(String(b)));
const memberOption = (r) => ({ label: r.employeeCode ? `${r.fullName} (${r.employeeCode})` : r.fullName, value: r.employeeId });

const filterOptions = computed(() => {
  const departments = uniqueSorted(rows.value.map((r) => r.department || NONE))
    .map((v) => ({ label: v === NONE ? "No department" : v, value: v }));

  // Team leads of the listed employees, plus "NA" for the ones who have none.
  const leadNames = new Map();
  for (const r of rows.value) {
    if (r.teamLeadId) leadNames.set(r.teamLeadId, r.teamLeadName || r.teamLeadId);
  }
  const teamLeads = [
    { label: "No team lead (NA)", value: NONE },
    ...[...leadNames.entries()]
      .sort((a, b) => a[1].localeCompare(b[1]))
      .map(([value, label]) => ({ label, value }))
  ];

  // Members follow the chosen leads (everyone when no lead is chosen).
  const teamMembers = rows.value
    .filter((r) => underLeads(r, filters.value.teamLeads))
    .map(memberOption);

  const shifts = [
    ...(rules.value?.shifts || []).map((s) => ({ label: `${s.name} (${fmtTime12(s.start)}–${fmtTime12(s.end)})`, value: s.name })),
    { label: `Not assigned (judged as ${rules.value?.defaultShift || "default"})`, value: NONE }
  ];
  return { departments, teamLeads, teamMembers, shifts, states: ATTENDANCE_DAY_FILTERS, movements: ATTENDANCE_MOVEMENT_FILTERS };
});

// Team Members only shows once a lead is chosen; a change of leads drops members no longer under them.
watch(() => filters.value.teamLeads, (leads) => {
  if (!filters.value.teamMembers.length) return;
  if (!leads.length) {
    filters.value = { ...filters.value, teamMembers: [] };
    return;
  }
  const allowed = new Set(rows.value.filter((r) => underLeads(r, leads)).map((r) => r.employeeId));
  const kept = filters.value.teamMembers.filter((id) => allowed.has(id));
  if (kept.length !== filters.value.teamMembers.length) filters.value = { ...filters.value, teamMembers: kept };
});

// Active filters, counted for the Clear button and the folded bar (the period is not a filter).
const activeFilterCount = computed(() => {
  const f = filters.value;
  return f.departments.length + f.teamLeads.length + f.teamMembers.length + f.shifts.length + f.states.length + f.movements.length
    + (f.hideHolidays ? 1 : 0) + (search.value.trim() ? 1 : 0) + (quick.value ? 1 : 0);
});

function clearFilters () {
  filters.value = { ...BLANK_FILTERS };
  quick.value = null;
  search.value = "";
}

// ---- what a cell shows -------------------------------------------------------------------------------
// Worked hours in every cell unless the person picked another reading; the choice is remembered.
const density = ref(prefs.get("density", "hours"));
if (!DENSITIES.some((d) => d.value === density.value)) density.value = "hours";
watch(density, (value) => prefs.set("density", value));
const compact = computed(() => density.value === "compact" || (density.value === "auto" && days.value.length > 14));
const cellMode = computed(() => (density.value === "inout" ? "inout" : "hours"));

// ---- day panel, employee attendance, employee record & guide -----------------------------------------
const dayOpen = ref(false);
const dayPayload = ref(null);

function openDay (payload) {
  if (!payload?.cell || payload.cell.state === "NotEmployed") return;
  dayPayload.value = payload;
  dayOpen.value = true;
}

// One employee's time-ins and Movement Register entries for the register's period or for any week (from the
// icon beside a name or from the day panel).
const employeeOpen = ref(false);
const employeePayload = ref(null);

function openEmployeeAttendance ({ row, focusDate = null, mode = "period" }) {
  if (!row?.employeeId) return;
  dayOpen.value = false;
  movementOpen.value = false;
  employeePayload.value = { row, from: range.value.from, to: range.value.to, focusDate, mode };
  employeeOpen.value = true;
}

// One employee's Work From Home days, breaks or time adjustments for the register's period, date by date:
// opened by clicking that employee's WFH, Brk or TA total (the column's action names the type).
const movementOpen = ref(false);
const movementPayload = ref(null);

function openMovement ({ row, column }) {
  if (!row?.employeeId || !column?.action) return;
  movementPayload.value = { row, from: range.value.from, to: range.value.to, kind: column.action };
  movementOpen.value = true;
}

// The same employee view the employee list opens from its row actions.
const activeEmployeeId = ref(null);
initEmployeeDialogs(activeEmployeeId);

function openEmployee (row) {
  if (row?.employeeId) onEmployeeView(row.employeeId);
}

const guideOpen = ref(false);

function openGuide () {
  guideOpen.value = true;
  prefs.set("guideSeen", true);
}

// ---- export ------------------------------------------------------------------------------------------
function exportRegister () {
  const indexes = days.value.map((d, i) => i).filter((i) => !filters.value.hideHolidays || !days.value[i].isHoliday);
  const columns = [
    { label: "Employee", value: (r) => r.fullName },
    { label: "Code", value: (r) => r.employeeCode || "" },
    { label: "Department", value: (r) => r.department || "" },
    { label: "Team lead", value: (r) => r.teamLeadName || "" },
    { label: "Shift", value: (r) => (r.shiftAssigned ? r.shift : `${r.shift} (not assigned)`) },
    ...indexes.map((i) => ({ label: formatDayLabel(days.value[i].date), value: (r) => attendanceExportCellText(r.cells[i]) })),
    { label: "Required hours", value: (r) => fmtHours((r.totals.requiredMinutes || 0) / 60) },
    { label: "Clocked hours", value: (r) => fmtHours((r.totals.clockedMinutes || 0) / 60) },
    { label: "Break hours", value: (r) => fmtHours(r.totals.breakMinutes / 60) },
    { label: "Time adjustment hours", value: (r) => fmtHours(r.totals.adjustMinutes / 60) },
    { label: "Productive hours", value: (r) => fmtHours((r.totals.workedMinutes || 0) / 60) },
    { label: "Hours short of required", value: (r) => fmtHours((r.totals.shortMinutes || 0) / 60) },
    { label: "Hours above required", value: (r) => fmtHours((r.totals.extraMinutes || 0) / 60) },
    { label: "Expected days", value: (r) => r.totals.expectedDays },
    { label: "Present days", value: (r) => r.totals.present },
    { label: "On time", value: (r) => r.totals.onTime },
    { label: "Late", value: (r) => r.totals.late },
    { label: "Late, no time adjustment", value: (r) => r.totals.lateUndeclared },
    { label: "Late minutes", value: (r) => r.totals.lateMinutes },
    { label: "Short hours", value: (r) => r.totals.short },
    { label: "No time-out", value: (r) => r.totals.noTimeOut },
    { label: "Absent", value: (r) => r.totals.absent },
    { label: "WFH days", value: (r) => r.totals.wfh },
    { label: "WFH without time-in", value: (r) => r.totals.wfhNoTimeIn },
    { label: "Leave days", value: (r) => r.totals.leave },
    { label: "Half days", value: (r) => r.totals.halfDay },
    { label: "Breaks", value: (r) => r.totals.breaks },
    { label: "Time adjustments", value: (r) => r.totals.adjustments }
  ];
  exportCsv({ filename: `attendance-register_${range.value.from}_${range.value.to}.csv`, columns, rows: visibleRows.value });
}

// ---- start -------------------------------------------------------------------------------------------
onMounted(() => {
  load();
  // First visit: open the guide once so the register is never a wall of colours without a key.
  if (!prefs.get("guideSeen", false)) {
    guideOpen.value = true;
    prefs.set("guideSeen", true);
  }
});
</script>

<style scoped>
/* Tighter than q-page padding: the grid is the point of this page. */
.tsr-page {
  padding: 10px 12px 12px;
}
.tsr-top,
.tsr-card {
  border-radius: 12px;
}
.tsr-top {
  margin-bottom: 8px;
}
.tsr-top__bar {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 6px 12px;
  padding: 6px 14px;
  min-height: 48px;
}
.tsr-top__title {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px 10px;
  min-width: 0;
}
.tsr-top__caption {
  white-space: nowrap;
}
.tsr-top__mini {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px 6px;
  flex: 1 1 auto;
  justify-content: flex-end;
}
.tsr-top__tools {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 6px;
  margin-left: auto;
}
.tsr-top__tiles {
  padding: 8px 14px 10px;
  margin-bottom: 0 !important;
}
.tsr-card__legend {
  padding: 5px 14px;
}

/* The folded totals: a tinted pill with icon, name and count; click toggles the same filter as the tile. */
.tsr-mini {
  --cell-colour: #647a82;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 2px 9px 2px 7px;
  border: 1px solid transparent;
  border-radius: 12px;
  background: color-mix(in srgb, var(--cell-colour) 12%, #fff);
  color: var(--ink-700, #3a4d54);
  font: inherit;
  font-size: 12px;
  cursor: pointer;
}
.tsr-mini .q-icon {
  color: var(--cell-colour);
}
.tsr-mini b {
  color: var(--ink-900, #16262c);
  font-variant-numeric: tabular-nums;
}
.tsr-mini:hover,
.tsr-mini--on {
  border-color: var(--cell-colour);
}
.tsr-mini--on {
  box-shadow: inset 0 0 0 1px var(--cell-colour);
}
</style>
