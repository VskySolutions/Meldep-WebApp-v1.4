<template>
  <div
    ref="root"
    :class="['tsr-grid', { 'tsr-grid--compact': compact }]"
    @mouseover="onOver"
    @mouseleave="hideHover"
    @focusin="onOver"
    @focusout="hideHover"
    @scroll.passive="hideHover"
  >
    <table class="tsr-table">
      <thead ref="thead">
        <tr>
          <th class="tsr-th tsr-th--emp">Employee</th>
          <th class="tsr-th tsr-th--shift">
            <span class="tsr-abbr">Shift</span>
            <q-tooltip :delay="150" max-width="320px" anchor="bottom middle" self="top middle">
              <div class="tsr-tip__title">Shift</div>
              <div class="tsr-tip__text">
                The working shift on the employee's designation record. S1, S2 and S3 are Shift 1, 2 and 3;
                “—” means none is assigned and the default shift is used.
              </div>
            </q-tooltip>
          </th>
          <th
            v-for="(day, i) in visibleDays"
            :key="day.date"
            :class="['tsr-th', 'tsr-th--day', dayClass(day)]"
          >
            <div v-if="showMonth(i)" class="tsr-th__mon">{{ day.month }}</div>
            <div class="tsr-th__dow">{{ day.day }}</div>
            <div class="tsr-th__num">{{ day.dayNumber }}</div>
            <q-tooltip :delay="150" max-width="280px" anchor="bottom middle" self="top middle">
              <div class="tsr-tip__title">{{ formatDayLabel(day.date) }}</div>
              <div class="tsr-tip__text">{{ dayText(day) }}</div>
            </q-tooltip>
          </th>
          <th v-for="col in TOTAL_COLUMNS" :key="col.key" class="tsr-th tsr-th--tot">
            <span class="tsr-abbr">{{ col.abbr }}</span>
            <q-tooltip :delay="150" max-width="320px" anchor="bottom middle" self="top middle">
              <div class="tsr-tip__title">{{ col.full }}</div>
              <div class="tsr-tip__text">{{ col.desc }}</div>
            </q-tooltip>
          </th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="(row, r) in rows" :key="row.employeeId" class="tsr-tr">
          <td class="tsr-td tsr-td--emp">
            <div
              class="tsr-emp tsr-emp--link"
              role="button"
              tabindex="0"
              :aria-label="`Open the employee record of ${row.fullName}`"
              @click="$emit('employee', row)"
              @keydown.enter.prevent="$emit('employee', row)"
            >
              <app-avatar :name="row.fullName" :size="24" />
              <div class="tsr-emp__text">
                <div class="tsr-emp__name ellipsis">{{ row.fullName }}</div>
                <div class="tsr-emp__meta ellipsis">{{ metaOf(row) }}</div>
              </div>
              <q-tooltip :delay="300" anchor="bottom left" self="top left">Open the employee record</q-tooltip>
            </div>
          </td>
          <td class="tsr-td tsr-td--shift">
            <span :class="['tsr-shift', { 'tsr-shift--none': !row.shiftAssigned }]">{{ shiftShort(row) }}</span>
            <q-tooltip :delay="150" max-width="320px" anchor="bottom middle" self="top middle">
              <div class="tsr-tip__title">{{ shiftFull(row) }}</div>
              <div class="tsr-tip__text">{{ shiftText(row) }}</div>
            </q-tooltip>
          </td>
          <td
            v-for="i in visibleIndexes"
            :key="days[i].date"
            :class="['tsr-td', 'tsr-td--cell', dayClass(days[i])]"
            :data-row="r"
            :data-day="i"
          >
            <register-cell
              :cell="row.cells[i]"
              :day="days[i]"
              :row="row"
              :compact="compact"
              @click="$emit('cell', { row, day: days[i], cell: row.cells[i] })"
            />
          </td>
          <td v-for="col in TOTAL_COLUMNS" :key="col.key" :class="['tsr-td', 'tsr-td--tot', col.cls]">
            {{ col.value(row.totals || {}) }}
          </td>
        </tr>
      </tbody>

      <tfoot>
        <tr class="tsr-foot">
          <td class="tsr-td tsr-td--emp tsr-foot__label">
            <span class="tsr-abbr">Complete / expected</span>
            <q-tooltip :delay="150" max-width="320px" anchor="top middle" self="bottom middle">
              <div class="tsr-tip__title">Complete / expected</div>
              <div class="tsr-tip__text">
                Per day: employees whose expected hours were reached (on time or late), out of those expected
                to fill the day. Days before joining, holidays and full-day leaves are not expected.
              </div>
            </q-tooltip>
          </td>
          <td class="tsr-td tsr-td--shift" />
          <td
            v-for="i in visibleIndexes"
            :key="days[i].date"
            :class="['tsr-td', 'tsr-td--cell', 'tsr-foot__cell', dayClass(days[i])]"
          >
            <template v-if="perDay[i] && perDay[i].expected">{{ perDay[i].complete }}/{{ perDay[i].expected }}</template>
            <span v-else class="text-grey-5">–</span>
            <q-tooltip :delay="150" max-width="300px" anchor="top middle" self="bottom middle">
              <div class="tsr-tip__title">{{ formatDayLabel(days[i].date) }}</div>
              <div class="tsr-tip__text">{{ footText(perDay[i]) }}</div>
            </q-tooltip>
          </td>
          <td v-for="col in TOTAL_COLUMNS" :key="col.key" :class="['tsr-td', 'tsr-td--tot', col.cls]">
            {{ col.value(totals) }}
          </td>
        </tr>
      </tfoot>
    </table>

    <!-- One hover card for every cell (delegated), instead of thousands of tooltip instances. -->
    <teleport to="body">
      <div v-if="hover" class="tsr-hover" :style="hoverStyle" role="tooltip">
        <div class="tsr-hover__head" :style="{ '--cell-colour': hover.colour }">
          <q-icon v-if="hover.icon" :name="hover.icon" size="16px" class="tsr-hover__icon" />
          <b v-else class="tsr-hover__glyph">{{ hover.glyph }}</b>
          <span class="tsr-hover__title">{{ hover.title }}</span>
        </div>
        <div class="tsr-hover__sub">{{ hover.subtitle }}</div>
        <div class="tsr-hover__text">{{ hover.text }}</div>
        <ul v-if="hover.facts.length" class="tsr-hover__facts">
          <li v-for="fact in hover.facts" :key="fact">{{ fact }}</li>
        </ul>
      </div>
    </teleport>
  </div>
