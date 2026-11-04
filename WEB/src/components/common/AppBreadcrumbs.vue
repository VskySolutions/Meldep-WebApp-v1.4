<template>
  <q-breadcrumbs :class="noMargin ? 'text-grey-7' : 'q-mb-md text-grey-7'" active-color="primary">
    <template #separator>
      <q-icon size="1.2em" name="o_chevron_right" color="grey-6" />
    </template>
    <q-breadcrumbs-el
      v-for="(item, index) in items"
      :key="index"
      :label="item.label"
      :icon="item.icon"
      :to="item.to"
    />
  </q-breadcrumbs>
</template>

<script setup>
// The crumb trail every page starts with: Home → group → page (the last crumb is the page or record).
defineProps({
  // [{ label, icon?, to? }]
  items: { type: Array, default: () => [] },
  noMargin: { type: Boolean, default: false }
});
</script>
