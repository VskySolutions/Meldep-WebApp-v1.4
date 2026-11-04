<template>
  <app-side-panel
    title="How to use the Timesheet Register"
    subtitle="A day-by-day attendance view of timesheet discipline"
    width="52vw"
    min-width="680px"
    hide-save
    cancel-label="Close"
    @close="$emit('close')"
  >
    <nav class="tsr-guide__toc">
      <a v-for="s in sections" :key="s.id" :href="'#' + s.id" @click.prevent="jump(s.id)">{{ s.title }}</a>
    </nav>

    <section :id="sections[0].id" class="tsr-guide__section">
      <h3>1. What this page is for</h3>
      <p>
        The register answers one question for every active employee and every day of a period: <b>was the
        day's timesheet filled, and was it filled on time?</b> Rows are employees, columns are days, and each
        cell is the verdict for that employee on that day. It replaces checking weekly timesheets one person
        at a time.
      </p>
      <p>
        It lists the site's <b>active employees that have a department record</b>. Client contacts are stored
        as employees too but belong to no department, so they never appear here. Days before an employee's joining
        date or after the release date stay blank.
      </p>
      <p>
        It is read-only. Fixing a day still happens in the employee's weekly timesheet; fixing a shift or a
        leave happens on the employee and leave records.
      </p>
    </section>

    <section :id="sections[1].id" class="tsr-guide__section">
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

    <section :id="sections[2].id" class="tsr-guide__section">
      <h3>3. Read the grid</h3>
      <p>
        Hover any cell for a plain-language explanation; click it to open the day with the timesheet lines
        behind it. Every short form on the page has a tooltip with its full form: the column heads
        <b>Hrs</b> (logged hours), <b>OT</b> (on time), <b>Dly</b> (delayed), <b>Par</b> (partial) and
        <b>Pnd</b> (pending), the shift chips <b>S1</b>, <b>S2</b>, <b>S3</b>, and the cell glyphs
        <b>H</b>, <b>L</b> and <b>½</b>. The <b>Shift</b> column shows the shift the employee is judged by (S1, S2, S3); “—” means
        none is assigned and the default applies. The four columns on the right total the period for the
        employee: logged hours, on-time days, delayed days, partial days and pending days. The last row counts,
        per day, how many employees completed the day out of those expected to fill it.
      </p>
      <table class="tsr-guide__table">
        <thead>
          <tr><th>Cell</th><th>Meaning</th><th>What to do</th></tr>
        </thead>
        <tbody>
          <tr v-for="s in legend" :key="s.key">
            <td class="tsr-guide__cell-col">
              <span class="tsr-guide__swatch" :style="{ '--cell-colour': s.colour }">
                <q-icon v-if="s.icon" :name="s.icon" size="14px" />
                <b v-else>{{ s.glyph }}</b>
              </span>
              {{ s.label }}
            </td>
            <td>{{ s.description }}</td>
            <td>{{ actions[s.key] }}</td>
          </tr>
          <tr>
            <td class="tsr-guide__cell-col"><span class="tsr-guide__swatch tsr-guide__swatch--plain"><b>½</b></span> Half day</td>
            <td>A half-day leave covers the day, so {{ halfDayHours }} hours are expected instead of {{ fullDayHours }}. The cell still shows on time / delayed / partial / pending against those {{ halfDayHours }} hours.</td>
            <td>Nothing; the leave is already accounted for.</td>
          </tr>
          <tr>
            <td class="tsr-guide__cell-col"><span class="tsr-guide__swatch tsr-guide__swatch--plain"><span class="tsr-guide__dot" /></span> Pre-filled</td>
            <td>The hours were entered before the day had started.</td>
            <td>Counts as on time, but worth a look: hours logged in advance are rarely real.</td>
          </tr>
          <tr>
            <td class="tsr-guide__cell-col"><span class="tsr-guide__swatch tsr-guide__swatch--plain"><small>+8</small></span> Off-day hours</td>
            <td>Hours were logged on a holiday, a leave day or a future day. The day stays H, L or future.</td>
            <td>Check whether the leave or holiday is right, or whether the hours belong to another day.</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section :id="sections[3].id" class="tsr-guide__section">
      <h3>4. How a day is judged</h3>
      <ol>
        <li><b>Is it a working day for this employee?</b> Sundays and the site's leave-schedule dates (public holidays, Saturdays off) are <b>H</b>. Days before joining or after release are blank. A full-day leave is <b>L</b>. None of these expect hours.</li>
        <li><b>How many hours are expected?</b> {{ fullDayHours }} on a working day, {{ halfDayHours }} under a half-day leave.</li>
        <li><b>What is the cutoff?</b> The employee's shift end on that date plus {{ graceText }} of grace. The shift is read from the employee's designation record; without one, {{ defaultShift }} is used and the cell says so.</li>
        <li><b>When were the hours complete?</b> Timesheet lines are taken in the order they were entered; the moment the running total reaches the expected hours is the completion time.</li>
        <li><b>Verdict.</b> Complete by the cutoff → <b>On time</b>. Complete after it → <b>Delayed</b>, with how late. Not complete and the cutoff not passed → <b>Due</b>. Cutoff passed with some hours → <b>Partial</b>; with none → <b>Pending</b>.</li>
      </ol>
      <table class="tsr-guide__table">
        <thead>
          <tr><th>Shift</th><th>Working window</th><th>Cutoff for that day's timesheet</th></tr>
        </thead>
        <tbody>
          <tr v-for="s in shifts" :key="s.name">
            <td>{{ s.name }}</td>
            <td>{{ s.start }} – {{ s.end }}{{ s.endsNextDay ? " (next day)" : "" }}</td>
            <td>{{ s.cutoff }}</td>
          </tr>
          <tr>
            <td>Not assigned</td>
            <td>judged as {{ defaultShift }}</td>
            <td>{{ defaultCutoff }}</td>
          </tr>
        </tbody>
      </table>
      <p class="text-caption text-grey-7">
        Entry times are converted to the site time zone ({{ timeZone }}) before they are compared with the cutoff.
      </p>
    </section>

    <section :id="sections[4].id" class="tsr-guide__section">
      <h3>5. Leaves and holidays</h3>
      <ul>
        <li>A leave counts <b>from the moment it is applied</b>: Applied, Sent to Approver and Approved all cover the day. Only {{ ignoredStatuses }} leaves are ignored. The tooltip shows the status, so an unapproved leave is easy to spot.</li>
        <li>A <b>half-day</b> leave keeps the day expected at {{ halfDayHours }} hours and marks the cell with ½ (1st or 2nd half in the tooltip).</li>
        <li><b>Every Sunday</b> is a weekly off. <b>Saturdays</b> are working days unless the leave schedule lists them off.</li>
        <li>Holidays beat leaves, and leaves beat timesheets: hours logged on such a day appear as a small “+hours” hint without changing the cell.</li>
      </ul>
    </section>

    <section :id="sections[5].id" class="tsr-guide__section">
      <h3>6. Tiles, filters and search</h3>
      <ul>
        <li>The <b>tiles</b> count employee-days in the period for the employees currently listed. Click a tile to keep only the employees it counts (for example everyone with a pending day); click it again to release. <b>No shift</b> lists the employees whose record has no shift.</li>
        <li><b>Filters</b> (top right) narrow the rows by department, employee, shift or day state, and can hide the Sunday and holiday columns. Active filters show as chips above the grid.</li>
        <li><b>Search</b> matches the employee's name, code or department.</li>
        <li><b>Density</b>: Hours (the default) shows the logged hours in each cell; Compact shows only the icon, which suits a whole month. Auto picks by the length of the period.</li>
      </ul>
    </section>

    <section :id="sections[6].id" class="tsr-guide__section">
      <h3>7. Day details and export</h3>
      <ul>
        <li>Click a cell to open the <b>day panel</b>: the verdict in words, the facts it rests on (expected, logged, shift, cutoff, when the hours were complete, how late) and every timesheet line with its project, task, hours and the time it was entered.</li>
        <li>Click an <b>employee's name</b> to open their employee record, the same view the employee list offers.</li>
        <li><b>Export CSV</b> downloads the grid as shown: one row per employee, one column per visible day with the state and hours, and the period totals.</li>
      </ul>
    </section>

    <section :id="sections[7].id" class="tsr-guide__section">
      <h3>8. Common questions</h3>
      <dl class="tsr-guide__faq">
        <dt>The hours are there, so why is the day Delayed?</dt>
        <dd>Because they were entered after the cutoff. The register judges when the hours were entered, not only whether they exist. Open the day to see the completion time against the cutoff.</dd>
        <dt>Why is a Saturday H for some weeks and a working day for others?</dt>
        <dd>Saturdays follow the site's leave schedule. Only the Saturdays listed there are off.</dd>
        <dt>The employee works nights; why does today still say Due at 2 AM?</dt>
        <dd>A Shift 3 day ends at 03:00 the next morning and gets {{ graceText }} of grace after that, so its cutoff is {{ shift3Cutoff }}. Until then the day is due, not pending.</dd>
        <dt>Why does an employee show “—” under Shift?</dt>
        <dd>The designation record has no shift. The register judges them as {{ defaultShift }} until a shift is set on the employee record.</dd>
        <dt>A leave was applied but not approved yet. Does the day count?</dt>
        <dd>Yes. Applied and forwarded leaves cover the day like approved ones; the tooltip and the day panel show the status.</dd>
        <dt>Why does the Pending count differ from the footer?</dt>
        <dd>The tiles count the employees currently listed after search and filters; the footer counts the rows on screen, which also honours the tile you clicked.</dd>
      </dl>
    </section>

    <section :id="sections[8].id" class="tsr-guide__section">
      <h3>9. Rules in force</h3>
      <table class="tsr-guide__table tsr-guide__table--kv">
        <tbody>
          <tr><th>Full working day</th><td>{{ fullDayHours }} hours</td></tr>
          <tr><th>Half-day leave</th><td>{{ halfDayHours }} hours</td></tr>
          <tr><th>Grace after shift end</th><td>{{ graceText }}</td></tr>
          <tr><th>Default shift</th><td>{{ defaultShift }}</td></tr>
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
// The in-page user guide. The numbers and the shift table come from the rules the API returned with the
// register, so the guide can never drift from what the page actually did.
import { computed } from "vue";
import AppSidePanel from "components/common/AppSidePanel.vue";
import { STATES, STATE_ORDER } from "modules/analytics/utils/registerStates";

