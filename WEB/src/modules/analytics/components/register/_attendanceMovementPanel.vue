<template>
  <app-side-panel
    :title="row.fullName || 'Employee'"
    :subtitle="subtitle"
    width="68vw"
    min-width="720px"
    hide-save
    cancel-label="Close"
    :loading="loading && !detail"
    @close="$emit('close')"
  >
    <template #head-actions>
      <q-btn flat dense no-caps color="white" icon="o_badge" label="Employee record" class="q-mr-sm" @click="$emit('record', row)">
        <q-tooltip>Open the employee record</q-tooltip>
      </q-btn>
    </template>

    <!-- Which of the three Movement Register types is listed; the panel opens on the column that was clicked. -->
    <div class="att-mv__bar">
      <div class="att-mv__field">
        <app-field-label label="Show" />
        <app-segmented-control :model-value="kind" :options="kindOptions" aria-label="Movement Register type" @update:model-value="kind = $event" />
      </div>
      <div class="att-mv__range text-caption text-grey-7">{{ rangeLabel }}</div>
    </div>

    <div v-if="loadError" class="text-caption text-negative q-mt-sm">The entries could not be loaded.</div>

    <template v-if="detail">
      <div class="att-mv__tiles">
        <app-stat-tile
          v-for="t in tiles"
          :key="t.key"
          dense
          :label="t.label"
          :value="t.value"
          :caption="t.caption"
          :icon="t.icon"
          :color="t.colour"
          :hint="t.hint"
        />
      </div>

      <!-- One row per date: how the day went, how much was declared, and every entry of that date. -->
      <app-data-table
        class="att-mv__table q-mt-md"
        :title="`${meta.label} by date (${dateRows.length} ${dateRows.length === 1 ? 'date' : 'dates'})`"
        :rows="dateRows"
        :columns="columns"
        row-key="id"
        :page-key="`analytics-attendance-movement-${kind}`"
        dense
        client-sort
        :pagination="pagination"
        :rows-per-page-options="[0]"
        hide-pagination
        :loading="loading"
        @refresh="load"
      >
        <template #actions>
          <q-btn flat dense no-caps color="primary" icon="o_download" label="CSV" :disable="!entries.length" @click="exportEntries">
            <q-tooltip>Download every {{ meta.entry }} of this period for this employee</q-tooltip>
          </q-btn>
        </template>

        <template #no-data>
          <app-empty-state :icon="look.icon" :title="`No ${meta.entries}`" :text="`This employee has no ${meta.entry} in the selected period.`" />
        </template>

        <template #body="scope">
          <q-tr :props="scope">
            <q-td v-for="col in scope.cols" :key="col.name" :props="scope">
              <template v-if="col.name === 'date'">
                <div>{{ formatDayLabel(scope.row.date) }}</div>
                <div v-if="scope.row.day && scope.row.day.isHoliday" class="text-caption text-grey-7">{{ scope.row.day.holidayTitle || "Holiday" }}</div>
              </template>
              <template v-else-if="col.name === 'stateLabel'">
                <span v-if="scope.row.state" class="att-mv__state" :style="{ '--cell-colour': scope.row.state.colour }">
                  <q-icon v-if="scope.row.state.icon" :name="scope.row.state.icon" size="15px" />
                  <b v-else>{{ scope.row.state.glyph }}</b>
                  {{ scope.row.state.label }}
                </span>
              </template>
              <template v-else-if="col.name === 'details'">
                <div v-for="(entry, index) in scope.row.entries" :key="index" class="att-mv__entry">
                  <span class="att-mv__entry-length">{{ movementLength(entry) || "—" }}</span>
                  <span class="att-mv__entry-text">
                    {{ entry.message || "No message" }}
                    <span v-if="entryMeta(entry)" class="att-mv__entry-meta">{{ entryMeta(entry) }}</span>
                  </span>
                </div>
              </template>
              <template v-else>{{ col.value }}</template>
            </q-td>
          </q-tr>
        </template>

        <!-- Totals for the period, under whichever columns are visible. -->
        <template #bottom-row="scope">
          <q-tr class="att-mv__total">
            <q-td v-for="col in scope.cols" :key="col.name" :class="col.align === 'right' ? 'text-right' : 'text-left'">
              <template v-if="col.name === 'date'">Total</template>
              <template v-else-if="col.name === 'count'">{{ entries.length }}</template>
              <template v-else-if="col.name === 'minutes'">{{ totalLength }}</template>
            </q-td>
          </q-tr>
        </template>
      </app-data-table>

      <div class="text-caption text-grey-7 q-mt-sm">{{ meta.note }}</div>
    </template>

    <!-- Everything else about the same employee and period: every day, every time-in and every entry. -->
    <template #footer-actions>
      <q-btn flat no-caps color="primary" icon="o_fact_check" label="View attendance" @click="$emit('attendance')">
        <q-tooltip>This employee's days, time-ins and all Movement Register entries for the selected period</q-tooltip>
      </q-btn>
    </template>
  </app-side-panel>
