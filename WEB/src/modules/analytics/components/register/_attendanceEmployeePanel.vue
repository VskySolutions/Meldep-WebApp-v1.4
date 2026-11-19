<template>
  <app-side-panel
    :title="row.fullName || 'Employee'"
    :subtitle="subtitle"
    width="75vw"
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

    <!-- Range: the register's period, or any week picked by its Sunday. -->
    <div class="att-emp__bar">
      <div class="att-emp__field">
        <app-field-label label="Show" />
        <app-segmented-control :model-value="mode" :options="modeOptions" aria-label="Range" @update:model-value="mode = $event" />
      </div>
      <register-week-picker v-if="mode === 'week'" v-model="weekEnd" />
      <div class="att-emp__range text-caption text-grey-7">{{ rangeLabel }}</div>
    </div>

    <div v-if="loadError" class="text-caption text-negative q-mt-sm">The attendance could not be loaded.</div>

    <template v-if="detail">
      <div class="att-emp__facts">
        <div v-for="f in facts" :key="f.label">
          <div class="record-label">{{ f.label }}</div>
          <div class="record-value">{{ f.value }}</div>
        </div>
      </div>

      <div class="att-emp__tiles">
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

      <!-- One row per day: the verdict, the time in and out, the hours, and what the Movement Register holds. -->
      <app-data-table
        class="att-emp__table q-mt-md"
        :title="`Days (${dayRows.length})`"
        :rows="dayRows"
        :columns="dayColumns"
        row-key="id"
        page-key="analytics-attendance-employee-days"
        dense
        client-sort
        :pagination="dayPagination"
        :rows-per-page-options="[0]"
        hide-pagination
        :loading="loading"
        @refresh="load"
      >
        <template #actions>
          <q-btn flat dense no-caps color="primary" icon="o_download" label="CSV" :disable="!dayRows.length" @click="exportDays">
            <q-tooltip>Download one row per day of this range</q-tooltip>
          </q-btn>
        </template>

        <template #no-data>
          <app-empty-state icon="o_event_busy" title="No days" text="The employee was not employed in this range." />
        </template>

        <template #body="scope">
          <q-tr :props="scope">
            <q-td v-for="col in scope.cols" :key="col.name" :props="scope">
              <template v-if="col.name === 'date'">
                <div>{{ formatDayLabel(scope.row.date) }}</div>
                <div v-if="scope.row.day.isHoliday" class="text-caption text-grey-7">{{ scope.row.day.holidayTitle || "Holiday" }}</div>
              </template>
              <template v-else-if="col.name === 'stateLabel'">
                <span class="att-emp__state" :style="{ '--cell-colour': scope.row.state.colour }">
                  <q-icon v-if="scope.row.state.icon" :name="scope.row.state.icon" size="15px" />
                  <b v-else>{{ scope.row.state.glyph }}</b>
                  {{ scope.row.state.label }}
                </span>
                <div v-if="scope.row.cell.leave && scope.row.cell.state !== 'Leave'" class="text-caption text-grey-7">
                  half-day leave ({{ scope.row.cell.leave.kind }})
                </div>
              </template>
              <template v-else>{{ col.value }}</template>
            </q-td>
          </q-tr>
        </template>

        <!-- Totals for the range, under whichever columns are visible. -->
        <template #bottom-row="scope">
          <q-tr class="att-emp__total">
            <q-td v-for="col in scope.cols" :key="col.name" :class="col.align === 'right' ? 'text-right' : 'text-left'">
              <template v-if="col.name === 'date'">Total</template>
              <template v-else-if="col.name === 'clocked'">{{ fmtMinutes(totals.clockedMinutes) }}</template>
              <template v-else-if="col.name === 'breakMinutes'">{{ fmtMinutes(totals.breakMinutes) }}</template>
              <template v-else-if="col.name === 'worked'">{{ fmtMinutes(totals.workedMinutes) }}</template>
              <template v-else-if="col.name === 'lateMinutes'">{{ fmtLate(totals.lateMinutes) }}</template>
            </q-td>
          </q-tr>
        </template>
      </app-data-table>

      <!-- TimeInTimeOut and the Movement Register in one list: every record behind the days above. -->
      <app-data-table
        class="att-emp__table q-mt-md"
        :title="`Time in / time out and Movement Register entries (${entryRows.length})`"
        :rows="entryRows"
        :columns="entryColumns"
        row-key="id"
        page-key="analytics-attendance-employee-entries"
        dense
        client-sort
        :pagination="entryPagination"
        :rows-per-page-options="[0]"
        hide-pagination
        :loading="loading"
        @refresh="load"
      >
        <template #actions>
          <q-btn flat dense no-caps color="primary" icon="o_download" label="CSV" :disable="!entryRows.length" @click="exportEntries">
            <q-tooltip>Download every time-in and every Movement Register entry of this range</q-tooltip>
          </q-btn>
        </template>

        <template #no-data>
          <app-empty-state icon="o_event_busy" title="Nothing recorded" text="No time-in and no Movement Register entry exists in this range." />
        </template>

        <template #body="scope">
          <q-tr :props="scope">
            <q-td v-for="col in scope.cols" :key="col.name" :props="scope">
              <template v-if="col.name === 'type'">
                <span class="att-emp__type" :style="{ '--tone': scope.row.colour }">
                  <q-icon :name="scope.row.icon" size="14px" />
                  {{ scope.row.type }}
                </span>
              </template>
              <template v-else-if="col.name === 'details'">
                <div class="att-emp__details">{{ scope.row.details }}</div>
                <div v-if="scope.row.note" class="text-caption text-grey-7">{{ scope.row.note }}</div>
              </template>
              <template v-else>{{ col.value }}</template>
            </q-td>
          </q-tr>
        </template>
      </app-data-table>
    </template>
  </app-side-panel>
