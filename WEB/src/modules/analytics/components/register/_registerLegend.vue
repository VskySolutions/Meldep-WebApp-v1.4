<template>
  <div class="tsr-legend">
    <span class="tsr-legend__title">Legend</span>
    <span
      v-for="item in items"
      :key="item.key"
      class="tsr-legend__item"
      :style="{ '--cell-colour': item.colour }"
    >
      <span :class="['tsr-legend__swatch', { 'tsr-legend__swatch--plain': item.plain }]">
        <q-icon v-if="item.icon" :name="item.icon" size="13px" />
        <span v-else-if="item.dot" class="tsr-legend__dot" />
        <small v-else-if="item.small">{{ item.glyph }}</small>
        <b v-else>{{ item.glyph }}</b>
      </span>
      {{ item.label }}
      <q-tooltip :delay="150" max-width="300px" anchor="top middle" self="bottom middle">
        <div class="tsr-tip__title">{{ item.full }}</div>
        <div class="tsr-tip__text">{{ item.description }}</div>
      </q-tooltip>
    </span>
  </div>
</template>

<script setup>
// The key to the cells, in the order of the state list, each with its icon or glyph so the grid is never read
// by colour alone. Hover an item for its full form and the one-line definition.
import { STATES, STATE_ORDER } from "modules/analytics/utils/registerStates";

const stateItems = STATE_ORDER
  .filter((k) => k !== "NotEmployed")
  .map((k) => ({ key: k, label: STATES[k].label, full: STATES[k].label, icon: STATES[k].icon, glyph: STATES[k].glyph, colour: STATES[k].colour, description: STATES[k].description }));

const items = [
  ...stateItems,
  {
    key: "half",
    label: "Half-day leave",
    full: "½ · Half-day leave",
    glyph: "½",
    plain: true,
    description: "A half-day leave covers the day, so 4 hours are expected instead of 8. The cell is still judged on time, delayed, partial or pending against those 4 hours."
  },
  {
    key: "prefilled",
    label: "Pre-filled",
    full: "Pre-filled",
    dot: true,
    plain: true,
    description: "The hours were entered before the day had started. Counts as on time, but worth a look."
  },
  {
    key: "offday",
    label: "Hours on an off day",
    full: "+8 · Hours on an off day",
    glyph: "+8",
    small: true,
    plain: true,
    description: "Hours were logged on a holiday, a leave day or a future day. The day keeps its H, L or future state; the hint shows how many hours."
  }
];
</script>

<style scoped>
.tsr-legend {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px 14px;
  font-size: 12px;
  color: var(--ink-700, #3a4d54);
}
.tsr-legend__title {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--ink-500, #647a82);
}
.tsr-legend__item {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  cursor: help;
}
.tsr-legend__swatch {
  --cell-colour: #b0b4b8;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 18px;
  border-radius: 5px;
  background: color-mix(in srgb, var(--cell-colour) 13%, #fff);
  color: var(--cell-colour);
  font-size: 11px;
}
.tsr-legend__swatch--plain {
  background: #f1f3f4;
  color: var(--ink-700, #3a4d54);
}
.tsr-legend__dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: var(--ink-500, #647a82);
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
</style>
