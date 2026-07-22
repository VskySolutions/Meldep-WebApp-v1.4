import { boot } from "quasar/wrappers";
import VueApexCharts from "vue3-apexcharts";

// Registers the global <apexchart> component used by the Infrastructure Dashboard charts.
export default boot(({ app }) => {
  app.use(VueApexCharts);
});