</template>

<script setup>
// One employee's attendance over a range: the register's selected period, or any week picked by its Sunday.
// Shows the facts and totals for that range, one row per day, and (TimeInTimeOut and the Movement Register
// merged into one list) every time-in and every Movement Register entry behind those days. Opened from the
// icon beside a name or from the day panel.
import { ref, computed, watch } from "vue";
import AppSidePanel from "components/common/AppSidePanel.vue";
import AppEmptyState from "components/common/AppEmptyState.vue";
import AppDataTable from "components/common/AppDataTable.vue";
import AppStatTile from "components/common/AppStatTile.vue";
import AppSegmentedControl from "components/common/AppSegmentedControl.vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";
import RegisterWeekPicker from "modules/analytics/components/register/_registerWeekPicker.vue";
import analyticsService from "modules/analytics/analytics.service";
import { exportCsv } from "composables/useCsvExport";
import {
  ATTENDANCE_STATES,
  ATTENDANCE_PRESENT_STATES,
  RECORD_LOOK,
  SESSION_STATUS_TEXT,
  attendanceStateOf,
  movementFacts,
  movementLength,
  fmtLate,
  fmtMinutes,
  fmtWorked,
  fmtClock,
  fmtStamp,
  fmtTime12
} from "modules/analytics/utils/registerStates";
import {
  todayIso,
  sundayOf,
  weekRangeEnding,
  describeRange,
  formatAppDate,
  formatDayLabel
} from "modules/analytics/composables/useDateRangePresets";

const props = defineProps({
  // { row, from, to, focusDate?, mode? } — the register row, the register's period, and where to start.
  payload: { type: Object, required: true }
});

defineEmits(["close", "record"]);

const loading = ref(false);
const loadError = ref(false);
const detail = ref(null);

const mode = ref(props.payload.mode === "week" ? "week" : "period");
const weekEnd = ref(sundayOf(props.payload.focusDate || todayIso()));

const modeOptions = [
  { value: "period", label: "Selected period", icon: "o_date_range", tooltip: "The period chosen on the register" },
  { value: "week", label: "Week", icon: "o_view_week", tooltip: "One week, picked by its Sunday" }
];

