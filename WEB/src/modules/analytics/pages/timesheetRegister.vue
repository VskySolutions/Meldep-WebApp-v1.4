<template>
  <q-page class="tsr-page">
    <!-- One top card: the title bar, then (collapsible) the filters and the totals. -->
    <q-card flat bordered class="tsr-top">
      <div class="tsr-top__bar">
        <div class="tsr-top__title">
          <app-breadcrumbs :items="breadcrumbs" no-margin />
          <app-info-tip size="18px" color="primary" max-width="460px" anchor="bottom left" self="top left">
            Daily timesheet of every active employee for the chosen period, judged against each employee's shift
            cutoff. Click a tile to keep only the employees it counts, a cell to open that day, the receipt icon
            beside a name to open that employee's timesheet, and "How to use" for the full guide. The arrow on
            the right folds the filters and totals away so the grid gets the whole screen.
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
            @update:filters="filters = $event"
            @apply="load"
            @clear="clearFilters"
          />
          <q-separator />
          <register-summary class="tsr-top__tiles" :summary="summary" :rules="rules" :active="quick" :loading="loading && !data" @toggle="toggleQuick" />
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
        :hide-holidays="filters.hideHolidays"
        @cell="openDay"
        @employee="openEmployee"
        @timesheet="openEmployeeTimesheet({ row: $event })"
      />
      <q-separator />
      <register-legend class="tsr-card__legend" />
    </q-card>

    <q-dialog v-model="dayOpen" position="right" full-height class="app-side-dialog">
      <register-day-panel
        v-if="dayPayload"
        :payload="dayPayload"
        @close="dayOpen = false"
        @timesheet="openEmployeeTimesheet({ row: dayPayload.row, focusDate: dayPayload.day.date, mode: $event.mode })"
      />
    </q-dialog>

    <q-dialog v-model="employeeOpen" position="right" full-height class="app-side-dialog">
      <register-employee-panel
        v-if="employeePayload"
        :payload="employeePayload"
        @close="employeeOpen = false"
        @record="openEmployee"
      />
    </q-dialog>

    <q-dialog v-model="guideOpen" position="right" full-height class="app-side-dialog">
      <register-guide-panel :rules="rules" @close="guideOpen = false" />
    </q-dialog>
  </q-page>
</template>

<script setup>
// Timesheet Register (Analytics). Rows are active employees, columns are the days of the period, cells are
// the verdict the API gave each employee-day. The API does the judging; this page picks the period, filters
// the rows in the browser, sums what is on screen and opens a day, an employee's timesheet or the guide.
// Layout: one top card (title bar · filters · totals) that folds to a single bar, and the grid below it at
// its full height.
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
import RegisterDayPanel from "modules/analytics/components/register/_registerDayPanel.vue";
import RegisterEmployeePanel from "modules/analytics/components/register/_registerEmployeePanel.vue";
import RegisterGuidePanel from "modules/analytics/components/register/_registerGuidePanel.vue";
import analyticsService from "modules/analytics/analytics.service";
import { initEmployeeDialogs, onEmployeeView } from "src/modules/employee/utils/dialogs.js";
import { STATES, FILTERABLE_STATES, fmtHours, exportCellText } from "modules/analytics/utils/registerStates";
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

const breadcrumbs = [{ label: "Analytics" }, { label: "Timesheet Register" }];

// ---- period -------------------------------------------------------------------------------------------
const prefs = usePreferences("analytics-timesheet-register");
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
    data.value = await analyticsService.getTimesheetRegister({ fromDate: range.value.from, toDate: range.value.to });
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
const BLANK_FILTERS = { departments: [], teamLeads: [], teamMembers: [], shifts: [], states: [], hideHolidays: false };
const savedFilters = prefs.get("filters", null);
const stringList = (value) => (Array.isArray(value) ? value.filter((x) => typeof x === "string") : []);

const search = ref(String(prefs.get("search", "") || ""));
const filters = ref({
  departments: stringList(savedFilters?.departments),
  teamLeads: stringList(savedFilters?.teamLeads),
  // "teamMembers" replaced the earlier "employees" filter; an old saved value still applies.
  teamMembers: stringList(savedFilters?.teamLeads).length ? stringList(savedFilters?.teamMembers) : [],
  shifts: stringList(savedFilters?.shifts),
  states: stringList(savedFilters?.states),
  hideHolidays: !!(savedFilters ? savedFilters.hideHolidays : prefs.get("hideHolidays", false))
});
const quick = ref(prefs.get("quick", null) || null);

watch(filters, (value) => prefs.set("filters", value), { deep: true });
watch(search, (value) => prefs.set("search", value));
watch(quick, (value) => prefs.set("quick", value));

const normalize = (text) => String(text || "").toLowerCase();

function rowHasState (row, key) {
  if (key === "noShift") return !row.shiftAssigned;
  if (key === "Leave") return row.cells.some((c) => c.state === "Leave" || c.leave);
  return row.cells.some((c) => c.state === key);
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
    if (f.states.length && !f.states.some((s) => rowHasState(row, s))) return false;
    return true;
  });
});

// Rows on screen: the tile the reader clicked narrows the base rows further.
const visibleRows = computed(() => (quick.value ? baseRows.value.filter((row) => rowHasState(row, quick.value)) : baseRows.value));

function toggleQuick (key) {
  if (key === "expected") return;
  quick.value = quick.value === key ? null : key;
}

