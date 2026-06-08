<!-- Manish Dhuri -->
<template>
  <q-page padding class="reports">
    <q-card>
      <q-card-section class="card-header with-tools">
        <!-- Container for embedding the report -->
        <table class="Custom-Table" style="width: 100%;">
          <thead>
            <th>Report</th>
          </thead>
          <tbody>
            <td>
              <div ref="reportContainer" id="reportContainers" style="width: 100%; height: 80vh;"></div>
            </td>
          </tbody>
        </table>
      </q-card-section>
      <q-separator />
    </q-card>
  </q-page>
</template>

<style>
.reports iframe{
  background: transparent !important;
  border: none;
  overflow: hidden;
}
.active {
  background-color: #0090e769; /* Example active color */
}
</style>
<script setup>
import { useRoute } from "vue-router";
import { ref, onMounted, watch } from "vue";
import reportService from "modules/reports/reports.service";
import * as powerbi from "powerbi-client"; // Import Power BI client
import { useQuasar, QSpinnerFacebook } from "quasar"; // Import Quasar Loading Plugin
const $q = useQuasar();

const route = useRoute();

// Reference to the report container
const reportContainer = ref(null);
// const AllReports = ref([]);

const currentReportId = ref(null);

// Fetch all reports
// async function getAllReports () {
//   const resp = await reportService.getAllReport();
//   AllReports.value = resp.reportModelList;
//   console.log(AllReports.value);
// }

// Fetch and embed the report
async function embedReport (reportId) {
  try {
    // Show the loading spinner
    $q.loading.show({
      spinner: QSpinnerFacebook,
      spinnerColor: "yellow",
      message: "Please Wait..."
    });
    currentReportId.value = reportId;
    const powerbiService = new powerbi.service.Service(
      powerbi.factories.hpmFactory,
      powerbi.factories.wpmpFactory,
      powerbi.factories.routerFactory
    );

    // Reset the container to avoid duplication issues
    if (reportContainer.value) {
      powerbiService.reset(reportContainer.value);
    }

    // Fetch report details from the API
    const resp = await reportService.getReport(reportId);

    // Extract necessary details
    const accessToken = resp.embedToken.token;
    const embedUrl = resp.embedReports[0].embedUrl;
    const embedReportId = resp.embedReports[0].reportId;

    // Define configuration for embedding
    const config = {
      type: "report",
      tokenType: powerbi.models.TokenType.Embed,
      accessToken: accessToken,
      embedUrl: embedUrl,
      id: embedReportId,
      permissions: powerbi.models.Permissions.All,
      settings: {
        filterPaneEnabled: true,
        navContentPaneEnabled: true
      }
    };

    // Embed the report into the container
    const report = powerbiService.embed(reportContainer.value, config);
    $q.loading.hide();
    // Add event listeners to handle loading states
    report.on("loaded", () => {
      $q.loading.hide(); // Hide loading when the report is loaded
    });

    report.on("error", (event) => {
      console.error("Error embedding report:", event.detail);
      $q.loading.hide(); // Hide loading if an error occurs
    });
  } catch (error) {
    console.error("Failed to embed the report:", error);
    $q.loading.hide(); // Ensure loading spinner is hidden in case of failure
  }
}

onMounted(() => {
  // getAllReports();
  // embedReport("2983633b-2d34-4f15-9c9c-e2cae1c48ad0");
});

watch(() => route.params.reportId, (newValue, oldValue) => {
  if (newValue) {
    embedReport(newValue);
  }
}, { immediate: true });
</script>

<style scoped>
iframe{
  border: 0px !important;
}
</style>
