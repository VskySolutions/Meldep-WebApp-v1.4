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
    <div class="tsr-emp-panel__bar">
      <div class="tsr-emp-panel__field">
        <app-field-label label="Show" />
        <app-segmented-control :model-value="mode" :options="modeOptions" aria-label="Range" @update:model-value="mode = $event" />
      </div>
      <register-week-picker v-if="mode === 'week'" v-model="weekEnd" />
      <div class="tsr-emp-panel__range text-caption text-grey-7">{{ rangeLabel }}</div>
      <q-space />
      <q-btn outline no-caps color="primary" icon="o_download" label="Export CSV" :disable="!lineRows.length" @click="exportLines">
        <q-tooltip>Download every line of this range for this employee</q-tooltip>
      </q-btn>
    </div>

    <div v-if="loadError" class="text-caption text-negative q-mt-sm">The timesheet could not be loaded.</div>

    <template v-if="detail">
      <div class="tsr-emp-panel__facts">
        <div v-for="f in facts" :key="f.label">
          <div class="record-label">{{ f.label }}</div>
          <div class="record-value">{{ f.value }}</div>
        </div>
      </div>

      <div class="tsr-emp-panel__tiles">
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

      <app-data-table
        class="tsr-emp-panel__lines q-mt-md"
        :title="`Timesheet lines (${lineRows.length})`"
        :rows="lineRows"
        :columns="lineColumns"
        row-key="id"
        page-key="analytics-register-employee-lines"
        dense
        client-sort
        :pagination="linePagination"
        :rows-per-page-options="[0]"
        hide-pagination
        :loading="loading"
        @refresh="load"
      >
        <template #no-data>
          <app-empty-state icon="o_pending_actions" title="No lines" text="No timesheet line exists in this range." />
        </template>

        <template #body="scope">
          <q-tr :props="scope">
            <q-td v-for="col in scope.cols" :key="col.name" :props="scope">
              <template v-if="col.name === 'date'">
                <div>{{ formatDayLabel(scope.row.date) }}</div>
                <div v-if="scope.row.timesheetStatus" class="text-caption text-grey-7">{{ scope.row.timesheetStatus }}</div>
              </template>
              <template v-else-if="col.name === 'project'">
                <div>{{ scope.row.project || "—" }}</div>
                <div v-if="scope.row.projectModule" class="text-caption text-grey-7">{{ scope.row.projectModule }}</div>
              </template>
              <template v-else-if="col.name === 'task'">
                <div>{{ scope.row.task || "—" }}</div>
                <div v-if="scope.row.activity" class="text-caption text-grey-7">{{ scope.row.activity }}</div>
              </template>
              <template v-else-if="col.name === 'enteredAt'">
                {{ formatLocalMinute(scope.row.enteredAt) }}
                <q-icon v-if="scope.row.isApproved" name="o_verified" color="positive" size="16px" class="q-ml-xs">
                  <q-tooltip>Approved</q-tooltip>
                </q-icon>
              </template>
              <template v-else>{{ col.value }}</template>
            </q-td>
          </q-tr>
          <q-tr v-if="scope.row.description" :props="scope" class="tsr-emp-panel__desc">
            <q-td :colspan="scope.cols.length" class="text-grey-8">{{ scope.row.description }}</q-td>
          </q-tr>
        </template>

        <template #bottom-row="scope">
          <q-tr class="tsr-emp-panel__total">
            <q-td v-for="col in scope.cols" :key="col.name" :class="col.align === 'right' ? 'text-right' : 'text-left'">
              <template v-if="col.name === 'date'">Total</template>
              <template v-else-if="col.name === 'hours'">{{ fmtHours(lineTotals.hours) }}</template>
              <template v-else-if="col.name === 'billableHours'">{{ fmtHours(lineTotals.billable) }}</template>
            </q-td>
          </q-tr>
        </template>
      </app-data-table>
    </template>
  </app-side-panel>
