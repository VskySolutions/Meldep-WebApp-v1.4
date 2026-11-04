<template>
  <q-icon :name="icon" :size="size" :color="color" class="app-info-tip">
    <q-tooltip :anchor="anchor" :self="self" :max-width="maxWidth" class="text-body2">
      <slot>{{ text }}</slot>
    </q-tooltip>
  </q-icon>
</template>

<script setup>
// The standard place for a note that explains rather than instructs: an info icon beside a heading, with
// the note on its tooltip.
defineProps({
  text: { type: String, default: "" },
  icon: { type: String, default: "o_info" },
  size: { type: String, default: "16px" },
  color: { type: String, default: "grey-6" },
  maxWidth: { type: String, default: "320px" },
  anchor: { type: String, default: "top middle" },
  self: { type: String, default: "bottom middle" }
});
</script>

<style scoped>
.app-info-tip {
  margin-left: 4px;
  cursor: help;
  vertical-align: text-bottom;
}
</style>
