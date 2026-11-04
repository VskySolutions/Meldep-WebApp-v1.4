<template>
  <component
    :is="clickable ? 'button' : 'div'"
    :type="clickable ? 'button' : undefined"
    :class="['app-stat', { 'app-stat--clickable': clickable, 'app-stat--active': active, 'app-stat--dense': dense }]"
    :style="style"
    :aria-pressed="clickable ? String(active) : undefined"
    @click="clickable && $emit('click')"
  >
    <div v-if="icon" class="app-stat__icon">
      <q-icon :name="icon" size="18px" />
    </div>
    <div class="app-stat__body">
      <div class="app-stat__label">{{ label }}</div>
      <div class="app-stat__value">
        <q-skeleton v-if="loading" type="text" width="40px" />
        <template v-else>{{ display }}</template>
      </div>
      <div v-if="caption" class="app-stat__caption">{{ caption }}</div>
    </div>
    <q-icon v-if="active" name="o_check_circle" size="16px" class="app-stat__check" />
    <q-tooltip v-if="hint" :delay="400" max-width="280px">{{ hint }}</q-tooltip>
  </component>
</template>

<script setup>
// A KPI tile: a figure with its name, in a colour, optionally acting as a filter toggle. A strip of these
// above a list is the "at a glance" row every CRM list opens with; clicking one narrows the list and the
// tile stays marked until clicked again.
import { computed } from "vue";

const props = defineProps({
  label: { type: String, required: true },
  value: { type: [Number, String], default: 0 },
  caption: { type: String, default: "" },
  icon: { type: String, default: "" },
  // Any CSS colour: the icon, the stripe and the active tint are derived from it.
  color: { type: String, default: "" },
  active: { type: Boolean, default: false },
  clickable: { type: Boolean, default: false },
  loading: { type: Boolean, default: false },
  dense: { type: Boolean, default: false },
  hint: { type: String, default: "" }
});

defineEmits(["click"]);

const display = computed(() => {
  const v = props.value;
  if (typeof v === "number") return v.toLocaleString();
  return v ?? "—";
});

const style = computed(() => (props.color ? { "--stat-colour": props.color } : {}));
</script>

<style scoped>
.app-stat {
  --stat-colour: var(--q-primary);
  position: relative;
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 132px;
  padding: 10px 12px;
  border: 1px solid var(--line, #dde6e8);
  border-radius: 10px;
  background: #fff;
  text-align: left;
  font: inherit;
  color: inherit;
  transition: box-shadow 0.15s ease, border-color 0.15s ease, background 0.15s ease;
}
.app-stat--dense {
  min-width: 112px;
  padding: 0px 10px;
  gap: 8px;
}
.app-stat::before {
  content: "";
  position: absolute;
  left: 0;
  top: 8px;
  bottom: 8px;
  width: 3px;
  border-radius: 0 3px 3px 0;
  background: var(--stat-colour);
  opacity: 0.9;
}
.app-stat--clickable {
  cursor: pointer;
}
.app-stat--clickable:hover {
  border-color: var(--stat-colour);
  box-shadow: 0 2px 8px rgba(18, 54, 69, 0.1);
}
.app-stat--clickable:focus-visible {
  outline: 2px solid var(--q-primary);
  outline-offset: 2px;
}
.app-stat--active {
  border-color: var(--stat-colour);
  background: color-mix(in srgb, var(--stat-colour) 8%, #fff);
}
.app-stat__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  color: var(--stat-colour);
  background: color-mix(in srgb, var(--stat-colour) 12%, #fff);
  flex: 0 0 auto;
}
.app-stat--dense .app-stat__icon {
  width: 26px;
  height: 26px;
}
.app-stat__body {
  min-width: 0;
  flex: 1 1 auto;
}
.app-stat__label {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--ink-500, #647a82);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.app-stat__value {
  font-size: 20px;
  font-weight: 700;
  line-height: 1.2;
  color: var(--ink-900, #16262c);
  font-variant-numeric: tabular-nums;
}
.app-stat--dense .app-stat__value {
  font-size: 17px;
}
.app-stat__caption {
  font-size: 11px;
  color: var(--ink-500, #647a82);
}
.app-stat__check {
  position: absolute;
  top: 6px;
  right: 6px;
  color: var(--stat-colour);
}
</style>