</template>

<script setup>
// One employee's Movement Register entries of one type (Work From Home, Breaks or Time adjustments) over the
// register's period, date by date: opened by clicking that employee's WFH, Brk or TA total in the grid. Each
// date shows how the day went beside what was declared, so a break total can be read against the hours
// worked and a time adjustment against how late the arrival was.
import { ref, computed, watch } from "vue";
import AppSidePanel from "components/common/AppSidePanel.vue";
import AppEmptyState from "components/common/AppEmptyState.vue";
import AppDataTable from "components/common/AppDataTable.vue";
import AppStatTile from "components/common/AppStatTile.vue";
import AppSegmentedControl from "components/common/AppSegmentedControl.vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";
import analyticsService from "modules/analytics/analytics.service";
import { exportCsv } from "composables/useCsvExport";
import {
  ATTENDANCE_MOVEMENT_KINDS,
  RECORD_LOOK,
  attendanceStateOf,
  movementLength,
  fmtLate,
  fmtMinutes,
  fmtClock,
  fmtStamp
} from "modules/analytics/utils/registerStates";
import { describeRange, formatAppDate, formatDayLabel } from "modules/analytics/composables/useDateRangePresets";

const props = defineProps({
  // { row, from, to, kind } — the register row, the register's period, and the column that was clicked
  // ("Wfh", "Break" or "Adjust").
  payload: { type: Object, required: true }
});

defineEmits(["close", "record", "attendance"]);

const loading = ref(false);
const loadError = ref(false);
const detail = ref(null);

const kindOf = (value) => (ATTENDANCE_MOVEMENT_KINDS[value] ? value : "Break");
const kind = ref(kindOf(props.payload.kind));

const row = computed(() => detail.value?.employee || props.payload.row || {});
const totals = computed(() => row.value.totals || {});
const meta = computed(() => ({ ...ATTENDANCE_MOVEMENT_KINDS[kind.value], note: NOTES[kind.value] }));
const look = computed(() => RECORD_LOOK[meta.value.kind] || RECORD_LOOK.Other);

const NOTES = {
  Wfh: "A day is listed under the date the Work From Home entry was filed for. With a time-in the day is judged like any other; without one it shows as “WFH, no time-in”.",
  Break: "A break is listed under the date it was filed for. Its minutes are taken out of that day's worked hours.",
  Adjust: "A time adjustment is the employee's own declaration (a late arrival, an early login or a shift change). It does not change the hours or the verdict; “Late by” is what the time-in itself shows."
};

const rangeLabel = computed(() => describeRange(props.payload.from, props.payload.to));

const subtitle = computed(() => [
  meta.value.label,
  row.value.employeeCode,
  row.value.department,
  row.value.shift ? (row.value.shiftAssigned ? row.value.shift : `${row.value.shift} (default)`) : ""
].filter(Boolean).join(" · "));

// Every entry of the employee in the period, by the three types (the counts sit on the switch).
const byKind = computed(() => {
  const groups = { Wfh: [], Break: [], Adjust: [] };
  for (const entry of detail.value?.movements || []) {
    const key = Object.keys(ATTENDANCE_MOVEMENT_KINDS).find((k) => ATTENDANCE_MOVEMENT_KINDS[k].kind === entry.kind);
    if (key) groups[key].push(entry);
  }
  return groups;
});

