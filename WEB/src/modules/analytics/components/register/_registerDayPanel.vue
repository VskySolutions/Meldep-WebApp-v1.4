<template>
  <app-side-panel
    :title="title"
    :subtitle="subtitle"
    width="75vw"
    min-width="720px"
    hide-save
    cancel-label="Close"
    :loading="loading && !detail"
    @close="$emit('close')"
  >
    <template v-if="detail">
      <div class="tsr-day__banner" :style="{ '--cell-colour': state.colour }">
        <q-icon v-if="state.icon" :name="state.icon" size="24px" class="tsr-day__icon" />
        <b v-else class="tsr-day__glyph">{{ state.glyph }}</b>
        <div class="tsr-day__banner-text">
          <div class="tsr-day__state">
            {{ state.label }}
            <span v-if="half" class="text-weight-regular"> · half-day leave ({{ cell.leave.kind }}, {{ cell.leave.status || "applied" }})</span>
          </div>
          <div class="text-body2">{{ explanation }}</div>
        </div>
      </div>

      <div v-if="loadError" class="text-caption text-negative q-mt-sm">
        The lines could not be loaded; the day is shown from the register itself.
      </div>

      <div class="section-subhead">The day</div>
      <div class="tsr-day__facts">
        <div v-for="f in facts" :key="f.label" class="tsr-day__fact">
          <div class="record-label">{{ f.label }}</div>
          <div class="record-value">{{ f.value }}</div>
        </div>
      </div>

      <app-data-table
        class="tsr-day__lines q-mt-md"
        :title="`Timesheet lines (${lineRows.length})`"
        :rows="lineRows"
        :columns="lineColumns"
        row-key="id"
        page-key="analytics-register-day-lines"
        dense
        client-sort
        :pagination="linePagination"
        :rows-per-page-options="[0]"
        hide-pagination
        :loading="loading"
        @refresh="load"
      >
        <template #no-data>
          <app-empty-state icon="o_pending_actions" title="No lines" text="No timesheet line exists for this date." />
        </template>

        <!-- Two-line cells (project over module, task over activity) and the description under the row. -->
        <template #body="scope">
          <q-tr :props="scope">
            <q-td v-for="col in scope.cols" :key="col.name" :props="scope">
              <template v-if="col.name === 'project'">
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
          <q-tr v-if="scope.row.description" :props="scope" class="tsr-day__desc">
            <q-td :colspan="scope.cols.length" class="text-grey-8">{{ scope.row.description }}</q-td>
          </q-tr>
        </template>

        <!-- Totals for the day, under whichever columns are visible. -->
        <template #bottom-row="scope">
          <q-tr class="tsr-day__total">
            <q-td v-for="col in scope.cols" :key="col.name" :class="col.align === 'right' ? 'text-right' : 'text-left'">
              <template v-if="col.name === 'project'">Total</template>
              <template v-else-if="col.name === 'hours'">{{ fmtHours(lineTotals.hours) }}</template>
              <template v-else-if="col.name === 'billableHours'">{{ fmtHours(lineTotals.billable) }}</template>
            </q-td>
          </q-tr>
        </template>
      </app-data-table>
    </template>

    <!-- The same employee over the week of this day, or over the register's whole period. -->
    <template #footer-actions>
      <q-btn flat no-caps color="primary" icon="o_view_week" label="View week" @click="$emit('timesheet', { mode: 'week' })">
        <q-tooltip>This employee's timesheet for the week of this day</q-tooltip>
      </q-btn>
      <q-btn flat no-caps color="primary" icon="o_receipt_long" label="View period" @click="$emit('timesheet', { mode: 'period' })">
        <q-tooltip>This employee's timesheet for the selected period</q-tooltip>
      </q-btn>
    </template>
  </app-side-panel>
</template>

<script setup>
// One cell of the register opened up: the state in words, the facts the rule used (expected, logged, shift,
// cutoff, completed at) and the timesheet lines behind it in the standard data table. Falls back to the cell
// already on screen when the detail call fails, so the panel is never empty.
import { ref, computed, watch } from "vue";
import AppSidePanel from "components/common/AppSidePanel.vue";
import AppEmptyState from "components/common/AppEmptyState.vue";
import AppDataTable from "components/common/AppDataTable.vue";
import analyticsService from "modules/analytics/analytics.service";
import { stateOf, fmtHours, fmtLate, isHalfDay, explainCell, shiftOf } from "modules/analytics/utils/registerStates";
import { formatAppDate, formatDayLabel, formatLocalMinute } from "modules/analytics/composables/useDateRangePresets";

const props = defineProps({
  // { row, day, cell } from the grid
  payload: { type: Object, required: true }
});

defineEmits(["close", "timesheet"]);

const loading = ref(false);
const loadError = ref(false);
const detail = ref(null);

const row = computed(() => detail.value?.employee || props.payload.row);
const day = computed(() => detail.value?.day || props.payload.day);
const cell = computed(() => detail.value?.cell || props.payload.cell);
const state = computed(() => stateOf(cell.value));
const half = computed(() => isHalfDay(cell.value));
const explanation = computed(() => explainCell(cell.value, day.value));

const title = computed(() => row.value?.fullName || "Employee");
const subtitle = computed(() => `${formatDayLabel(day.value?.date)}${row.value?.employeeCode ? " · " + row.value.employeeCode : ""}`);

