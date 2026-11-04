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
    <span v-if="cell.preFilled && cell.state !== 'Future'" class="tsr-cell__dot" aria-hidden="true" />
  </button>
</template>

<script setup>
// One employee-day of the register: an icon or glyph for the state, the hours when there is room, "½" under a
// half-day leave. The explanation on hover comes from the grid's shared hover card (one for all cells), so
// the cell itself stays as light as possible.
import { computed } from "vue";
import { stateOf, cellText, cellExtra, isHalfDay } from "modules/analytics/utils/registerStates";

const props = defineProps({
  cell: { type: Object, required: true },
  day: { type: Object, default: null },
  row: { type: Object, default: null },
  compact: { type: Boolean, default: false }
});

defineEmits(["click"]);

const state = computed(() => stateOf(props.cell));
const half = computed(() => isHalfDay(props.cell));
const text = computed(() => cellText(props.cell));
const extra = computed(() => cellExtra(props.cell));
const ariaLabel = computed(() => `${props.row?.fullName || ""} ${props.day?.date || ""}: ${state.value.label}${half.value ? ", half-day leave" : ""}${text.value ? " " + text.value : ""}`.trim());

const classes = computed(() => [
  "tsr-cell",
  `tsr-cell--${state.value.key}`,
  {
    "tsr-cell--compact": props.compact,
    "tsr-cell--half": half.value,
    "tsr-cell--off": ["Holiday", "Future", "NotEmployed"].includes(state.value.key)
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
.tsr-cell__dot {
  position: absolute;
  top: 2px;
  right: 2px;
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: var(--ink-500, #647a82);
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
