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
// the first is a filter toggle: click it to keep only the employees it counts, click again to release. Which
// tiles there are and what they say comes from the register's vocabulary; the Timesheet Register's is the default.
import { computed } from "vue";
import AppStatTile from "components/common/AppStatTile.vue";
import { TIMESHEET_VOCAB } from "modules/analytics/utils/registerStates";

const props = defineProps({
  summary: { type: Object, default: null },
  rules: { type: Object, default: null },
  active: { type: String, default: null },
  loading: { type: Boolean, default: false },
  vocab: { type: Object, default: () => TIMESHEET_VOCAB }
});

defineEmits(["toggle"]);

const tiles = computed(() => props.vocab.tiles(props.summary || {}, props.rules));
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