const row = computed(() => detail.value?.employee || props.payload.row || {});
const totals = computed(() => row.value.totals || {});
const rules = computed(() => detail.value?.rules || null);

const currentRange = computed(() => (mode.value === "week"
  ? weekRangeEnding(weekEnd.value)
  : { from: props.payload.from, to: props.payload.to }));

const rangeLabel = computed(() => describeRange(currentRange.value.from, currentRange.value.to));

const subtitle = computed(() => [
  row.value.employeeCode,
  row.value.department,
  row.value.shift ? (row.value.shiftAssigned ? row.value.shift : `${row.value.shift} (default)`) : ""
].filter(Boolean).join(" · "));

const pad = (n) => String(n).padStart(2, "0");

// The middle first time-in of the range: when the employee usually arrives, whatever the shift record says.
const usualTimeIn = computed(() => {
  const minutes = [];
  (detail.value?.days || []).forEach((d, i) => {
    const c = row.value.cells?.[i];
    const m = /^(\d{4}-\d{2}-\d{2})T(\d{2}):(\d{2})/.exec(c?.firstIn || "");
    if (!m || !ATTENDANCE_PRESENT_STATES.includes(c.state)) return;
    // A time-in after midnight still belongs to the day before: keep it after that day's evening.
    minutes.push(Number(m[2]) * 60 + Number(m[3]) + (m[1] === d.date ? 0 : 1440));
  });
  if (!minutes.length) return null;
  minutes.sort((a, b) => a - b);
  const middle = minutes[Math.floor((minutes.length - 1) / 2)] % 1440;
  return { text: fmtTime12(`${pad(Math.floor(middle / 60))}:${pad(middle % 60)}`), days: minutes.length };
});

const facts = computed(() => {
  const r = row.value;
  const t = totals.value;
  const rule = (rules.value?.shifts || []).find((s) => s.name === r.shift);
  const shift = r.shift ? (r.shiftAssigned ? r.shift : `${r.shift} (none assigned, default used)`) : "—";
  const usual = usualTimeIn.value;
  return [
    { label: "Team lead", value: r.teamLeadName || "None" },
    { label: "Department", value: r.department || "—" },
    { label: "Shift", value: rule ? `${shift} · ${fmtTime12(rule.start)}–${fmtTime12(rule.end)}, late after ${fmtTime12(rule.lateAfter)}` : shift },
    { label: "Usual time-in", value: usual ? `${usual.text} (middle of ${usual.days} ${usual.days === 1 ? "day" : "days"})` : "—" },
    { label: "Joined", value: r.joiningDate ? formatAppDate(r.joiningDate) : "—" },
    { label: "Productive", value: `${fmtWorked(t.workedMinutes)} of ${fmtWorked(t.requiredMinutes)} h required${t.shortMinutes > 0 ? ` · ${fmtMinutes(t.shortMinutes)} short` : ""}` }
  ];
});

