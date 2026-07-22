<template>
  <q-card flat bordered class="chart-panel">
    <q-card-section class="chart-panel__header">
      <div class="row items-center no-wrap">
        <div class="col">
          <div class="text-subtitle1 text-weight-bold text-primary">{{ title }}</div>
          <div v-if="subtitle" class="text-caption text-grey-7">{{ subtitle }}</div>
        </div>
        <div class="col-auto">
          <slot name="action" />
        </div>
      </div>
    </q-card-section>
    <q-separator />
    <q-card-section class="chart-panel__body">
      <q-inner-loading :showing="loading">
        <q-spinner-ios size="32px" color="primary" />
      </q-inner-loading>

      <div v-if="!loading && !hasData" class="column flex-center text-grey-6 chart-empty">
        <q-icon name="o_bar_chart" size="34px" class="q-mb-sm" />
        <div class="text-caption">No data for the selected filters</div>
      </div>

      <apexchart
        v-else-if="!loading"
        :type="type"
        :height="height"
        :series="apexSeries"
        :options="apexOptions"
      />
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  title: { type: String, default: "" },
  subtitle: { type: String, default: "" },
  type: { type: String, default: "donut" }, // donut | pie | bar | line | area
  series: { type: Array, default: () => [] }, // number[]
  labels: { type: Array, default: () => [] }, // string[]
  seriesName: { type: String, default: "Amount" },
  height: { type: [Number, String], default: 300 },
  loading: { type: Boolean, default: false },
  horizontal: { type: Boolean, default: false },
  currency: { type: Boolean, default: true },
  monthlyFromYearly: { type: Boolean, default: false },
  colors: { type: Array, default: null }
});

// Categorical palette anchored on the house primary blue (#1b75ab).
const PALETTE = [
  "#1b75ab", "#4a9fd4", "#5cb85c", "#f0ad4e", "#d9534f", "#8e6bb0",
  "#20b2aa", "#e6842e", "#6c8ebf", "#17a2b8", "#9467bd", "#adb5bd"
];

const isCircular = computed(() => props.type === "donut" || props.type === "pie");
const isDistributedBar = computed(() => props.type === "bar" && props.horizontal);

const hasData = computed(() => {
  if (!props.series || props.series.length === 0) return false;
  const total = props.series.reduce((a, b) => a + (Number(b) || 0), 0);
  return total !== 0;
});

function fmtCurrency (val) {
  const n = Number(val) || 0;
  return "$" + n.toLocaleString("en-US", { maximumFractionDigits: 0 });
}

const apexSeries = computed(() => {
  const nums = (props.series || []).map((v) => Number(v) || 0);
  if (isCircular.value) return nums;
  return [{ name: props.seriesName, data: nums }];
});

const apexOptions = computed(() => {
  const colors = props.colors || PALETTE;
  const valueFormatter = props.currency ? fmtCurrency : (v) => v;

  const base = {
    chart: { toolbar: { show: false }, fontFamily: "inherit" },
    colors,
    labels: props.labels,
    legend: { position: "bottom", fontSize: "12px", itemMargin: { horizontal: 8, vertical: 2 } }
  };

  if (isCircular.value) {
    return {
      ...base,
      plotOptions: { pie: { donut: { size: "62%" }, expandOnClick: false } },
      stroke: { width: 1, colors: ["#fff"] },
      dataLabels: {
        enabled: true,
        formatter: (val) => `${Number(val).toFixed(1)}%`,
        style: { fontSize: "11px", fontWeight: 600 },
        dropShadow: { enabled: false }
      },
      tooltip: {
        y: {
          formatter: (val) => {
            const main = valueFormatter(val);
            return props.monthlyFromYearly
              ? `${main} · ${fmtCurrency((Number(val) || 0) / 12)}/mo`
              : main;
          }
        }
      }
    };
  }

  // bar / line / area
  return {
    ...base,
    legend: { show: !isDistributedBar.value, position: "bottom", fontSize: "12px" },
    plotOptions: {
      bar: {
        horizontal: props.horizontal,
        borderRadius: 4,
        columnWidth: "55%",
        distributed: isDistributedBar.value
      }
    },
    dataLabels: {
      enabled: isDistributedBar.value,
      formatter: valueFormatter,
      style: { fontSize: "10px", colors: ["#334155"] },
      offsetX: isDistributedBar.value ? 24 : 0
    },
    xaxis: {
      categories: props.labels,
      labels: {
        formatter: props.horizontal ? valueFormatter : undefined,
        style: { fontSize: "11px" },
        rotate: props.horizontal ? 0 : -35,
        trim: true,
        hideOverlappingLabels: true,
        maxHeight: 100
      }
    },
    yaxis: { labels: { formatter: props.horizontal ? undefined : valueFormatter, style: { fontSize: "11px" } } },
    stroke: props.type === "line" ? { curve: "smooth", width: 3 } : { width: 1, colors: isDistributedBar.value ? undefined : ["transparent"] },
    fill: props.type === "area"
      ? { type: "gradient", gradient: { opacityFrom: 0.4, opacityTo: 0.05 } }
      : { opacity: 1 },
    markers: props.type === "line" ? { size: 3, hover: { size: 5 } } : { size: 0 },
    grid: { borderColor: "#eef1f4", strokeDashArray: 4 },
    tooltip: { shared: false, y: { formatter: valueFormatter } }
  };
});
</script>

<style scoped>
.chart-panel {
  border-radius: 10px;
  height: 100%;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.06);
}
.chart-panel__header {
  padding: 12px 16px;
}
.chart-panel__body {
  position: relative;
  min-height: 220px;
}
.chart-empty {
  position: absolute;
  inset: 0;
}
</style>