const facts = computed(() => {
  const c = cell.value || {};
  const r = row.value || {};
  const d = day.value || {};
  const list = [];
  const working = ["OnTime", "Delayed", "Partial", "Pending", "Due"].includes(c.state);

  if (working) {
    list.push({ label: "Expected", value: `${fmtHours(c.expected)} h` });
  }
  list.push({ label: "Logged", value: `${fmtHours(c.logged)} h in ${c.lines || 0} ${c.lines === 1 ? "line" : "lines"}` });
  if (working) {
    const { shift, assigned } = shiftOf(c, r);
    list.push({ label: "Shift", value: assigned ? shift : `${shift} (none assigned, default used)` });
  }
  if (c.cutoff && working) {
    list.push({ label: "Cutoff", value: formatLocalMinute(c.cutoff) });
  }
  if (c.firstEntryAt) {
    list.push({ label: "First entry", value: formatLocalMinute(c.firstEntryAt) });
  }
  if (c.completedAt) {
    list.push({ label: `Reached ${fmtHours(c.expected)} h at`, value: formatLocalMinute(c.completedAt) + (c.preFilled ? " (before the day)" : "") });
  }
  if (c.state === "Delayed") {
    list.push({ label: "Late by", value: fmtLate(c.lateMinutes) || "under a minute" });
  }
  if (c.leave) {
    list.push({ label: "Leave", value: `${c.leave.kind}${c.leave.status ? " · " + c.leave.status : ""}${c.leave.category ? " · " + c.leave.category : ""}` });
  }
  if (d.isHoliday) {
    list.push({ label: "Holiday", value: d.holidayTitle || "Holiday" });
  }
  list.push({ label: "Week status", value: c.timesheetStatus || "Not submitted" });
  if (r.department) {
    list.push({ label: "Department", value: r.department });
  }
  if (r.teamLeadName) {
    list.push({ label: "Team lead", value: r.teamLeadName });
  }
  if (c.state === "NotEmployed") {
    list.push({ label: "Employment", value: `${r.joiningDate ? "from " + formatAppDate(r.joiningDate) : ""}${r.releaseDate ? " to " + formatAppDate(r.releaseDate) : ""}`.trim() || "—" });
  }
  return list;
});

// The lines as table rows (a key per row) and the standard column contract.
const lineRows = computed(() => (detail.value?.lines || []).map((line, index) => ({ id: index + 1, ...line })));

const lineColumns = [
  { name: "project", label: "Project / Module", field: "project", align: "left", sortable: true },
  { name: "task", label: "Task / Activity", field: "task", align: "left", sortable: true },
  { name: "hours", label: "Hours", field: "hours", align: "right", sortable: true, format: (v) => fmtHours(v) },
  { name: "billableHours", label: "Billable Hours", field: "billableHours", align: "right", sortable: true, format: (v) => fmtHours(v) },
  { name: "enteredAt", label: "Entered at", field: "enteredAt", align: "left", sortable: true }
];

// All lines on one page, oldest entry first. rowsNumber is cleared on purpose: with it set, QTable treats
// the table as server-paged and never sorts locally.
const linePagination = { rowsPerPage: 0, sortBy: "enteredAt", descending: false, rowsNumber: undefined };

// Footer of the lines table: the day's hours and billable hours added up.
const lineTotals = computed(() => (detail.value?.lines || []).reduce(
  (sum, line) => ({ hours: sum.hours + Number(line.hours || 0), billable: sum.billable + Number(line.billableHours || 0) }),
  { hours: 0, billable: 0 }
));

async function load () {
  const { row: r, day: d } = props.payload;
  if (!r?.employeeId || !d?.date) return;
  loading.value = true;
  loadError.value = false;
  try {
    detail.value = await analyticsService.getTimesheetRegisterDay(r.employeeId, d.date);
  } catch {
    loadError.value = true;
    detail.value = { employee: r, day: d, cell: props.payload.cell, lines: [] };
  } finally {
    loading.value = false;
  }
}

watch(() => props.payload, load, { immediate: true });
</script>

<style scoped>
.tsr-day__banner {
  --cell-colour: #b0b4b8;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 12px 14px;
  border-radius: 10px;
  border: 1px solid color-mix(in srgb, var(--cell-colour) 40%, #fff);
  background: color-mix(in srgb, var(--cell-colour) 10%, #fff);
}
.tsr-day__icon,
.tsr-day__glyph {
  color: var(--cell-colour);
  flex: 0 0 auto;
}
.tsr-day__glyph {
  font-size: 20px;
  line-height: 24px;
  width: 24px;
  text-align: center;
}
.tsr-day__banner-text {
  min-width: 0;
}
.tsr-day__state {
  font-size: 15px;
  font-weight: 600;
  color: var(--ink-900, #16262c);
}
.tsr-day__facts {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 10px 16px;
}
/* Header in the theme blue with white text, like the register grid and the other Meldep tables. */
.tsr-day__lines :deep(thead tr th) {
  background: var(--q-primary);
  color: #fff;
  font-weight: 600;
}
.tsr-day__lines :deep(thead tr th .q-table__sort-icon) {
  color: #fff;
}
.tsr-day__lines :deep(thead tr th.sortable:hover) {
  color: #fff;
}
.tsr-day__lines :deep(td) {
  vertical-align: top;
}
.tsr-day__total td {
  font-weight: 600;
  background: #f7fafb;
  border-top: 2px solid var(--line, #dde6e8);
}
.tsr-day__desc td {
  font-size: 12px;
  white-space: normal;
  border-top: none !important;
  padding-top: 0 !important;
}
</style>