</template>

<script setup>
// The register itself: a plain table (not q-table) so the employee column and the header can stay put while
// a month of columns scrolls, and so thousands of cells render without per-cell overhead beyond the cell
// button. Rows are employees, columns are days, the footer counts complete days per column. Every short
// form (OT, Dly, Par, Pnd, Hrs, S1, H, L, ½) has a tooltip with its full form; cells share one delegated
// hover card. The grid takes its full content height (the page scrolls); only the columns scroll sideways.
import { ref, computed, watch, nextTick, onMounted, onBeforeUnmount } from "vue";
import AppAvatar from "components/common/AppAvatar.vue";
import RegisterCell from "modules/analytics/components/register/_registerCell.vue";
import { TOTAL_COLUMNS, cellHover } from "modules/analytics/utils/registerStates";
import { formatDayLabel } from "modules/analytics/composables/useDateRangePresets";

const props = defineProps({
  days: { type: Array, default: () => [] },
  rows: { type: Array, default: () => [] },
  // Per-day totals aligned with `days`: { expected, complete, onTime, delayed, partial, pending, due, leave }
  perDay: { type: Array, default: () => [] },
  // The rules the API applied (shift windows and cutoffs), for the shift tooltips.
  rules: { type: Object, default: null },
  compact: { type: Boolean, default: false },
  hideHolidays: { type: Boolean, default: false }
});

defineEmits(["cell", "employee"]);

const visibleIndexes = computed(() =>
  props.days.map((d, i) => i).filter((i) => !props.hideHolidays || !props.days[i].isHoliday));

