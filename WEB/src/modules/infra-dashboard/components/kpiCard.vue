<template>
  <q-card flat bordered class="kpi-card" :style="`border-left: 5px solid ${accent};`">
    <q-card-section class="q-py-md">
      <div class="row items-center no-wrap">
        <div class="col">
          <div class="text-caption text-grey-7 text-uppercase kpi-label">{{ label }}</div>
          <div class="text-h5 text-weight-bold kpi-value" :style="`color:${accent};`">{{ value }}</div>
          <div v-if="sub" class="text-caption text-grey-6 q-mt-xs">{{ sub }}</div>
        </div>
        <div v-if="icon" class="col-auto">
          <q-avatar :style="`background:${accent}1a;`" size="46px">
            <q-icon :name="icon" :style="`color:${accent};`" size="26px" />
          </q-avatar>
        </div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
defineProps({
  label: { type: String, default: "" },
  value: { type: [String, Number], default: "" },
  sub: { type: String, default: "" },
  icon: { type: String, default: "" },
  accent: { type: String, default: "#1b75ab" }
});
</script>

<style scoped>
.kpi-card {
  border-radius: 10px;
  height: 100%;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.06);
}
.kpi-label {
  letter-spacing: 0.5px;
}
.kpi-value {
  line-height: 1.25;
  word-break: break-word;
}
</style>