const tiles = computed(() => {
  const t = totals.value;
  const A = ATTENDANCE_STATES;
  const pct = (n) => (t.expectedDays ? `${Math.round((n / t.expectedDays) * 100)}% of expected days` : "");
  return [
    { key: "expected", label: "Expected days", value: t.expectedDays || 0, caption: `${t.present || 0} with a time-in`, icon: "o_event_available", colour: "#647a82", hint: "Working days expected in this range, once holidays and full-day leaves are taken out." },
    { key: "OnTime", label: A.OnTime.label, value: t.onTime || 0, caption: pct(t.onTime || 0), icon: A.OnTime.icon, colour: A.OnTime.colour, hint: A.OnTime.description },
    { key: "Late", label: A.Late.label, value: t.late || 0, caption: t.late ? `${t.lateUndeclared || 0} with no time adjustment` : "", icon: A.Late.icon, colour: A.Late.colour, hint: "Days whose first time-in came after the shift start plus grace, whatever else happened that day." },
    { key: "Short", label: A.Short.label, value: t.short || 0, caption: pct(t.short || 0), icon: A.Short.icon, colour: A.Short.colour, hint: A.Short.description },
    { key: "NoTimeOut", label: A.NoTimeOut.label, value: t.noTimeOut || 0, caption: pct(t.noTimeOut || 0), icon: A.NoTimeOut.icon, colour: A.NoTimeOut.colour, hint: A.NoTimeOut.description },
    { key: "Absent", label: A.Absent.label, value: t.absent || 0, caption: pct(t.absent || 0), icon: A.Absent.icon, colour: A.Absent.colour, hint: A.Absent.description },
    { key: "Wfh", label: "Work From Home", value: t.wfh || 0, caption: t.wfhNoTimeIn ? `days · ${t.wfhNoTimeIn} with no time-in` : "days", icon: "o_home_work", colour: A.Wfh.colour, hint: "Days with a Work From Home entry in the Movement Register." },
    { key: "Leave", label: "On leave", value: t.leave || 0, caption: t.halfDay ? `full days · ${t.halfDay} half days besides` : "full days", icon: "o_beach_access", colour: A.Leave.colour, hint: "Days covered by a full-day leave." },
    { key: "Break", label: "Breaks", value: t.breaks || 0, caption: `entries · ${fmtMinutes(t.breakMinutes)}`, icon: "o_free_breakfast", colour: "#647a82", hint: "Break entries in the Movement Register; their minutes are taken out of the productive hours." },
    { key: "Adjust", label: "Time adjustments", value: t.adjustments || 0, caption: `entries · ${fmtMinutes(t.adjustMinutes)}`, icon: "o_more_time", colour: "#647a82", hint: "Time Adjustment entries in the Movement Register; their minutes are taken out of the productive hours." }
  ];
});

// ---- days table ----------------------------------------------------------------------------------------
function lastOutText (cell, date) {
  if (!cell.firstIn) return "";
  if (cell.lastOut) return fmtClock(cell.lastOut, date);
  return cell.state === "InNow" ? "Still in" : "Missing";
}

// One row per day the employee was employed, flat so every column sorts by a plain value.
const dayRows = computed(() => {
  const list = [];
  (detail.value?.days || []).forEach((d, i) => {
    const c = row.value.cells?.[i];
    if (!c || c.state === "NotEmployed") return;
    const state = attendanceStateOf(c);
    list.push({
      id: d.date,
      date: d.date,
      day: d,
      cell: c,
      state,
      stateLabel: state.label,
      firstIn: c.firstIn || "",
      lastOut: lastOutText(c, d.date),
      clocked: c.clocked || 0,
      breakMinutes: c.breakMinutes || 0,
      worked: c.worked || 0,
      lateMinutes: c.lateMinutes || 0,
      movement: movementFacts(c).join(" · ")
    });
  });
  return list;
});

const minutesOrBlank = (value) => (value > 0 ? fmtMinutes(value) : "");

const dayColumns = [
  { name: "date", label: "Date", field: "date", align: "left", sortable: true },
  { name: "stateLabel", label: "Day", field: "stateLabel", align: "left", sortable: true },
  { name: "firstIn", label: "Time in", field: "firstIn", align: "left", sortable: true, format: (v, r) => fmtClock(v, r.date) },
  { name: "lastOut", label: "Time out", field: "lastOut", align: "left", sortable: true },
  { name: "clocked", label: "Clocked", field: "clocked", align: "right", sortable: true, format: minutesOrBlank },
  { name: "breakMinutes", label: "Breaks", field: "breakMinutes", align: "right", sortable: true, format: minutesOrBlank },
  { name: "worked", label: "Productive", field: "worked", align: "right", sortable: true, format: minutesOrBlank },
  { name: "lateMinutes", label: "Late by", field: "lateMinutes", align: "right", sortable: true, format: (v) => fmtLate(v) },
  { name: "movement", label: "Movement Register", field: "movement", align: "left", sortable: true }
];