const visibleDays = computed(() => visibleIndexes.value.map((i) => props.days[i]));

const totals = computed(() => {
  const t = { loggedHours: 0, onTime: 0, delayed: 0, partial: 0, pending: 0 };
  for (const row of props.rows) {
    t.loggedHours += Number(row.totals?.loggedHours || 0);
    t.onTime += row.totals?.onTime || 0;
    t.delayed += row.totals?.delayed || 0;
    t.partial += row.totals?.partial || 0;
    t.pending += row.totals?.pending || 0;
  }
  return t;
});

const root = ref(null);
const thead = ref(null);

// ---- pinned header -------------------------------------------------------------------------------------
// The page scrolls as a whole while the table scrolls sideways inside its own box, so CSS "position: sticky"
// cannot pin the header row to the window. Instead the header row is shifted down by however far the table's
// top has gone under the app header, which reads the same as a fixed header and keeps the sideways scroll.
let pinFrame = 0;

function pinHeader () {
  const box = root.value;
  const head = thead.value;
  if (!box || !head) return;
  const rect = box.getBoundingClientRect();
  const appHeader = document.querySelector(".q-header");
  const ceiling = appHeader ? appHeader.getBoundingClientRect().bottom : 0;
  const maxShift = Math.max(0, rect.height - head.offsetHeight);
  const shift = Math.round(Math.min(Math.max(ceiling - rect.top, 0), maxShift));
  head.style.transform = shift > 0 ? `translateY(${shift}px)` : "";
  head.classList.toggle("tsr-thead--pinned", shift > 0);
}

function schedulePin () {
  cancelAnimationFrame(pinFrame);
  pinFrame = requestAnimationFrame(pinHeader);
}

onMounted(() => {
  // Capture phase: catches the window scroll and any scrolling layout container alike.
  window.addEventListener("scroll", schedulePin, { passive: true, capture: true });
  window.addEventListener("resize", schedulePin);
  nextTick(pinHeader);
});

onBeforeUnmount(() => {
  window.removeEventListener("scroll", schedulePin, { capture: true });
  window.removeEventListener("resize", schedulePin);
  cancelAnimationFrame(pinFrame);
  hideHover();
});

watch(() => [props.rows, props.days, props.compact, props.hideHolidays], () => nextTick(pinHeader));

// ---- hover card ----------------------------------------------------------------------------------------
const hover = ref(null);
const hoverPos = ref({ x: 0, y: 0, above: false });
const CARD_WIDTH = 320;
const CARD_GUESS_HEIGHT = 190;

function onOver (event) {
  const td = event.target?.closest?.("td.tsr-td--cell");
  if (!td || !root.value?.contains(td)) {
    if (hover.value) hideHover();
    return;
  }
  const r = Number(td.dataset.row);
  const d = Number(td.dataset.day);
  const row = props.rows[r];
  const day = props.days[d];
  const cell = row?.cells?.[d];
  if (!row || !day || !cell || cell.state === "NotEmployed") {
    hideHover();
    return;
  }
  const key = `${row.employeeId}|${day.date}`;
  if (hover.value?.key !== key) hover.value = { key, ...cellHover(row, day, cell) };

  const rect = td.getBoundingClientRect();
  const half = CARD_WIDTH / 2 + 8;
  const x = Math.min(Math.max(rect.left + rect.width / 2, half), window.innerWidth - half);
  const above = rect.bottom + CARD_GUESS_HEIGHT > window.innerHeight;
  hoverPos.value = { x, y: above ? rect.top - 6 : rect.bottom + 6, above };
}

function hideHover () {
  hover.value = null;
}

const hoverStyle = computed(() => ({
  left: `${hoverPos.value.x}px`,
  top: `${hoverPos.value.y}px`,
  width: `${CARD_WIDTH}px`,
  transform: hoverPos.value.above ? "translate(-50%, -100%)" : "translateX(-50%)"
}));

// ---- headers & short forms -----------------------------------------------------------------------------
// The month shows over the first column and wherever it changes.
function showMonth (i) {
  if (i === 0) return true;
  return visibleDays.value[i].month !== visibleDays.value[i - 1].month;
}

