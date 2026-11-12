<template>
  <app-side-panel
    :title="title"
    :subtitle="subtitle"
    width="62vw"
    min-width="720px"
    hide-save
    cancel-label="Close"
    :loading="loading && !detail"
    @close="$emit('close')"
  >
    <template v-if="detail">
      <div class="att-day__banner" :style="{ '--cell-colour': state.colour }">
        <q-icon v-if="state.icon" :name="state.icon" size="24px" class="att-day__icon" />
        <b v-else class="att-day__glyph">{{ state.glyph }}</b>
        <div class="att-day__banner-text">
          <div class="att-day__state">
            {{ state.label }}
            <span v-if="half" class="text-weight-regular"> · half-day leave ({{ cell.leave.kind }}, {{ cell.leave.status || "applied" }})</span>
          </div>
          <div class="text-body2">{{ explanation }}</div>
        </div>
      </div>

      <div v-if="loadError" class="text-caption text-negative q-mt-sm">
        The records could not be loaded; the day is shown from the register itself.
      </div>

      <div class="section-subhead">The day</div>
      <div class="att-day__facts">
        <div v-for="f in facts" :key="f.label" class="att-day__fact">
          <div class="record-label">{{ f.label }}</div>
          <div class="record-value">{{ f.value }}</div>
        </div>
      </div>

      <!-- TimeInTimeOut and the Movement Register in one list, in the order things happened. -->
      <div class="section-subhead">
        Time in, time out and Movement Register
        <span class="text-weight-regular text-grey-7">({{ timeline.length }})</span>
      </div>
      <app-empty-state
        v-if="!timeline.length"
        icon="o_event_busy"
        title="Nothing recorded"
        text="No time-in and no Movement Register entry exists for this date."
      />
      <ol v-else class="att-line">
        <li v-for="item in timeline" :key="item.key" class="att-line__item" :style="{ '--tone': item.colour }">
          <div class="att-line__time">
            <div>{{ item.time }}</div>
            <div v-if="item.timeNote" class="att-line__time-note">{{ item.timeNote }}</div>
          </div>
          <div class="att-line__mark"><q-icon :name="item.icon" size="15px" /></div>
          <div class="att-line__body">
            <div class="att-line__title">
              {{ item.title }}
              <span v-if="item.badge" class="att-line__badge">{{ item.badge }}</span>
            </div>
            <div v-if="item.text" class="att-line__text">{{ item.text }}</div>
            <div v-if="item.meta" class="att-line__meta">{{ item.meta }}</div>
          </div>
        </li>
      </ol>
      <div v-if="hasMovement" class="text-caption text-grey-7 q-mt-sm">
        A Movement Register entry has no clock time of its own, so it sits at the time it was entered. Breaks
        take their minutes out of the worked hours; time adjustments and work from home are shown as declared.
      </div>
    </template>

    <!-- The same employee over the week of this day, or over the register's whole period. -->
    <template #footer-actions>
      <q-btn flat no-caps color="primary" icon="o_view_week" label="View week" @click="$emit('attendance', { mode: 'week' })">
        <q-tooltip>This employee's time-ins and Movement Register entries for the week of this day</q-tooltip>
      </q-btn>
      <q-btn flat no-caps color="primary" icon="o_fact_check" label="View period" @click="$emit('attendance', { mode: 'period' })">
        <q-tooltip>This employee's time-ins and Movement Register entries for the selected period</q-tooltip>
      </q-btn>
    </template>
  </app-side-panel>
</template>

<script setup>
// One cell of the Attendance Register opened up: the verdict in words, the facts the rule used (first in,
// last out, clocked, breaks, worked, shift, how late) and, merged into one timeline, the TimeInTimeOut rows
// and the Movement Register entries behind it. Falls back to the cell already on screen when the detail call
// fails, so the panel is never empty.
import { ref, computed, watch } from "vue";
import AppSidePanel from "components/common/AppSidePanel.vue";
import AppEmptyState from "components/common/AppEmptyState.vue";
import analyticsService from "modules/analytics/analytics.service";
import {
  ATTENDANCE_EXPECTED_STATES,
  attendanceStateOf,
  explainAttendance,
  isHalfDay,
  shiftOf,
  movementLength,
  RECORD_LOOK,
  fmtLate,
  fmtMinutes,
  fmtClock
} from "modules/analytics/utils/registerStates";
import { formatAppDate, formatDayLabel } from "modules/analytics/composables/useDateRangePresets";

const props = defineProps({
  // { row, day, cell } from the grid
  payload: { type: Object, required: true }
});

defineEmits(["close", "attendance"]);

const loading = ref(false);
const loadError = ref(false);
const detail = ref(null);

