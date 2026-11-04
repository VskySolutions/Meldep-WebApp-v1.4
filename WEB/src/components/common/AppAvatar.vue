<template>
  <span :class="['app-avatar', { 'app-avatar--with-name': showName }]">
    <span class="app-avatar__disc" :style="discStyle" :aria-label="name || 'Unassigned'">
      <q-icon v-if="!name" name="o_person_outline" :size="iconSize" />
      <template v-else>{{ initials }}</template>
    </span>
    <span v-if="showName" class="app-avatar__name ellipsis">{{ name || empty }}</span>
    <q-tooltip v-if="!showName && (name || empty)" :delay="300">{{ name || empty }}</q-tooltip>
  </span>
</template>

<script setup>
// A person as a small initials disc, coloured by hashing the name so the same person is always the same
// colour, with the full name beside it or on a tooltip. An empty name draws the "unassigned" silhouette.
import { computed } from "vue";

const props = defineProps({
  name: { type: String, default: "" },
  size: { type: Number, default: 28 },
  showName: { type: Boolean, default: false },
  empty: { type: String, default: "Unassigned" }
});

// Ten hues with enough contrast for white initials.
const PALETTE = ["#1b75ab", "#26a69a", "#7e57c2", "#ef6c00", "#c2185b", "#00897b", "#5c6bc0", "#43a047", "#8d6e63", "#546e7a"];

const initials = computed(() => {
  const parts = String(props.name || "").trim().split(/\s+/).filter(Boolean);
  if (!parts.length) return "";
  const first = parts[0][0] || "";
  const last = parts.length > 1 ? parts[parts.length - 1][0] : "";
  return (first + last).toUpperCase();
});

const colour = computed(() => {
  const text = String(props.name || "");
  if (!text) return "#b0bec5";
  let hash = 0;
  for (let i = 0; i < text.length; i += 1) hash = (hash * 31 + text.charCodeAt(i)) >>> 0;
  return PALETTE[hash % PALETTE.length];
});

const discStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${props.size}px`,
  fontSize: `${Math.max(10, Math.round(props.size * 0.4))}px`,
  backgroundColor: colour.value
}));

const iconSize = computed(() => `${Math.round(props.size * 0.6)}px`);
</script>

<style scoped>
.app-avatar {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  max-width: 100%;
  vertical-align: middle;
}
.app-avatar__disc {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border-radius: 50%;
  color: #fff;
  font-weight: 600;
  letter-spacing: 0.02em;
  user-select: none;
}
.app-avatar__name {
  min-width: 0;
  font-size: 13px;
  color: var(--ink-900, #16262c);
}
</style>
