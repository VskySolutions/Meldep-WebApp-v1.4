<template>
  <app-side-panel
    title="How to use the Attendance Register"
    subtitle="Time In / Time Out and the Movement Register in one day-by-day view"
    width="52vw"
    min-width="680px"
    hide-save
    cancel-label="Close"
    @close="$emit('close')"
  >
    <nav class="att-guide__toc">
      <a v-for="s in sections" :key="s.id" :href="'#' + s.id" @click.prevent="jump(s.id)">{{ s.title }}</a>
    </nav>

    <section :id="sections[0].id" class="att-guide__section">
      <h3>1. What this page is for</h3>
      <p>
        The register brings two records together, <b>Time In / Time Out</b> and the <b>Movement Register</b>
        (breaks, time adjustments, work from home), and answers for every active employee and every day of a
        period: <b>did they time in, when, for how long, and what did they declare that day?</b> Rows are
        employees, columns are days, and each cell is the verdict for that employee on that day.
      </p>
      <p>
        It lists the site's <b>active employees that have a department record</b>. Client contacts are stored
        as employees too but belong to no department, so they never appear here. Days before an employee's
        joining date or after the release date stay blank.
      </p>
      <p>
        It is read-only. Time-ins and Movement Register entries are still made by the employees from their
        dashboard; a shift or a leave is fixed on the employee and leave records.
      </p>
    </section>

    <section :id="sections[1].id" class="att-guide__section">
      <h3>2. Choose the period</h3>
      <ul>
        <li><b>This Week</b> and <b>Last Week</b> run Monday to Sunday.</li>
        <li><b>This Month</b> runs from the 1st to the last day of the current month; <b>Last Month</b> is the whole previous calendar month.</li>
        <li><b>By Weeks</b> shows one week at a time: pick any Sunday in the calendar (only Sundays can be chosen, up to this week) or step with the arrows, and the register shows that Monday to Sunday.</li>
        <li>
          <b>Custom</b> shows the From and To dates, pre-filled with the period you were on: set them and press
          <b>Apply</b>. A period may cover at most {{ maxRangeDays }} days.
        </li>
      </ul>
      <p>The period and the filters you choose are remembered on this computer for your next visit.</p>
    </section>

    <section :id="sections[2].id" class="att-guide__section">
      <h3>3. Read the grid</h3>
      <p>
        Hover any cell for a plain-language explanation; click it to open the day with every time-in and every
        Movement Register entry behind it. <b>Cell shows</b> picks what a cell prints: <b>Hours</b> (the hours
        worked), <b>In–Out</b> (the first time-in and the last time-out) or <b>Compact</b> (the icon alone).
        Every short form has a tooltip with its full form. The columns on the right total the period for the
        employee, and the last row counts, per day, how many employees were present out of those expected.
      </p>
      <table class="att-guide__table">
        <thead>
          <tr><th>Cell</th><th>Meaning</th><th>What to do</th></tr>
        </thead>
        <tbody>
          <tr v-for="s in legend" :key="s.key">
            <td class="att-guide__cell-col">
              <span class="att-guide__swatch" :style="{ '--cell-colour': s.colour }">
                <q-icon v-if="s.icon" :name="s.icon" size="14px" />
                <b v-else>{{ s.glyph }}</b>
              </span>
              {{ s.label }}
            </td>
            <td>{{ s.description }}</td>
            <td>{{ actions[s.key] }}</td>
          </tr>
          <tr>
            <td class="att-guide__cell-col"><span class="att-guide__swatch att-guide__swatch--plain att-guide__swatch--flag" /> Also late</td>
            <td>An amber corner: the first time-in was late on a day that shows another state (short hours, no time-out, in now).</td>
            <td>Read it with the state; the Late count includes these days.</td>
          </tr>
          <tr>
            <td class="att-guide__cell-col"><span class="att-guide__swatch att-guide__swatch--plain"><b>½</b></span> Half day</td>
            <td>A half-day leave covers the day, so {{ halfDayText }} are expected instead of {{ fullDayText }}. With the leave on the first half, the arrival is not judged late.</td>
            <td>Nothing; the leave is already accounted for.</td>
          </tr>
          <tr>
            <td class="att-guide__cell-col"><span class="att-guide__swatch att-guide__swatch--plain"><small>W B T</small></span> Movement</td>
            <td>Letters beside the hours: <b>W</b> a Work From Home entry (with a time-in), <b>B</b> break entries, <b>T</b> a time adjustment. In the compact view one dot stands for all three.</td>
            <td>Hover or open the day to read the entries.</td>
          </tr>
          <tr>
            <td class="att-guide__cell-col"><span class="att-guide__swatch att-guide__swatch--plain"><small>+8</small></span> Off-day hours</td>
            <td>The employee timed in on a holiday or a leave day. The day stays H or L.</td>
            <td>Check whether the leave or holiday is right.</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section :id="sections[3].id" class="att-guide__section">
      <h3>4. How a day is judged</h3>
      <ol>
        <li><b>Is it a working day for this employee?</b> Sundays and the site's leave-schedule dates (public holidays, Saturdays off) are <b>H</b>. Days before joining or after release are blank. A full-day leave is <b>L</b>. Nobody is expected on these.</li>
        <li><b>Which day does a time-in count for?</b> Its own date; a time-in before {{ dayStartsAt }} counts for the day before, so a night shift stays on the day it began.</li>
        <li><b>How many hours were productive?</b> Time in to time out, added over the day's time-ins, <b>minus the Break and the Time Adjustment minutes</b> of the Movement Register. {{ fullDayText }} are required on a working day, {{ halfDayText }} under a half-day leave.</li>
        <li><b>Was the arrival late?</b> The first time-in is late when it comes more than {{ graceText }} after the start of the employee's shift. The shift is read from the designation record; without one, {{ defaultShift }} is used and the cell says so. A Time Adjustment entered that day is shown beside the cell; the day still counts as late.</li>
        <li><b>Was the time-out made?</b> A time-in still open after {{ forgottenText }}, or closed that long afterwards, is a forgotten time-out: the day counts as attended but its hours are not counted.</li>
        <li>
          <b>Verdict</b>, first match wins: still timed in → <b>In now</b>; a time-out missing → <b>No time-out</b>;
          fewer hours than expected → <b>Short hours</b>; a late arrival → <b>Late</b>; otherwise <b>On time</b>.
          With no time-in: a Work From Home entry → <b>WFH, no time-in</b>; today before the shift start plus
          grace → <b>Not in yet</b>; otherwise <b>Absent</b>.
        </li>
      </ol>
      <table class="att-guide__table">
        <thead>
          <tr><th>Shift</th><th>Working window</th><th>A first time-in is late after</th></tr>
        </thead>
        <tbody>
          <tr v-for="s in shifts" :key="s.name">
            <td>{{ s.name }}</td>
            <td>{{ fmtTime12(s.start) }} – {{ fmtTime12(s.end) }}{{ s.endsNextDay ? " (next day)" : "" }}</td>
            <td>{{ fmtTime12(s.lateAfter) }}</td>
          </tr>
          <tr>
            <td>Not assigned</td>
            <td>judged as {{ defaultShift }}</td>
            <td>{{ defaultLateAfter }}</td>
          </tr>
        </tbody>
      </table>
      <p class="text-caption text-grey-7">
        Time-ins and time-outs are stored in UTC and shown here in the site time zone ({{ timeZone }}), to the minute.
      </p>
    </section>

    <section :id="sections[4].id" class="att-guide__section">
      <h3>5. The Movement Register on the grid</h3>
      <ul>
        <li><b>Break</b>: the minutes the employee declared, with the reason and who gave permission. They are taken out of the productive hours and never change the state by themselves, except that a day can fall short because of them.</li>
        <li><b>Time Adjustment</b>: a late arrival, an early login or a shift change the employee declared. Its minutes are taken out of the productive hours like a break, so a day can fall short because of it. It also marks a late day as declared (the day still counts late), and the Late tile counts the late days that have none.</li>
        <li><b>Work From Home</b>: a full day or a half. With a time-in the day is judged like any other and carries a small <b>W</b>; without one the day shows as <b>WFH, no time-in</b> instead of absent.</li>
        <li>An entry belongs to the date it was filed under, as in the Movement Register list, whenever it was typed in. The day panel says when an entry was made afterwards or ahead.</li>
        <li>These records carry the name of the person who gave permission, not an approval status.</li>
      </ul>
    </section>

    <section :id="sections[5].id" class="att-guide__section">
      <h3>6. Leaves and holidays</h3>
      <ul>
        <li>A leave counts <b>from the moment it is applied</b>: Applied, Sent to Approver and Approved all cover the day. Only {{ ignoredStatuses }} leaves are ignored. The tooltip shows the status.</li>
        <li>A <b>half-day</b> leave keeps the day expected at {{ halfDayText }} and marks the cell with ½.</li>
        <li><b>Every Sunday</b> is a weekly off. <b>Saturdays</b> are working days unless the leave schedule lists them off.</li>
        <li>Holidays beat leaves, and leaves beat time-ins: hours worked on such a day appear as a small “+hours” hint without changing the cell.</li>
      </ul>
    </section>

    <section :id="sections[6].id" class="att-guide__section">
      <h3>7. Tiles, filters and search</h3>
      <ul>
        <li>The <b>tiles</b> count employee-days in the period for the employees currently listed. Click a tile to keep only the employees it counts (for example everyone with an absent day); click it again to release. <b>Late</b> counts every late arrival, whatever else happened that day, so the tiles need not add up to the expected days.</li>
        <li><b>Filters</b> sit under the title bar: Department, Team Lead and Team Members from the org structure, Shift, Day state and Movement, plus a switch to hide the Sunday and holiday columns. <b>Day state</b> offers “Late, no time adjustment” for the late days nobody declared. <b>Movement</b> keeps the employees with a break, a time adjustment or work from home in the period.</li>
        <li><b>Search</b> matches the employee's name, code or department.</li>
      </ul>
    </section>

    <section :id="sections[7].id" class="att-guide__section">
      <h3>8. Day details and export</h3>
      <ul>
        <li>Click a cell to open the <b>day panel</b>: the verdict in words, the facts it rests on (first time-in, last time-out, clocked, breaks, worked, shift, how late) and one timeline of every time-in, time-out and Movement Register entry, with its message and who gave permission.</li>
        <li>Click an <b>employee's name</b> to open their employee record, the same view the employee list offers.</li>
        <li>The <b>icon</b> beside a name, or <b>View week</b> / <b>View period</b> in the day panel, opens that employee's <b>attendance</b>: the totals, one row per day, and every time-in and Movement Register entry in one list, for the selected period or any week you pick by its Sunday. Both tables export to CSV.</li>
        <li>The <b>WFH</b>, <b>Brk</b> and <b>TA</b> totals on the right of a row are links. Click one to see that employee's Work From Home days, breaks or time adjustments for the period <b>date by date</b>: how each day went, how much was declared, and every entry with its message, who gave permission and when it was entered. A switch at the top moves between the three, and the list exports to CSV.</li>
        <li>The <b>hours columns</b> on the right of a row read left to right: <b>Req</b> (required: {{ fullDayText }} a working day, {{ halfDayText }} under a half-day leave; today counts once the employee has timed out), <b>Clk</b> (clocked, time in to time out), <b>Brk</b> and <b>TA</b> (break and time adjustment hours), <b>Prod</b> (productive = clocked minus breaks and time adjustments), <b>Short</b> (required minus productive when productive is lower, in red) and <b>Extra</b> (productive minus required when productive is higher, in green); an employee has one or the other. The balance is struck over the whole period, so a longer day offsets a shorter one. A day with no time-in (absent, work from home) or no time-out adds zero productive hours. The <b>Hours short</b> tile adds the shortfalls up and keeps the employees who are short.</li>
        <li><b>Export CSV</b> downloads the grid as shown: one row per employee, one column per visible day with the state, the times, the hours and the movement, and the period totals.</li>
      </ul>
    </section>

    <section :id="sections[8].id" class="att-guide__section">
      <h3>9. Common questions</h3>
      <dl class="att-guide__faq">
        <dt>Almost every day of an employee is Late by about the same amount. Why?</dt>
        <dd>The shift on the designation record does not match the hours the employee keeps. The employee's attendance panel shows the usual time-in next to the shift; correct the shift on the employee record and the days are judged against the right start.</dd>
        <dt>The employee was in the office, so why is the day Absent?</dt>
        <dd>Because there is no time-in for that day. The register only knows what was recorded: a time-in, a Work From Home entry or a leave.</dd>
        <dt>Why does a day with nine hours in the office show Short hours?</dt>
        <dd>The breaks and the time adjustments of the Movement Register are taken out. Open the day to see clocked time, breaks, time adjustments and productive time side by side.</dd>
        <dt>Why is a time-out "missing" although the employee timed out the next morning?</dt>
        <dd>A time-out more than {{ forgottenText }} after the time-in is taken as forgotten: nobody works that long, so the hours cannot be trusted.</dd>
        <dt>An employee on the night shift timed in after midnight. Which day is it?</dt>
        <dd>The day before, as long as the time-in is before {{ dayStartsAt }}.</dd>
        <dt>Why does the Late count differ from the cells that look late?</dt>
        <dd>A late arrival is counted even when the cell shows short hours, no time-out or in now; those cells carry the amber corner.</dd>
        <dt>Why does an employee show “—” under Shift?</dt>
        <dd>The designation record has no shift. The register judges them as {{ defaultShift }} until a shift is set on the employee record.</dd>
      </dl>
    </section>

    <section :id="sections[9].id" class="att-guide__section">
      <h3>10. Rules in force</h3>
      <table class="att-guide__table att-guide__table--kv">
        <tbody>
          <tr><th>Full working day</th><td>{{ fullDayText }}</td></tr>
          <tr><th>Half-day leave</th><td>{{ halfDayText }}</td></tr>
          <tr><th>Productive hours</th><td>Time in to time out, minus Break and Time Adjustment minutes</td></tr>
          <tr><th>Hours short</th><td>Required minus productive over the period; longer days offset shorter ones</td></tr>
          <tr><th>Late grace after the shift start</th><td>{{ graceText }}</td></tr>
          <tr><th>Forgotten time-out</th><td>Open or closed {{ forgottenText }} or more after the time-in</td></tr>
          <tr><th>Day starts at</th><td>{{ dayStartsAt }} (earlier time-ins count for the day before)</td></tr>
          <tr><th>Default shift</th><td>{{ defaultShift }}</td></tr>
          <tr><th>Movement Register types</th><td>{{ movementTypes }}</td></tr>
          <tr><th>Who is listed</th><td>Active employees of the site with a department record, within their joining and release dates</td></tr>
          <tr><th>Weekly off</th><td>{{ weeklyOff }}</td></tr>
          <tr><th>Holidays</th><td>Leave schedule of the site</td></tr>
          <tr><th>Leave statuses ignored</th><td>{{ ignoredStatuses }}</td></tr>
          <tr><th>Time zone</th><td>{{ timeZone }}</td></tr>
          <tr><th>Longest period</th><td>{{ maxRangeDays }} days</td></tr>
          <tr><th>Who can open this page</th><td>Administrators, by URL</td></tr>
        </tbody>
      </table>
    </section>
  </app-side-panel>