</template>

<script setup>
// One employee's timesheet over a range: the register's selected period, or any week picked by its Sunday.
// Shows the facts and totals for that range and every timesheet line, newest day first, with a totals footer. Opened from the receipt icon beside a name or from the day panel.
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
import { STATES, fmtHours } from "modules/analytics/utils/registerStates";
import {
  todayIso,
  sundayOf,
  weekRangeEnding,
  describeRange,
  formatAppDate,
  formatDayLabel,
  formatLocalMinute
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

const currentRange = computed(() => (mode.value === "week"
  ? weekRangeEnding(weekEnd.value)
  : { from: props.payload.from, to: props.payload.to }));

const rangeLabel = computed(() => describeRange(currentRange.value.from, currentRange.value.to));

const subtitle = computed(() => [
  row.value.employeeCode,
  row.value.department,
  row.value.shift ? (row.value.shiftAssigned ? row.value.shift : `${row.value.shift} (default)`) : ""
].filter(Boolean).join(" · "));

const facts = computed(() => {
  const r = row.value;
  const t = r.totals || {};
  return [
    { label: "Team lead", value: r.teamLeadName || "None" },
    { label: "Department", value: r.department || "—" },
    { label: "Shift", value: r.shift ? (r.shiftAssigned ? r.shift : `${r.shift} (none assigned, default used)`) : "—" },
    { label: "Joined", value: r.joiningDate ? formatAppDate(r.joiningDate) : "—" },
    { label: "Logged", value: `${fmtHours(t.loggedHours)} of ${fmtHours(t.expectedHours)} h expected` }
  ];
});

const tiles = computed(() => {
  const t = row.value.totals || {};
  const pct = (n) => (t.expectedDays ? `${Math.round((n / t.expectedDays) * 100)}% of expected days` : "");
  return [
    { key: "expected", label: "Expected days", value: t.expectedDays || 0, caption: `${fmtHours(t.loggedHours)} h logged`, icon: "o_event_available", colour: "#647a82", hint: "Working days expected in this range, once holidays and full-day leaves are taken out." },
    { key: "OnTime", label: STATES.OnTime.label, value: t.onTime || 0, caption: pct(t.onTime || 0), icon: STATES.OnTime.icon, colour: STATES.OnTime.colour, hint: STATES.OnTime.description },
    { key: "Delayed", label: STATES.Delayed.label, value: t.delayed || 0, caption: pct(t.delayed || 0), icon: STATES.Delayed.icon, colour: STATES.Delayed.colour, hint: STATES.Delayed.description },
    { key: "Partial", label: STATES.Partial.label, value: t.partial || 0, caption: pct(t.partial || 0), icon: STATES.Partial.icon, colour: STATES.Partial.colour, hint: STATES.Partial.description },
    { key: "Pending", label: STATES.Pending.label, value: t.pending || 0, caption: pct(t.pending || 0), icon: STATES.Pending.icon, colour: STATES.Pending.colour, hint: STATES.Pending.description },
    { key: "Due", label: STATES.Due.label, value: t.due || 0, caption: "cutoff not passed yet", icon: STATES.Due.icon, colour: STATES.Due.colour, hint: STATES.Due.description },
    { key: "Leave", label: "On leave", value: t.leave || 0, caption: t.halfDay ? `full days · ${t.halfDay} half days besides` : "full days", icon: "o_beach_access", colour: STATES.Leave.colour, hint: "Days covered by a full-day leave." }
  ];
});

// ---- lines table ---------------------------------------------------------------------------------------
// Newest day first, and within a day the latest entry first; header clicks re-sort from here.
const lineRows = computed(() => (detail.value?.lines || [])
  .map((line, index) => ({ id: index + 1, ...line }))
  .sort((a, b) => String(b.date || "").localeCompare(String(a.date || ""))
    || String(b.enteredAt || "").localeCompare(String(a.enteredAt || ""))));

const lineColumns = [
  { name: "date", label: "Date", field: "date", align: "left", sortable: true },
  { name: "project", label: "Project / Module", field: "project", align: "left", sortable: true },
  { name: "task", label: "Task / Activity", field: "task", align: "left", sortable: true },
  { name: "hours", label: "Hours", field: "hours", align: "right", sortable: true, format: (v) => fmtHours(v) },
  { name: "billableHours", label: "Billable Hours", field: "billableHours", align: "right", sortable: true, format: (v) => fmtHours(v) },
  { name: "enteredAt", label: "Entered at", field: "enteredAt", align: "left", sortable: true }
];

// All lines on one page, newest day first. rowsNumber is cleared on purpose: with it set, QTable treats
// the table as server-paged and never sorts locally.
const linePagination = { rowsPerPage: 0, sortBy: "date", descending: true, rowsNumber: undefined };

const lineTotals = computed(() => (detail.value?.lines || []).reduce(
  (sum, line) => ({ hours: sum.hours + Number(line.hours || 0), billable: sum.billable + Number(line.billableHours || 0) }),
  { hours: 0, billable: 0 }
));

function exportLines () {
  const r = row.value;
  const { from, to } = currentRange.value;
  const who = String(r.employeeCode || r.fullName || "employee").replace(/\s+/g, "_");
  exportCsv({
    filename: `timesheet_${who}_${from}_${to}.csv`,
    columns: [
      { label: "Date", value: (l) => formatAppDate(l.date) },
      { label: "Project", value: (l) => l.project || "" },
      { label: "Module", value: (l) => l.projectModule || "" },
      { label: "Task", value: (l) => l.task || "" },
      { label: "Activity", value: (l) => l.activity || "" },
      { label: "Hours", value: (l) => fmtHours(l.hours) },
      { label: "Billable hours", value: (l) => fmtHours(l.billableHours) },
      { label: "Entered at", value: (l) => formatLocalMinute(l.enteredAt) },
      { label: "Approved", value: (l) => (l.isApproved ? "Yes" : "No") },
      { label: "Week status", value: (l) => l.timesheetStatus || "" },
      { label: "Description", value: (l) => l.description || "" }
    ],
    rows: detail.value?.lines || []
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
    detail.value = await analyticsService.getTimesheetRegisterEmployee(employeeId, from, to);
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
.tsr-emp-panel__bar {
  display: flex;
  align-items: flex-end;
  flex-wrap: wrap;
  gap: 8px 14px;
  margin-bottom: 12px;
}
.tsr-emp-panel__field {
  display: flex;
  flex-direction: column;
}
.tsr-emp-panel__field :deep(.app-segmented) {
  height: 40px;
}
.tsr-emp-panel__field :deep(.app-segmented__item) {
  min-height: 38px;
}
.tsr-emp-panel__range {
  padding-bottom: 10px;
}
.tsr-emp-panel__facts {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 10px 16px;
  margin-bottom: 12px;
}
.tsr-emp-panel__tiles {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.tsr-emp-panel__tiles > * {
  flex: 1 1 140px;
}
/* Lines table: header in the theme blue with white text, like the register grid. */
.tsr-emp-panel__lines :deep(thead tr th) {
  background: var(--q-primary);
  color: #fff;
  font-weight: 600;
}
.tsr-emp-panel__lines :deep(thead tr th .q-table__sort-icon),
.tsr-emp-panel__lines :deep(thead tr th.sortable:hover) {
  color: #fff;
}
.tsr-emp-panel__lines :deep(td) {
  vertical-align: top;
}
.tsr-emp-panel__total td {
  font-weight: 600;
  background: #f7fafb;
  border-top: 2px solid var(--line, #dde6e8);
}
.tsr-emp-panel__desc td {
  font-size: 12px;
  white-space: normal;
  border-top: none !important;
  padding-top: 0 !important;
}
</style>