const props = defineProps({
  rules: { type: Object, default: null }
});

defineEmits(["close"]);

const sections = [
  { id: "tsr-guide-purpose", title: "Purpose" },
  { id: "tsr-guide-period", title: "Period" },
  { id: "tsr-guide-grid", title: "Grid" },
  { id: "tsr-guide-rules", title: "How a day is judged" },
  { id: "tsr-guide-leave", title: "Leaves & holidays" },
  { id: "tsr-guide-filters", title: "Tiles & filters" },
  { id: "tsr-guide-details", title: "Details & export" },
  { id: "tsr-guide-faq", title: "Questions" },
  { id: "tsr-guide-live", title: "Rules in force" }
];

const legend = STATE_ORDER.map((k) => STATES[k]);

const actions = {
  OnTime: "Nothing.",
  Delayed: "Remind the employee to log the day before the cutoff; the tooltip shows how late it was.",
  Partial: "Ask the employee to complete the day's hours.",
  Pending: "Ask the employee to fill the day; it is overdue.",
  Due: "Wait; the employee still has time.",
  Leave: "Nothing, unless the leave itself is wrong.",
  Holiday: "Nothing.",
  Future: "Nothing.",
  NotEmployed: "Nothing; check joining or release dates if the blank is unexpected."
};

const fullDayHours = computed(() => props.rules?.fullDayHours ?? 8);
const halfDayHours = computed(() => props.rules?.halfDayHours ?? 4);
const defaultShift = computed(() => props.rules?.defaultShift || "Shift 1");
const weeklyOff = computed(() => props.rules?.weeklyOff || "Sunday");
const timeZone = computed(() => props.rules?.timeZone || "the site time zone");
const maxRangeDays = computed(() => props.rules?.maxRangeDays ?? 93);
const shifts = computed(() => props.rules?.shifts || []);
const ignoredStatuses = computed(() => (props.rules?.leaveStatusesIgnored || ["Declined", "Cancelled"]).join(", "));