function dayClass (day) {
  return {
    "tsr-col--holiday": day?.isHoliday,
    "tsr-col--today": day?.isToday,
    "tsr-col--future": day?.isFuture
  };
}

function dayText (day) {
  if (day.isHoliday) return `${day.holidayTitle || "Holiday"}. Nothing is expected on this day.`;
  if (day.isToday) return "Today. Days are “due” until their cutoff passes.";
  if (day.isFuture) return "Not yet due.";
  return "Working day.";
}

function metaOf (row) {
  return [row.employeeCode, row.department].filter(Boolean).join(" · ");
}

function shiftShort (row) {
  if (!row.shiftAssigned) return "—";
  const m = /(\d+)/.exec(row.shift || "");
  return m ? `S${m[1]}` : row.shift;
}

function shiftRule (name) {
  return (props.rules?.shifts || []).find((s) => s.name === name) || null;
}

function shiftFull (row) {
  return row.shiftAssigned ? row.shift : "No shift assigned";
}

function shiftText (row) {
  const rule = shiftRule(row.shift);
  const window = rule ? `${rule.start}–${rule.end}${rule.endsNextDay ? " (next day)" : ""}, cutoff ${rule.cutoff}` : "";
  if (row.shiftAssigned) return window ? `Works ${window}.` : "";
  const says = row.shiftSource ? ` The record says “${row.shiftSource}”, which is not a known shift.` : "";
  return `Judged as ${row.shift}${window ? ` (${window})` : ""}, the default, until a shift is set on the employee's designation record.${says}`;
}