// All days on one page, newest first. rowsNumber is cleared on purpose: with it set, QTable treats the
// table as server-paged and never sorts locally.
const dayPagination = { rowsPerPage: 0, sortBy: "date", descending: true, rowsNumber: undefined };

// ---- entries table -------------------------------------------------------------------------------------
function enteredNote (m) {
  if (m.enteredDaysLater > 0) return `Entered ${m.enteredDaysLater} ${m.enteredDaysLater === 1 ? "day" : "days"} after the date`;
  if (m.enteredDaysLater < 0) return `Entered ${-m.enteredDaysLater} ${m.enteredDaysLater === -1 ? "day" : "days"} ahead of the date`;
  return "";
}

// Time-ins and Movement Register entries together: newest day first, and within a day in the order they happened.
const entryRows = computed(() => {
  const list = [];

  (detail.value?.sessions || []).forEach((s, index) => {
    const out = s.out ? fmtClock(s.out, s.date) : (s.status === "Live" ? "still in" : "no time-out");
    list.push({
      id: `s${index}`,
      date: s.date,
      at: s.in,
      kind: "Session",
      type: "Time in / out",
      ...RECORD_LOOK.Session,
      time: `${fmtClock(s.in, s.date)} – ${out}`,
      minutes: s.minutes || 0,
      length: s.minutes > 0 ? fmtMinutes(s.minutes) : "",
      details: SESSION_STATUS_TEXT[s.status] || "",
      note: "",
      approver: "",
      enteredAt: ""
    });
  });

  (detail.value?.movements || []).forEach((m, index) => {
    const look = RECORD_LOOK[m.kind] || RECORD_LOOK.Other;
    list.push({
      id: `m${index}`,
      date: m.date,
      at: m.enteredAt || "",
      kind: m.kind,
      type: m.type || "Movement Register",
      ...look,
      time: "",
      minutes: m.minutes || 0,
      length: movementLength(m),
      details: m.message || "",
      note: enteredNote(m),
      approver: m.approver || "",
      enteredAt: m.enteredAt || ""
    });
  });

  return list.sort((a, b) => String(b.date).localeCompare(String(a.date)) || String(a.at).localeCompare(String(b.at)));
});

const entryColumns = [
  { name: "date", label: "Date", field: "date", align: "left", sortable: true, format: (v) => formatDayLabel(v) },
  { name: "type", label: "Record", field: "type", align: "left", sortable: true },
  { name: "time", label: "Time in – out", field: "time", align: "left", sortable: true },
  { name: "length", label: "Length", field: "minutes", align: "right", sortable: true, format: (v, r) => r.length },
  { name: "details", label: "Details", field: "details", align: "left", sortable: true },
  { name: "approver", label: "Permission by", field: "approver", align: "left", sortable: true },
  { name: "enteredAt", label: "Entered at", field: "enteredAt", align: "left", sortable: true, format: (v) => fmtStamp(v) }
];

const entryPagination = { rowsPerPage: 0, sortBy: "date", descending: true, rowsNumber: undefined };

// ---- export --------------------------------------------------------------------------------------------
function fileStem () {
  const r = row.value;
  const { from, to } = currentRange.value;
  const who = String(r.employeeCode || r.fullName || "employee").replace(/\s+/g, "_");
  return `${who}_${from}_${to}`;
}