const graceText = computed(() => {
  const minutes = props.rules?.graceMinutes ?? 240;
  if (minutes % 60 === 0) return `${minutes / 60} ${minutes === 60 ? "hour" : "hours"}`;
  return `${minutes} minutes`;
});

const defaultCutoff = computed(() => shifts.value.find((s) => s.name === defaultShift.value)?.cutoff || "");
const shift3Cutoff = computed(() => shifts.value.find((s) => /3/.test(s.name))?.cutoff || "07:00 the next day");

function jump (id) {
  document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "start" });
}
</script>

<style scoped>
.tsr-guide__toc {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 6px;
  margin-bottom: 12px;
}
.tsr-guide__toc a {
  font-size: 12px;
  padding: 3px 9px;
  border-radius: 12px;
  background: var(--brand-050, #f3f8fb);
  color: var(--q-primary);
  text-decoration: none;
}
.tsr-guide__toc a:hover {
  background: var(--brand-100, #e3f1f8);
}
.tsr-guide__section {
  scroll-margin-top: 8px;
  margin-bottom: 20px;
  font-size: 13.5px;
  line-height: 1.5;
  color: var(--ink-900, #16262c);
}
.tsr-guide__section h3 {
  font-size: 15px;
  font-weight: 600;
  line-height: 1.3;
  margin: 0 0 6px;
  color: var(--q-primary);
}
.tsr-guide__section p,
.tsr-guide__section ul,
.tsr-guide__section ol {
  margin: 0 0 8px;
}
.tsr-guide__section li {
  margin-bottom: 4px;
}
.tsr-guide__table {
  width: 100%;
  border-collapse: collapse;
  margin: 6px 0 10px;
  font-size: 12.5px;
}
.tsr-guide__table th,
.tsr-guide__table td {
  padding: 6px 8px;
  border: 1px solid var(--line, #dde6e8);
  vertical-align: top;
  text-align: left;
}
.tsr-guide__table th {
  background: #f7fafb;
  font-weight: 600;
  color: var(--ink-700, #3a4d54);
}
.tsr-guide__table--kv th {
  width: 40%;
}
.tsr-guide__cell-col {
  white-space: nowrap;
}
.tsr-guide__swatch {
  --cell-colour: #b0b4b8;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 20px;
  margin-right: 4px;
  border-radius: 5px;
  background: color-mix(in srgb, var(--cell-colour) 13%, #fff);
  color: var(--cell-colour);
  font-size: 11px;
  vertical-align: middle;
}
.tsr-guide__swatch--plain {
  background: #f1f3f4;
  color: var(--ink-700, #3a4d54);
}
.tsr-guide__dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: var(--ink-500, #647a82);
}
.tsr-guide__faq dt {
  font-weight: 600;
  margin-top: 8px;
}
.tsr-guide__faq dd {
  margin: 2px 0 0;
  color: var(--ink-700, #3a4d54);
}
</style>