function footText (t) {
  if (!t || !t.expected) return "Nothing expected on this day.";
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
</script>

<style scoped>
/* Full content height: the page scrolls vertically, the table scrolls sideways when it is wider. */
.tsr-grid {
  overflow-x: auto;
}
.tsr-table {
  border-collapse: separate;
  border-spacing: 0;
  width: max-content;
  min-width: 100%;
  font-size: 12.5px;
}
.tsr-th,
.tsr-td {
  padding: 3px 4px;
  border-bottom: 1px solid var(--line, #dde6e8);
  border-right: 1px solid #eef2f3;
  background: #fff;
  white-space: nowrap;
  vertical-align: middle;
}
.tsr-table thead {
  position: relative;
  z-index: 5;
  will-change: transform;
}
.tsr-thead--pinned .tsr-th {
  box-shadow: 0 2px 3px rgba(18, 54, 69, 0.12);
}
/* Header row in the theme blue with white text, like the other Meldep tables, so it never blends with the rows. */
.tsr-th {
  position: sticky;
  z-index: 2;
  background: var(--q-primary);
  border-right-color: rgba(255, 255, 255, 0.22);
  border-bottom-color: var(--q-primary);
  font-weight: 600;
  color: #fff;
  text-align: center;
  vertical-align: bottom;
  line-height: 1.15;
  padding-bottom: 5px;
}
.tsr-th__mon {
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: rgba(255, 255, 255, 0.85);
}
.tsr-th__dow {
  font-size: 10.5px;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.82);
}
.tsr-th__num {
  font-size: 13px;
}
/* A short form: dotted underline says "hover me", the tooltip gives the full form. */
.tsr-abbr {
  border-bottom: 1px dotted currentColor;
  cursor: help;
}
.tsr-tip__title {
  font-size: 12.5px;
  font-weight: 600;
  margin-bottom: 2px;
}
.tsr-tip__text {
  font-size: 11.5px;
  line-height: 1.4;
  white-space: normal;
}
.tsr-th--emp,
.tsr-td--emp {
  position: sticky;
  left: 0;
  min-width: 232px;
  max-width: 232px;
  text-align: left;
}
.tsr-th--shift,
.tsr-td--shift {
  position: sticky;
  left: 232px;
  min-width: 52px;
  text-align: center;
}
.tsr-td--emp,
.tsr-td--shift {
  z-index: 1;
}
.tsr-th--emp,
.tsr-th--shift {
  z-index: 4;
}
.tsr-th--day,
.tsr-td--cell {
  min-width: 70px;
}
.tsr-td--cell {
  padding: 2px 3px;
}
.tsr-grid--compact .tsr-th--day,
.tsr-grid--compact .tsr-td--cell {
  min-width: 36px;
}
.tsr-grid--compact .tsr-td--cell {
  padding: 2px;
}
.tsr-th--tot,
.tsr-td--tot {
  min-width: 44px;
  text-align: right;
  font-variant-numeric: tabular-nums;
}
.tsr-td--ok { color: #0a7a0a; }
.tsr-td--dly { color: #a66f00; }
.tsr-td--par { color: #c2410c; }
.tsr-td--pnd { color: #b32424; font-weight: 600; }

.tsr-col--holiday {
  background: #f4f6f7;
}
.tsr-th.tsr-col--holiday {
  background: color-mix(in srgb, var(--q-primary) 86%, #000);
}
.tsr-th.tsr-col--today {
  box-shadow: inset 0 -3px 0 #fff;
}
.tsr-th.tsr-col--today .tsr-th__num {
  font-weight: 700;
}
.tsr-td.tsr-col--today {
  background: var(--brand-050, #f3f8fb);
}
.tsr-tr:hover .tsr-td {
  background: #fbfcfd;
}
.tsr-tr:hover .tsr-td.tsr-col--today {
  background: var(--brand-100, #e3f1f8);
}

.tsr-emp {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
}
.tsr-emp--link {
  cursor: pointer;
  border-radius: 6px;
  padding: 1px 4px;
  margin: -1px -4px;
}
.tsr-emp--link:hover .tsr-emp__name {
  color: var(--q-primary);
  text-decoration: underline;
}
.tsr-emp--link:focus-visible {
  outline: 2px solid var(--q-primary);
  outline-offset: 1px;
}
.tsr-emp__text {
  min-width: 0;
}
.tsr-emp__name {
  font-weight: 500;
  color: var(--ink-900, #16262c);
}
.tsr-emp__meta {
  font-size: 11px;
  color: var(--ink-500, #647a82);
}
.tsr-shift {
  display: inline-block;
  min-width: 28px;
  padding: 1px 6px;
  border-radius: 10px;
  background: var(--brand-100, #e3f1f8);
  color: var(--q-primary);
  font-size: 11px;
  font-weight: 600;
  cursor: help;
}
.tsr-shift--none {
  background: #f1f3f4;
  color: var(--ink-500, #647a82);
}

.tsr-foot .tsr-td {
  position: sticky;
  bottom: 0;
  z-index: 2;
  background: #f7fafb;
  font-weight: 600;
  border-top: 2px solid var(--line, #dde6e8);
}
.tsr-foot .tsr-td--emp,
.tsr-foot .tsr-td--shift {
  z-index: 4;
}
.tsr-foot__label {
  color: var(--ink-700, #3a4d54);
}
.tsr-foot__cell {
  text-align: center;
  font-variant-numeric: tabular-nums;
  font-size: 11.5px;
}

/* The shared hover card of the cells (teleported to body, so it is never clipped by the scroll area). */
.tsr-hover {
  position: fixed;
  z-index: 7000;
  padding: 8px 10px 9px;
  border: 1px solid var(--line, #dde6e8);
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 8px 24px rgba(18, 54, 69, 0.18);
  color: var(--ink-900, #16262c);
  font-size: 12px;
  line-height: 1.4;
  pointer-events: none;
  white-space: normal;
}
.tsr-hover__head {
  --cell-colour: #647a82;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
}
.tsr-hover__icon,
.tsr-hover__glyph {
  color: var(--cell-colour);
}
.tsr-hover__glyph {
  display: inline-block;
  width: 16px;
  text-align: center;
}
.tsr-hover__sub {
  color: var(--ink-500, #647a82);
  font-size: 11.5px;
  margin-bottom: 4px;
}
.tsr-hover__text {
  margin-bottom: 4px;
}
.tsr-hover__facts {
  margin: 0;
  padding-left: 16px;
  color: var(--ink-700, #3a4d54);
}
</style>