function exportDays () {
  exportCsv({
    filename: `attendance-days_${fileStem()}.csv`,
    columns: [
      { label: "Date", value: (d) => formatAppDate(d.date) },
      { label: "Day", value: (d) => d.stateLabel },
      { label: "Time in", value: (d) => fmtClock(d.firstIn, d.date) },
      { label: "Time out", value: (d) => d.lastOut },
      { label: "Time-ins", value: (d) => d.cell.sessions || 0 },
      { label: "Clocked minutes", value: (d) => d.clocked },
      { label: "Break minutes", value: (d) => d.breakMinutes },
      { label: "Productive minutes", value: (d) => d.worked },
      { label: "Expected minutes", value: (d) => d.cell.expected || 0 },
      { label: "Late minutes", value: (d) => d.lateMinutes },
      { label: "Breaks", value: (d) => d.cell.breaks || 0 },
      { label: "Time adjustments", value: (d) => d.cell.adjustments || 0 },
      { label: "Time adjustment minutes", value: (d) => d.cell.adjustMinutes || 0 },
      { label: "Work From Home", value: (d) => d.cell.wfh || "" },
      { label: "Leave", value: (d) => (d.cell.leave ? `${d.cell.leave.kind} (${d.cell.leave.status || "applied"})` : "") },
      { label: "Holiday", value: (d) => (d.day.isHoliday ? (d.day.holidayTitle || "Holiday") : "") }
    ],
    rows: [...dayRows.value].sort((a, b) => a.date.localeCompare(b.date))
  });
}

function exportEntries () {
  exportCsv({
    filename: `attendance-entries_${fileStem()}.csv`,
    columns: [
      { label: "Date", value: (e) => formatAppDate(e.date) },
      { label: "Record", value: (e) => e.type },
      { label: "Time in - out", value: (e) => e.time.replace("–", "-") },
      { label: "Minutes", value: (e) => e.minutes },
      { label: "Length", value: (e) => e.length },
      { label: "Details", value: (e) => e.details },
      { label: "Permission by", value: (e) => e.approver },
      { label: "Entered at", value: (e) => fmtStamp(e.enteredAt) },
      { label: "Note", value: (e) => e.note }
    ],
    rows: [...entryRows.value].sort((a, b) => String(a.date).localeCompare(String(b.date)) || String(a.at).localeCompare(String(b.at)))
  });
}

// ---- loading -------------------------------------------------------------------------------------------
async function load () {
  const employeeId = props.payload.row?.employeeId;
  const { from, to } = currentRange.value;
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

watch([mode, weekEnd], load);
watch(() => props.payload, (p) => {
  mode.value = p?.mode === "week" ? "week" : "period";
  weekEnd.value = sundayOf(p?.focusDate || todayIso());
  load();
}, { immediate: true });
</script>

<style scoped>
.att-emp__bar {
  display: flex;
  align-items: flex-end;
  flex-wrap: wrap;
  gap: 8px 14px;
  margin-bottom: 12px;
}
.att-emp__field {
  display: flex;
  flex-direction: column;
}
.att-emp__field :deep(.app-segmented) {
  height: 40px;
}
.att-emp__field :deep(.app-segmented__item) {
  min-height: 38px;
}
.att-emp__range {
  padding-bottom: 10px;
}
.att-emp__facts {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 10px 16px;
  margin-bottom: 12px;
}
.att-emp__tiles {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.att-emp__tiles > * {
  flex: 1 1 140px;
}
/* Tables: header in the theme blue with white text, like the register grid. */
.att-emp__table :deep(thead tr th) {
  background: var(--q-primary);
  color: #fff;
  font-weight: 600;
}
.att-emp__table :deep(thead tr th .q-table__sort-icon),
.att-emp__table :deep(thead tr th.sortable:hover) {
  color: #fff;
}
.att-emp__table :deep(td) {
  vertical-align: top;
}
.att-emp__total td {
  font-weight: 600;
  background: #f7fafb;
  border-top: 2px solid var(--line, #dde6e8);
}
.att-emp__state {
  --cell-colour: #647a82;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 1px 8px 1px 6px;
  border-radius: 10px;
  background: color-mix(in srgb, var(--cell-colour) 12%, #fff);
  white-space: nowrap;
}
.att-emp__state .q-icon,
.att-emp__state b {
  color: var(--cell-colour);
}
.att-emp__type {
  --tone: #647a82;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  white-space: nowrap;
}
.att-emp__type .q-icon {
  color: var(--tone);
}
.att-emp__details {
  white-space: normal;
  overflow-wrap: anywhere;
  min-width: 180px;
}
</style>