const entries = computed(() => byKind.value[kind.value] || []);

const kindOptions = computed(() => Object.values(ATTENDANCE_MOVEMENT_KINDS).map((k) => ({
  value: k.key,
  label: k.label,
  icon: (RECORD_LOOK[k.kind] || RECORD_LOOK.Other).icon,
  count: detail.value ? byKind.value[k.key].length : undefined,
  tooltip: `This employee's ${k.entries} in the selected period`
})));

// ---- date rows -----------------------------------------------------------------------------------------
function inOutText (cell, date) {
  if (!cell?.firstIn) return "";
  const out = cell.lastOut ? fmtClock(cell.lastOut, date) : (cell.state === "InNow" ? "still in" : "missing");
  return `${fmtClock(cell.firstIn, date)} – ${out}`;
}

// The length of a date: the minutes added up; for Work From Home the durations as picked ("Full Day").
function lengthOf (list) {
  if (kind.value !== "Wfh") return fmtMinutes(list.reduce((sum, e) => sum + (e.minutes || 0), 0));
  return [...new Set(list.map((e) => movementLength(e)).filter(Boolean))].join(", ");
}

// One row per date that has an entry of the chosen type, with the register's verdict for that day beside it.
const dateRows = computed(() => {
  const days = detail.value?.days || [];
  const cells = row.value.cells || [];
  const indexOf = new Map(days.map((d, i) => [d.date, i]));
  const groups = new Map();
  for (const entry of entries.value) {
    if (!groups.has(entry.date)) groups.set(entry.date, []);
    groups.get(entry.date).push(entry);
  }

  return [...groups.entries()].map(([date, list]) => {
    const i = indexOf.get(date);
    const cell = i === undefined ? null : cells[i];
    const state = cell ? attendanceStateOf(cell) : null;
    const sorted = [...list].sort((a, b) => String(a.enteredAt || "").localeCompare(String(b.enteredAt || "")));
    return {
      id: date,
      date,
      day: i === undefined ? null : days[i],
      cell,
      state,
      stateLabel: state?.label || "",
      inOut: inOutText(cell, date),
      count: sorted.length,
      minutes: sorted.reduce((sum, e) => sum + (e.minutes || 0), 0),
      length: lengthOf(sorted),
      worked: cell?.worked || 0,
      lateMinutes: cell?.lateMinutes || 0,
      entries: sorted,
      details: sorted.map((e) => e.message || "").join(" ")
    };
  });
});

const totalLength = computed(() => (kind.value === "Wfh"
  ? `${dateRows.value.length} ${dateRows.value.length === 1 ? "day" : "days"}`
  : fmtMinutes(entries.value.reduce((sum, e) => sum + (e.minutes || 0), 0))));

// What sits beside an entry's message: who gave permission, when it was entered, and whether that was the same day.
function entryMeta (entry) {
  const parts = [];
  if (entry.approver) parts.push(`Permission by ${entry.approver}`);
  if (entry.enteredAt) parts.push(`entered ${entry.enteredDaysLater ? fmtStamp(entry.enteredAt) : fmtClock(entry.enteredAt)}`);
  if (entry.enteredDaysLater > 0) parts.push(`${entry.enteredDaysLater} ${entry.enteredDaysLater === 1 ? "day" : "days"} after the date`);
  if (entry.enteredDaysLater < 0) parts.push(`${-entry.enteredDaysLater} ${entry.enteredDaysLater === -1 ? "day" : "days"} ahead of the date`);
  if (entry.enteredBy && entry.enteredBy !== row.value.fullName) parts.push(`by ${entry.enteredBy}`);
  return parts.join(" · ");
}

