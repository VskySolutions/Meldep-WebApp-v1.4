<template>
  <button
    type="button"
    :class="classes"
    :style="{ '--cell-colour': state.colour }"
    :aria-label="ariaLabel"
    :disabled="cell.state === 'NotEmployed'"
    @click="$emit('click')"
  >
    <q-icon v-if="state.icon" :name="state.icon" size="14px" class="tsr-cell__icon" />
    <span v-else-if="state.glyph" class="tsr-cell__glyph">{{ state.glyph }}</span>
    <span v-if="half" class="tsr-cell__half" aria-hidden="true">½</span>
    <span v-if="text && !compact" class="tsr-cell__text">{{ text }}</span>
    <span v-if="extra && !compact" class="tsr-cell__extra">{{ extra }}</span>
    <span v-if="tags.length && !compact" class="tsr-cell__tags" aria-hidden="true">{{ tags.join("") }}</span>
    <span v-if="dot" class="tsr-cell__dot" aria-hidden="true" />
    <span v-if="flag" class="tsr-cell__flag" aria-hidden="true" />
  </button>
</template>

<script setup>
// One employee-day of a register: an icon or glyph for the state, the text when there is room, "½" under a
// half-day leave, and the markers the register's vocabulary asks for (a corner dot, a corner flag, letters).
// The explanation on hover comes from the grid's shared hover card (one for all cells), so the cell itself
// stays as light as possible. What a state looks like and what the text says come from the vocabulary; the
// Timesheet Register's is the default.
import { computed } from "vue";
import { TIMESHEET_VOCAB } from "modules/analytics/utils/registerStates";

const props = defineProps({
  cell: { type: Object, required: true },
  day: { type: Object, default: null },
  row: { type: Object, default: null },
  compact: { type: Boolean, default: false },
  // What the text shows when the vocabulary offers more than one reading ("hours", "inout").
  mode: { type: String, default: "hours" },
  vocab: { type: Object, default: () => TIMESHEET_VOCAB }
});

defineEmits(["click"]);

const state = computed(() => props.vocab.stateOf(props.cell));
const half = computed(() => props.vocab.isHalfDay(props.cell));
const text = computed(() => props.vocab.cellText(props.cell, props.mode));
const extra = computed(() => props.vocab.cellExtra(props.cell));
const tags = computed(() => props.vocab.cellTags(props.cell));
const dot = computed(() => props.vocab.cellDot(props.cell, props.compact));
const flag = computed(() => props.vocab.cellFlag(props.cell));
const ariaLabel = computed(() => `${props.row?.fullName || ""} ${props.day?.date || ""}: ${state.value.label}${half.value ? ", half-day leave" : ""}${text.value ? " " + text.value : ""}`.trim());

const classes = computed(() => [
  "tsr-cell",
  `tsr-cell--${state.value.key}`,
  {
    "tsr-cell--compact": props.compact,
    "tsr-cell--half": half.value,
    "tsr-cell--off": props.vocab.offStates.includes(state.value.key)
  }
]);
</script>

<style scoped>
.tsr-cell {
  --cell-colour: #b0b4b8;
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 3px;
  width: 100%;
  min-height: 28px;
  padding: 0 4px;
  border: 1px solid transparent;
  border-radius: 6px;
  background: color-mix(in srgb, var(--cell-colour) 13%, #fff);
  color: var(--ink-900, #16262c);
  font: inherit;
  font-size: 12px;
  line-height: 1;
  font-variant-numeric: tabular-nums;
  cursor: pointer;
  transition: border-color 0.12s ease, box-shadow 0.12s ease;
}
.tsr-cell--compact {
  min-height: 24px;
  padding: 0 2px;
  gap: 1px;
}
.tsr-cell:hover:not(:disabled) {
  border-color: var(--cell-colour);
  box-shadow: 0 1px 4px rgba(18, 54, 69, 0.12);
}
.tsr-cell:focus-visible {
  outline: 2px solid var(--q-primary);
  outline-offset: 1px;
}
.tsr-cell:disabled {
  cursor: default;
}
.tsr-cell__icon {
  color: var(--cell-colour);
  flex: 0 0 auto;
}
.tsr-cell__glyph {
  color: var(--cell-colour);
  font-weight: 700;
  font-size: 11.5px;
  letter-spacing: 0.02em;
}
.tsr-cell__text {
  font-weight: 500;
}
.tsr-cell__half {
  font-size: 10px;
  font-weight: 700;
  color: var(--ink-700, #3a4d54);
}
.tsr-cell__extra {
  font-size: 10px;
  color: var(--ink-500, #647a82);
}
/* Letters beside the text, in one small chip: what else the day carries (W, B, T on the Attendance Register). */
.tsr-cell__tags {
  padding: 2px 3px;
  border-radius: 3px;
  background: rgba(22, 38, 44, 0.07);
  color: var(--ink-700, #3a4d54);
  font-size: 9px;
  font-weight: 700;
  line-height: 1;
  letter-spacing: 0.06em;
}
.tsr-cell__dot {
  position: absolute;
  top: 2px;
  right: 2px;
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: var(--ink-500, #647a82);
}
/* A small amber corner, top-left: a second fact about the day that its state does not show. */
.tsr-cell__flag {
  position: absolute;
  top: 0;
  left: 0;
  width: 0;
  height: 0;
  border-top: 8px solid #c98a00;
  border-right: 8px solid transparent;
  border-top-left-radius: 5px;
}
/* Non-working days recede: no fill, a hatch for holidays. */
.tsr-cell--off {
  background: transparent;
}
.tsr-cell--Holiday {
  background: repeating-linear-gradient(135deg, #eef1f2 0 4px, #f8f9fa 4px 8px);
}
.tsr-cell--Leave {
  background: color-mix(in srgb, var(--cell-colour) 10%, #fff);
}
.tsr-cell--NotEmployed {
  opacity: 0;
}
</style>