</template>

<script setup>
// The in-page user guide of the Attendance Register. The numbers and the shift table come from the rules the
// API returned with the register, so the guide can never drift from what the page actually did.
import { computed } from "vue";
import AppSidePanel from "components/common/AppSidePanel.vue";
import { ATTENDANCE_STATES, ATTENDANCE_STATE_ORDER, fmtMinutes, fmtTime12 } from "modules/analytics/utils/registerStates";

const props = defineProps({
  rules: { type: Object, default: null }
});

defineEmits(["close"]);

const sections = [
  { id: "att-guide-purpose", title: "Purpose" },
  { id: "att-guide-period", title: "Period" },
  { id: "att-guide-grid", title: "Grid" },
  { id: "att-guide-rules", title: "How a day is judged" },
  { id: "att-guide-movement", title: "Movement Register" },
  { id: "att-guide-leave", title: "Leaves & holidays" },
  { id: "att-guide-filters", title: "Tiles & filters" },
  { id: "att-guide-details", title: "Details & export" },
  { id: "att-guide-faq", title: "Questions" },
  { id: "att-guide-live", title: "Rules in force" }
];

const legend = ATTENDANCE_STATE_ORDER.map((k) => ATTENDANCE_STATES[k]);

const actions = {
  OnTime: "Nothing.",
  Late: "Check for the “T”: a late day with no time adjustment was not declared.",
  Short: "Open the day: early time-out, long breaks or a late start explain it.",
  NoTimeOut: "Remind the employee to time out; the hours of that day are lost to the count.",
  Absent: "Ask for the missing time-in, the Work From Home entry or the leave application.",
  InNow: "Nothing; the employee is at work.",
  Awaited: "Wait; the shift has not started yet.",
  Wfh: "Remind the employee to time in on work-from-home days too.",
  Leave: "Nothing, unless the leave itself is wrong.",
  Holiday: "Nothing.",
  Future: "Nothing.",
  NotEmployed: "Nothing; check joining or release dates if the blank is unexpected."
};