const summary = computed(() => {
  const s = { employees: 0, employeesWithoutShift: 0, expectedDays: 0, onTime: 0, delayed: 0, partial: 0, pending: 0, due: 0, leaveDays: 0, halfDays: 0, loggedHours: 0, expectedHours: 0 };
  for (const row of baseRows.value) {
    const t = row.totals || {};
    s.employees++;
    if (!row.shiftAssigned) s.employeesWithoutShift++;
    s.expectedDays += t.expectedDays || 0;
    s.onTime += t.onTime || 0;
    s.delayed += t.delayed || 0;
    s.partial += t.partial || 0;
    s.pending += t.pending || 0;
    s.due += t.due || 0;
    s.leaveDays += t.leave || 0;
    s.halfDays += t.halfDay || 0;
    s.loggedHours += Number(t.loggedHours || 0);
    s.expectedHours += Number(t.expectedHours || 0);
  }
  return s;
});

// The counts kept on the title bar while the top card is folded (each one still toggles its filter).
const miniStats = computed(() => {
  const s = summary.value;
  return [
    { key: "OnTime", label: STATES.OnTime.label, value: s.onTime, icon: STATES.OnTime.icon, colour: STATES.OnTime.colour, hint: STATES.OnTime.description },
    { key: "Delayed", label: STATES.Delayed.label, value: s.delayed, icon: STATES.Delayed.icon, colour: STATES.Delayed.colour, hint: STATES.Delayed.description },
    { key: "Partial", label: STATES.Partial.label, value: s.partial, icon: STATES.Partial.icon, colour: STATES.Partial.colour, hint: STATES.Partial.description },
    { key: "Pending", label: STATES.Pending.label, value: s.pending, icon: STATES.Pending.icon, colour: STATES.Pending.colour, hint: STATES.Pending.description },
    { key: "Due", label: STATES.Due.label, value: s.due, icon: STATES.Due.icon, colour: STATES.Due.colour, hint: STATES.Due.description },
    { key: "Leave", label: "Leave", value: s.leaveDays, icon: "o_beach_access", colour: STATES.Leave.colour, hint: "Employee-days covered by a full-day leave" }
  ];
});

// Per-day footer, from the rows on screen.
const perDay = computed(() => days.value.map((day, i) => {
  const t = { expected: 0, complete: 0, onTime: 0, delayed: 0, partial: 0, pending: 0, due: 0, leave: 0 };
  for (const row of visibleRows.value) {
    const c = row.cells[i];
    if (!c) continue;
    switch (c.state) {
      case "OnTime": t.expected++; t.complete++; t.onTime++; break;
      case "Delayed": t.expected++; t.complete++; t.delayed++; break;
      case "Partial": t.expected++; t.partial++; break;
      case "Pending": t.expected++; t.pending++; break;
      case "Due": t.expected++; t.due++; break;
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
    ...(rules.value?.shifts || []).map((s) => ({ label: `${s.name} (${s.start}–${s.end})`, value: s.name })),
    { label: `Not assigned (judged as ${rules.value?.defaultShift || "default"})`, value: NONE }
  ];
  const states = FILTERABLE_STATES.map((k) => ({ label: STATES[k].label, value: k }));
  return { departments, teamLeads, teamMembers, shifts, states };
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
  return f.departments.length + f.teamLeads.length + f.teamMembers.length + f.shifts.length + f.states.length
    + (f.hideHolidays ? 1 : 0) + (search.value.trim() ? 1 : 0) + (quick.value ? 1 : 0);
});

function clearFilters () {
  filters.value = { ...BLANK_FILTERS };
  quick.value = null;
  search.value = "";
}

// ---- density -----------------------------------------------------------------------------------------
// Hours in every cell unless the person picked another density; the choice is remembered.
const density = ref(prefs.get("density", "hours"));
watch(density, (value) => prefs.set("density", value));
const compact = computed(() => density.value === "compact" || (density.value === "auto" && days.value.length > 14));

// ---- day panel, employee timesheet, employee record & guide ------------------------------------------
const dayOpen = ref(false);
const dayPayload = ref(null);

function openDay (payload) {
  if (!payload?.cell || payload.cell.state === "NotEmployed") return;
  dayPayload.value = payload;
  dayOpen.value = true;
}

// One employee's timesheet for the register's period or for any week (from the receipt icon or the day panel).
const employeeOpen = ref(false);
const employeePayload = ref(null);

function openEmployeeTimesheet ({ row, focusDate = null, mode = "period" }) {
  if (!row?.employeeId) return;
  dayOpen.value = false;
  employeePayload.value = { row, from: range.value.from, to: range.value.to, focusDate, mode };
  employeeOpen.value = true;
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
    ...indexes.map((i) => ({ label: formatDayLabel(days.value[i].date), value: (r) => exportCellText(r.cells[i]) })),
    { label: "Logged hours", value: (r) => fmtHours(r.totals.loggedHours) },
    { label: "Expected hours", value: (r) => fmtHours(r.totals.expectedHours) },
    { label: "Expected days", value: (r) => r.totals.expectedDays },
    { label: "On time", value: (r) => r.totals.onTime },
    { label: "Delayed", value: (r) => r.totals.delayed },
    { label: "Partial", value: (r) => r.totals.partial },
    { label: "Pending", value: (r) => r.totals.pending },
    { label: "Due", value: (r) => r.totals.due },
    { label: "Leave days", value: (r) => r.totals.leave },
    { label: "Half days", value: (r) => r.totals.halfDay }
  ];
  exportCsv({ filename: `timesheet-register_${range.value.from}_${range.value.to}.csv`, columns, rows: visibleRows.value });
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
