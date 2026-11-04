<template>
  <div class="app-segmented" role="group" :aria-label="ariaLabel">
    <button
      v-for="option in options"
      :key="option.value"
      type="button"
      :class="['app-segmented__item', { 'app-segmented__item--on': option.value === modelValue }]"
      :aria-pressed="String(option.value === modelValue)"
      :disabled="option.disable"
      @click="pick(option.value)"
    >
      <q-icon v-if="option.icon" :name="option.icon" size="18px" />
      <span v-if="option.label && !iconOnly" class="app-segmented__label">{{ option.label }}</span>
      <q-badge v-if="option.count != null" :color="option.value === modelValue ? 'white' : 'grey-3'" :text-color="option.value === modelValue ? 'primary' : 'grey-8'" class="app-segmented__count">
        {{ option.count }}
      </q-badge>
      <q-tooltip v-if="option.tooltip || (iconOnly && option.label)" :delay="400">{{ option.tooltip || option.label }}</q-tooltip>
    </button>
  </div>
</template>

<script setup>
// A segmented control: one of a few readings of the same thing (Table / Board, Comfortable / Compact),
// drawn as joined white buttons with the chosen one filled primary.
defineProps({
  modelValue: { type: [String, Number], default: null },
  // [{ value, label?, icon?, tooltip?, count?, disable? }]
  options: { type: Array, default: () => [] },
  iconOnly: { type: Boolean, default: false },
  ariaLabel: { type: String, default: "View" }
});

const emit = defineEmits(["update:modelValue"]);

const pick = (value) => emit("update:modelValue", value);
</script>

<style scoped>
.app-segmented {
  display: inline-flex;
  align-items: stretch;
  border: 1px solid var(--line, #dde6e8);
  border-radius: 8px;
  background: #fff;
  overflow: hidden;
}
.app-segmented__item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-height: 34px;
  padding: 0 12px;
  border: none;
  border-right: 1px solid var(--line, #dde6e8);
  background: transparent;
  color: var(--ink-700, #3a4d54);
  font: inherit;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.15s ease, color 0.15s ease;
}
.app-segmented__item:last-child {
  border-right: none;
}
.app-segmented__item:hover:not(:disabled) {
  background: var(--brand-050, #f3f8fb);
}
.app-segmented__item:disabled {
  color: var(--ink-300, #a6b8bd);
  cursor: not-allowed;
}
.app-segmented__item:focus-visible {
  outline: 2px solid var(--q-primary);
  outline-offset: -2px;
}
.app-segmented__item--on {
  background: var(--q-primary);
  color: #fff;
}
.app-segmented__item--on:hover:not(:disabled) {
  background: var(--q-primary);
}
.app-segmented__count {
  font-size: 10.5px;
  padding: 1px 6px;
}
</style>
