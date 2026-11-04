<template>
  <div class="tsr-summary q-mb-md" role="group" aria-label="Summary">
    <app-stat-tile
      v-for="tile in tiles"
      :key="tile.key"
      dense
      :label="tile.label"
      :value="tile.value"
      :caption="tile.caption"
      :icon="tile.icon"
      :color="tile.colour"
      :hint="tile.hint"
      :clickable="tile.clickable"
      :active="active === tile.key"
      :loading="loading"
      @click="$emit('toggle', tile.key)"
    />
  </div>
</template>

<script setup>
// The "at a glance" strip: how many employee-days were expected in the range and how they went. Every tile but
// the first is a filter toggle: click it to keep only the employees it counts, click again to release.
import { computed } from "vue";
import AppStatTile from "components/common/AppStatTile.vue";
import { STATES, fmtHours } from "modules/analytics/utils/registerStates";

const props = defineProps({
  summary: { type: Object, default: null },
  rules: { type: Object, default: null },
  active: { type: String, default: null },
  loading: { type: Boolean, default: false }
});

defineEmits(["toggle"]);

const pct = (n, total) => (total ? `${Math.round((n / total) * 100)}% of expected days` : "");

const tiles = computed(() => {
  const s = props.summary || {};
  const expected = s.expectedDays || 0;
  const defaultShift = props.rules?.defaultShift || "the default shift";

  return [
    {
      key: "expected",
      label: "Expected days",
      value: expected,
      caption: `${s.employees || 0} employees · ${fmtHours(s.loggedHours)} of ${fmtHours(s.expectedHours)} h logged`,
      icon: "o_event_available",
      colour: "#647a82",
      clickable: false,
      hint: "Working days the listed employees were expected to fill (past days and today) once Sundays, holidays and full-day leaves are taken out."
    },
    {
      key: "OnTime",
      label: STATES.OnTime.label,
      value: s.onTime || 0,
      caption: pct(s.onTime || 0, expected),
      icon: STATES.OnTime.icon,
      colour: STATES.OnTime.colour,
      clickable: true,
      hint: STATES.OnTime.description
    },
    {
      key: "Delayed",
      label: STATES.Delayed.label,
      value: s.delayed || 0,
      caption: pct(s.delayed || 0, expected),
      icon: STATES.Delayed.icon,
      colour: STATES.Delayed.colour,
      clickable: true,
      hint: STATES.Delayed.description
    },
    {
      key: "Partial",
      label: STATES.Partial.label,
      value: s.partial || 0,
      caption: pct(s.partial || 0, expected),
      icon: STATES.Partial.icon,
      colour: STATES.Partial.colour,
      clickable: true,
      hint: STATES.Partial.description
    },
    {
      key: "Pending",
      label: STATES.Pending.label,
      value: s.pending || 0,
      caption: pct(s.pending || 0, expected),
      icon: STATES.Pending.icon,
      colour: STATES.Pending.colour,
      clickable: true,
      hint: STATES.Pending.description
    },
    {
      key: "Due",
      label: STATES.Due.label,
      value: s.due || 0,
      caption: "cutoff not passed yet",
      icon: STATES.Due.icon,
      colour: STATES.Due.colour,
      clickable: true,
      hint: STATES.Due.description
    },
    {
      key: "Leave",
      label: "On leave",
      value: s.leaveDays || 0,
      caption: s.halfDays ? `full days · ${s.halfDays} half days besides` : "full days",
      icon: "o_beach_access",
      colour: STATES.Leave.colour,
      clickable: true,
      hint: "Employee-days covered by a full-day leave (applied, forwarded or approved). Half-day leaves keep the day expected at 4 hours."
    },
    {
      key: "noShift",
      label: "No shift",
      value: s.employeesWithoutShift || 0,
      caption: `employees, judged as ${defaultShift}`,
      icon: "o_person_off",
      colour: "#898781",
      clickable: true,
      hint: "Employees whose designation record has no shift. Their cutoff uses the default shift until a shift is assigned on the employee record."
    }
  ];
});
</script>

<style scoped>
.tsr-summary {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.tsr-summary > * {
  flex: 1 1 150px;
}
</style>