const fullDayText = computed(() => fmtMinutes(props.rules?.fullDayMinutes ?? 480));
const halfDayText = computed(() => fmtMinutes(props.rules?.halfDayMinutes ?? 240));
const graceText = computed(() => fmtMinutes(props.rules?.lateGraceMinutes ?? 15));
const forgottenText = computed(() => fmtMinutes(props.rules?.forgottenAfterMinutes ?? 960));
const dayStartsAt = computed(() => fmtTime12(props.rules?.dayStartsAt || "05:00"));
const defaultShift = computed(() => props.rules?.defaultShift || "Shift 1");
const weeklyOff = computed(() => props.rules?.weeklyOff || "Sunday");
const timeZone = computed(() => props.rules?.timeZone || "the site time zone");
const maxRangeDays = computed(() => props.rules?.maxRangeDays ?? 93);
const shifts = computed(() => props.rules?.shifts || []);
const ignoredStatuses = computed(() => (props.rules?.leaveStatusesIgnored || ["Declined", "Cancelled"]).join(", "));
const movementTypes = computed(() => (props.rules?.movementTypes || ["Break", "Time Adjustment", "Work From Home"]).join(", "));
const defaultLateAfter = computed(() => fmtTime12(shifts.value.find((s) => s.name === defaultShift.value)?.lateAfter || ""));