// The column that puts the entries in context differs by type: hours worked beside breaks, lateness beside
// time adjustments; Work From Home needs neither.
const columns = computed(() => {
  const list = [
    { name: "date", label: "Date", field: "date", align: "left", sortable: true },
    { name: "stateLabel", label: "Day", field: "stateLabel", align: "left", sortable: true },
    { name: "inOut", label: "Time in – out", field: "inOut", align: "left", sortable: true },
    { name: "count", label: "Entries", field: "count", align: "right", sortable: true },
    { name: "minutes", label: "Length", field: "minutes", align: "right", sortable: true, format: (v, r) => r.length }
  ];
  if (kind.value === "Break") {
    list.push({ name: "worked", label: "Worked", field: "worked", align: "right", sortable: true, format: (v) => (v > 0 ? fmtMinutes(v) : "") });
  }
  if (kind.value === "Adjust") {
    list.push({ name: "lateMinutes", label: "Late by", field: "lateMinutes", align: "right", sortable: true, format: (v) => fmtLate(v) });
  }
  list.push({ name: "details", label: "Entries of the date", field: "details", align: "left", sortable: false });
  return list;
});

// All dates on one page, newest first. rowsNumber is cleared on purpose: with it set, QTable treats the
// table as server-paged and never sorts locally.
const pagination = { rowsPerPage: 0, sortBy: "date", descending: true, rowsNumber: undefined };

// ---- tiles ---------------------------------------------------------------------------------------------
const tiles = computed(() => {
  const t = totals.value;
  const list = entries.value;
  const dates = dateRows.value;
  const colour = look.value.colour;
  const icon = look.value.icon;
  const minutes = list.reduce((sum, e) => sum + (e.minutes || 0), 0);
  const perDate = (value) => (dates.length ? fmtMinutes(Math.round(value / dates.length)) : "");

  if (kind.value === "Wfh") {
    const full = dates.filter((d) => d.entries.some((e) => movementLength(e) === "Full Day")).length;
    return [
      { key: "days", label: "Work From Home days", value: dates.length, caption: `${list.length} ${list.length === 1 ? "entry" : "entries"}`, icon, colour, hint: "Dates in the period with a Work From Home entry." },
      { key: "full", label: "Full days", value: full, caption: `${dates.length - full} half or part days`, icon: "o_today", colour, hint: "Dates whose entry covers the full day; the rest cover a half or a part of it." },
      { key: "noTimeIn", label: "Without a time-in", value: t.wfhNoTimeIn || 0, caption: "Work From Home days", icon: "o_timer_off", colour: "#b5347f", hint: "Work From Home days on which the employee did not time in." }
    ];
  }

  if (kind.value === "Adjust") {
    return [
      { key: "count", label: "Time adjustments", value: list.length, caption: `on ${dates.length} ${dates.length === 1 ? "date" : "dates"}`, icon, colour, hint: "Time Adjustment entries in the period." },
      { key: "minutes", label: "Declared", value: fmtMinutes(minutes), caption: dates.length ? `${perDate(minutes)} a date on average` : "", icon: "o_schedule", colour, hint: "The minutes the employee declared, added up." },
      { key: "late", label: "Late days", value: t.late || 0, caption: `${t.lateUndeclared || 0} with no time adjustment`, icon: "o_schedule", colour: "#a66f00", hint: "Days in the period whose first time-in came after the shift start plus grace, and how many of them have no time adjustment." }
    ];
  }

  const longest = dates.reduce((best, d) => (d.minutes > (best?.minutes || 0) ? d : best), null);
  return [
    { key: "count", label: "Breaks", value: list.length, caption: `on ${dates.length} ${dates.length === 1 ? "date" : "dates"}`, icon, colour, hint: "Break entries in the period." },
    { key: "minutes", label: "Break time", value: fmtMinutes(minutes), caption: dates.length ? `${perDate(minutes)} a date on average` : "", icon: "o_schedule", colour, hint: "The minutes of all breaks, added up. They are taken out of the worked hours." },
    { key: "longest", label: "Longest date", value: longest ? fmtMinutes(longest.minutes) : "0 m", caption: longest ? formatDayLabel(longest.date) : "", icon: "o_trending_up", colour, hint: "The date with the most break time." }
  ];
});

