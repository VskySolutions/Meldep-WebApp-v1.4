<template>
  <div class="full-width column flex-center q-pa-xl text-grey-6 app-empty-state">
    <q-icon :name="icon" size="40px" class="q-mb-sm" />
    <div class="text-subtitle1 q-mb-xs app-empty-state__title">{{ title }}</div>
    <div v-if="text" :class="['text-body2', { 'q-mb-md': $slots.default }]">{{ text }}</div>
    <slot />
  </div>
</template>

<script setup>
// The empty state of a list or panel: a topic icon, a title, one explanatory line and (optionally) the
// primary call to action in the default slot — the same Add the header offers.
defineProps({
  icon: { type: String, default: "o_inbox" },
  title: { type: String, default: "No Data Available" },
  text: { type: String, default: "" }
});
</script>

<style scoped>
.app-empty-state__title {
  color: var(--ink-700, #3a4d54);
}
</style>