const row = computed(() => detail.value?.employee || props.payload.row);
const day = computed(() => detail.value?.day || props.payload.day);
const cell = computed(() => detail.value?.cell || props.payload.cell);
const rules = computed(() => detail.value?.rules || null);
const state = computed(() => attendanceStateOf(cell.value));
const half = computed(() => isHalfDay(cell.value));
const explanation = computed(() => explainAttendance(cell.value, day.value));

const title = computed(() => row.value?.fullName || "Employee");
const subtitle = computed(() => `${formatDayLabel(day.value?.date)}${row.value?.employeeCode ? " · " + row.value.employeeCode : ""}`);

const plural = (n, one, many) => `${n} ${n === 1 ? one : many}`;

const facts = computed(() => {
  const c = cell.value || {};
  const r = row.value || {};
  const d = day.value || {};
  const list = [];
  const judged = ATTENDANCE_EXPECTED_STATES.includes(c.state);

  if (judged) {
    list.push({ label: "Expected", value: fmtMinutes(c.expected) });
  }
  if (c.firstIn) {
    list.push({ label: "First time-in", value: `${fmtClock(c.firstIn, d.date)}${c.sessions > 1 ? ` (${c.sessions} time-ins)` : ""}` });
    list.push({ label: "Last time-out", value: c.lastOut ? fmtClock(c.lastOut, d.date) : (c.state === "InNow" ? "Still in" : "Missing") });
  }
  if (c.clocked > 0) {
    list.push({ label: "Clocked", value: fmtMinutes(c.clocked) });
  }
  if (c.breaks > 0) {
    list.push({ label: "Breaks", value: `${fmtMinutes(c.breakMinutes)} in ${plural(c.breaks, "entry", "entries")}` });
  }
  if (c.clocked > 0) {
    list.push({ label: c.state === "InNow" ? "Worked so far" : "Worked", value: judged ? `${fmtMinutes(c.worked)} of ${fmtMinutes(c.expected)}` : fmtMinutes(c.worked) });
  }
  if (judged) {
    const { shift, assigned } = shiftOf(c, r);
    const rule = (rules.value?.shifts || []).find((s) => s.name === shift);
    const window = rule ? ` · starts ${rule.start}, late after ${rule.lateAfter}` : "";
    list.push({ label: "Shift", value: `${assigned ? shift : `${shift} (none assigned, default used)`}${window}` });
  }
  if (c.lateMinutes > 0) {
    list.push({ label: "Late by", value: `${fmtLate(c.lateMinutes)} · ${c.adjustments > 0 ? "time adjustment entered" : "no time adjustment entered"}` });
  }
  if (c.adjustments > 0) {
    list.push({ label: "Time adjustments", value: `${fmtMinutes(c.adjustMinutes)} in ${plural(c.adjustments, "entry", "entries")}` });
  }
  if (c.wfh) {
    list.push({ label: "Work From Home", value: c.wfh === "Unspecified" ? "Yes" : c.wfh });
  }
  if (c.leave) {
    list.push({ label: "Leave", value: `${c.leave.kind}${c.leave.status ? " · " + c.leave.status : ""}${c.leave.category ? " · " + c.leave.category : ""}` });
  }
  if (d.isHoliday) {
    list.push({ label: "Holiday", value: d.holidayTitle || "Holiday" });
  }
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

// The marks of a time-in and of the ways it can end; Movement Register entries take theirs from RECORD_LOOK.
const TONES = { in: RECORD_LOOK.Session.colour, out: "#647a82", warn: "#b5347f", live: "#1b75ab" };

function movementMeta (m) {
  const who = row.value?.fullName;
  return [
    m.approver ? `Permission by ${m.approver}` : "",
    m.enteredBy && m.enteredBy !== who ? `Entered by ${m.enteredBy}` : "",
    m.enteredDaysLater > 0 ? `Entered ${plural(m.enteredDaysLater, "day", "days")} after the date` : "",
    m.enteredDaysLater < 0 ? `Entered ${plural(-m.enteredDaysLater, "day", "days")} ahead of the date` : "",
    m.notifyToStakeholders ? "Stakeholders notified" : ""
  ].filter(Boolean).join(" · ");
}

// Time-ins, time-outs and Movement Register entries as one list, by the time they happened.
const timeline = computed(() => {
  const d = day.value?.date || "";
  const c = cell.value || {};
  const items = [];

  (detail.value?.sessions || []).forEach((s, index) => {
    items.push({
      key: `in-${index}`,
      at: s.in,
      order: 0,
      time: fmtClock(s.in, d),
      icon: "o_login",
      colour: TONES.in,
      title: "Time in",
      badge: index === 0 && c.lateMinutes > 0 ? `late by ${fmtLate(c.lateMinutes)}` : ""
    });

    if (s.status === "Closed") {
      items.push({ key: `out-${index}`, at: s.out, order: 1, time: fmtClock(s.out, d), icon: "o_logout", colour: TONES.out, title: "Time out", text: `${fmtMinutes(s.minutes)} after the time-in` });
    } else if (s.status === "Live") {
      items.push({ key: `out-${index}`, at: `${s.in}~`, order: 1, time: "now", icon: "o_hourglass_empty", colour: TONES.live, title: "Still in", text: `${fmtMinutes(s.minutes)} so far` });
    } else if (s.status === "TooLong") {
      items.push({
        key: `out-${index}`,
        at: `${s.in}~`,
        order: 1,
        time: fmtClock(s.out, d),
        icon: "o_timer_off",
        colour: TONES.warn,
        title: "Time out, too late to count",
        text: "The time-out came so long after the time-in that it was plainly forgotten, so the hours of this time-in are not counted."
      });
    } else {
      items.push({ key: `out-${index}`, at: `${s.in}~`, order: 1, time: "—", icon: "o_timer_off", colour: TONES.warn, title: "No time-out", text: "This time-in was never closed, so its hours are not counted." });
    }
  });

  (detail.value?.movements || []).forEach((m, index) => {
    const length = movementLength(m);
    const look = RECORD_LOOK[m.kind] || RECORD_LOOK.Other;
    items.push({
      key: `mv-${index}`,
      at: m.enteredAt || "",
      order: 2,
      time: fmtClock(m.enteredAt, d),
      timeNote: "entered",
      icon: look.icon,
      colour: look.colour,
      title: `${m.type || "Movement Register"}${length ? ` · ${length}` : ""}`,
      text: m.message || "",
      meta: movementMeta(m)
    });
  });

  return items.sort((a, b) => String(a.at).localeCompare(String(b.at)) || a.order - b.order);
});

const hasMovement = computed(() => (detail.value?.movements || []).length > 0);

async function load () {
  const { row: r, day: d } = props.payload;
  if (!r?.employeeId || !d?.date) return;
  loading.value = true;
  loadError.value = false;
  try {
    detail.value = await analyticsService.getAttendanceRegisterDay(r.employeeId, d.date);
  } catch {
    loadError.value = true;
    detail.value = { employee: r, day: d, cell: props.payload.cell, sessions: [], movements: [] };
  } finally {
    loading.value = false;
  }
}

watch(() => props.payload, load, { immediate: true });
</script>

<style scoped>
.att-day__banner {
  --cell-colour: #b0b4b8;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 12px 14px;
  border-radius: 10px;
  border: 1px solid color-mix(in srgb, var(--cell-colour) 40%, #fff);
  background: color-mix(in srgb, var(--cell-colour) 10%, #fff);
}
.att-day__icon,
.att-day__glyph {
  color: var(--cell-colour);
  flex: 0 0 auto;
}
.att-day__glyph {
  font-size: 20px;
  line-height: 24px;
  width: 24px;
  text-align: center;
}
.att-day__banner-text {
  min-width: 0;
}
.att-day__state {
  font-size: 15px;
  font-weight: 600;
  color: var(--ink-900, #16262c);
}
.att-day__facts {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 10px 16px;
}

/* The merged timeline: time on the left, a mark on a rail, what happened on the right. */
.att-line {
  list-style: none;
  margin: 0;
  padding: 0;
}
.att-line__item {
  --tone: #647a82;
  display: grid;
  grid-template-columns: 96px 28px minmax(0, 1fr);
  column-gap: 10px;
  position: relative;
  padding-bottom: 12px;
}
/* The rail joining the marks. */
.att-line__item::before {
  content: "";
  position: absolute;
  left: 119px;
  top: 28px;
  bottom: 0;
  width: 2px;
  background: var(--line, #dde6e8);
}
.att-line__item:last-child {
  padding-bottom: 0;
}
.att-line__item:last-child::before {
  display: none;
}
.att-line__time {
  padding-top: 4px;
  text-align: right;
  font-size: 13px;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  color: var(--ink-900, #16262c);
}
.att-line__time-note {
  font-size: 10.5px;
  font-weight: 400;
  color: var(--ink-500, #647a82);
}
.att-line__mark {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--tone) 14%, #fff);
  color: var(--tone);
}
.att-line__body {
  min-width: 0;
  padding-top: 4px;
}
.att-line__title {
  font-size: 13.5px;
  font-weight: 600;
  color: var(--ink-900, #16262c);
}
.att-line__badge {
  margin-left: 6px;
  padding: 1px 7px;
  border-radius: 9px;
  background: color-mix(in srgb, #a66f00 14%, #fff);
  color: #7a5200;
  font-size: 11px;
  font-weight: 600;
  white-space: nowrap;
}
.att-line__text {
  margin-top: 2px;
  font-size: 13px;
  color: var(--ink-700, #3a4d54);
  white-space: pre-line;
  overflow-wrap: anywhere;
}
.att-line__meta {
  margin-top: 2px;
  font-size: 11.5px;
  color: var(--ink-500, #647a82);
}
</style>