// ---- export --------------------------------------------------------------------------------------------
function exportEntries () {
  const r = row.value;
  const who = String(r.employeeCode || r.fullName || "employee").replace(/\s+/g, "_");
  const dateOf = new Map(dateRows.value.map((d) => [d.date, d]));
  exportCsv({
    filename: `attendance-${kind.value.toLowerCase()}_${who}_${props.payload.from}_${props.payload.to}.csv`,
    columns: [
      { label: "Date", value: (e) => formatAppDate(e.date) },
      { label: "Type", value: (e) => e.type || meta.value.label },
      { label: "Minutes", value: (e) => e.minutes || 0 },
      { label: "Length", value: (e) => movementLength(e) },
      { label: "Message", value: (e) => e.message || "" },
      { label: "Permission by", value: (e) => e.approver || "" },
      { label: "Entered at", value: (e) => fmtStamp(e.enteredAt) },
      { label: "Entered by", value: (e) => e.enteredBy || "" },
      { label: "Day", value: (e) => dateOf.get(e.date)?.stateLabel || "" },
      { label: "Time in - out", value: (e) => (dateOf.get(e.date)?.inOut || "").replace("–", "-") },
      { label: "Worked minutes", value: (e) => dateOf.get(e.date)?.worked || 0 },
      { label: "Late minutes", value: (e) => dateOf.get(e.date)?.lateMinutes || 0 }
    ],
    rows: [...entries.value].sort((a, b) => String(a.date).localeCompare(String(b.date)) || String(a.enteredAt || "").localeCompare(String(b.enteredAt || "")))
  });
}

// ---- loading -------------------------------------------------------------------------------------------
async function load () {
  const employeeId = props.payload.row?.employeeId;
  const { from, to } = props.payload;
  if (!employeeId || !from || !to) return;
  loading.value = true;
  loadError.value = false;
  try {
    detail.value = await analyticsService.getAttendanceRegisterEmployee(employeeId, from, to);
  } catch {
    loadError.value = true;
  } finally {
    loading.value = false;
  }
}

watch(() => props.payload, (p) => {
  kind.value = kindOf(p?.kind);
  load();
}, { immediate: true });
</script>

<style scoped>
.att-mv__bar {
  display: flex;
  align-items: flex-end;
  flex-wrap: wrap;
  gap: 8px 14px;
  margin-bottom: 12px;
}
.att-mv__field {
  display: flex;
  flex-direction: column;
}
.att-mv__field :deep(.app-segmented) {
  height: 40px;
}
.att-mv__field :deep(.app-segmented__item) {
  min-height: 38px;
}
.att-mv__range {
  padding-bottom: 10px;
}
.att-mv__tiles {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.att-mv__tiles > * {
  flex: 1 1 180px;
}
/* Table: header in the theme blue with white text, like the register grid. */
.att-mv__table :deep(thead tr th) {
  background: var(--q-primary);
  color: #fff;
  font-weight: 600;
}
.att-mv__table :deep(thead tr th .q-table__sort-icon),
.att-mv__table :deep(thead tr th.sortable:hover) {
  color: #fff;
}
.att-mv__table :deep(td) {
  vertical-align: top;
}
.att-mv__total td {
  font-weight: 600;
  background: #f7fafb;
  border-top: 2px solid var(--line, #dde6e8);
}
.att-mv__state {
  --cell-colour: #647a82;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 1px 8px 1px 6px;
  border-radius: 10px;
  background: color-mix(in srgb, var(--cell-colour) 12%, #fff);
  white-space: nowrap;
}
.att-mv__state .q-icon,
.att-mv__state b {
  color: var(--cell-colour);
}
/* One entry of the date: its length, then the message with who and when underneath it. */
.att-mv__entry {
  display: grid;
  grid-template-columns: 74px minmax(0, 1fr);
  column-gap: 8px;
  min-width: 260px;
  white-space: normal;
}
.att-mv__entry + .att-mv__entry {
  margin-top: 4px;
  padding-top: 4px;
  border-top: 1px dashed var(--line, #dde6e8);
}
.att-mv__entry-length {
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}
.att-mv__entry-text {
  overflow-wrap: anywhere;
}
.att-mv__entry-meta {
  display: block;
  font-size: 11.5px;
  color: var(--ink-500, #647a82);
}
</style>