function jump (id) {
  document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "start" });
}
</script>

<style scoped>
.att-guide__toc {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 6px;
  margin-bottom: 12px;
}
.att-guide__toc a {
  font-size: 12px;
  padding: 3px 9px;
  border-radius: 12px;
  background: var(--brand-050, #f3f8fb);
  color: var(--q-primary);
  text-decoration: none;
}
.att-guide__toc a:hover {
  background: var(--brand-100, #e3f1f8);
}
.att-guide__section {
  scroll-margin-top: 8px;
  margin-bottom: 20px;
  font-size: 13.5px;
  line-height: 1.5;
  color: var(--ink-900, #16262c);
}
.att-guide__section h3 {
  font-size: 15px;
  font-weight: 600;
  line-height: 1.3;
  margin: 0 0 6px;
  color: var(--q-primary);
}
.att-guide__section p,
.att-guide__section ul,
.att-guide__section ol {
  margin: 0 0 8px;
}
.att-guide__section li {
  margin-bottom: 4px;
}
.att-guide__table {
  width: 100%;
  border-collapse: collapse;
  margin: 6px 0 10px;
  font-size: 12.5px;
}
.att-guide__table th,
.att-guide__table td {
  padding: 6px 8px;
  border: 1px solid var(--line, #dde6e8);
  vertical-align: top;
  text-align: left;
}
.att-guide__table th {
  background: #f7fafb;
  font-weight: 600;
  color: var(--ink-700, #3a4d54);
}
.att-guide__table--kv th {
  width: 40%;
}
.att-guide__cell-col {
  white-space: nowrap;
}
.att-guide__swatch {
  --cell-colour: #b0b4b8;
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 24px;
  height: 20px;
  padding: 0 3px;
  margin-right: 4px;
  border-radius: 5px;
  background: color-mix(in srgb, var(--cell-colour) 13%, #fff);
  color: var(--cell-colour);
  font-size: 11px;
  vertical-align: middle;
}
.att-guide__swatch--plain {
  background: #f1f3f4;
  color: var(--ink-700, #3a4d54);
}
/* The amber corner a cell carries when the arrival was late on a day with another state. */
.att-guide__swatch--flag::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  width: 0;
  height: 0;
  border-top: 9px solid #c98a00;
  border-right: 9px solid transparent;
  border-top-left-radius: 5px;
}
.att-guide__faq dt {
  font-weight: 600;
  margin-top: 8px;
}
.att-guide__faq dd {
  margin: 2px 0 0;
  color: var(--ink-700, #3a4d54);
}
</style>
