<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <q-card-section class="row items-center justify-end q-pb-sm">

      <div class="row items-center q-gutter-sm">
        <q-btn
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary q-ml-md hidden"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/requirement',
            state: {
              projectId: projectId,
              projectModuleId: projectModuleId,
              requirementId: props.requirementId
            }
          })"
        >
          <q-tooltip>Open Requirement List</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>
    <q-separator />
      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        bordered class="no-shadow"
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
      >
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">
              {{ col.label }}
              <q-icon v-if="col.tooltip" name="o_info" size="xs" class="q-mx-xs">
                <q-tooltip class="text-caption">
                  {{ col.tooltip }}
                </q-tooltip>
              </q-icon>
            </q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''">
            <q-td style="width: 10%">
              {{ props.row.trackingDate }}
            </q-td>

            <q-td style="width: 10%">
              {{ props.row.currentPercent }}
            </q-td>

            <q-td style="width: 10%">
              {{ props.row.timeSpentAsOfToday }}
            </q-td>

            <q-td style="width: 15%">
              {{ props.row.estimatedRemainingTime }}
            </q-td>

            <q-td style="width: 20%">
              {{ props.row.scope }}
            </q-td>

            <q-td style="width: 15%">
              {{ props.row.roadblock }}
            </q-td>

            <q-td style="width: 13%">
              {{ props.row.estimatedTimeForCompletion }}
            </q-td>
          </q-tr>
        </template>
      </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch } from 'vue';
import { useAuthStore } from "stores/auth";

import requirementStatusTrackingService from "modules/requirement-status-tracking/requirementStatusTracking.service";

// Shared DataTable Views
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

const emit = defineEmits(['summary'])

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
})

// Common variables
const rows = ref([]);
const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);
const loading = ref(true);
const projectId = ref('');
const projectModuleId = ref('');
const activeRowId = ref(null);
const highlightedId = computed(() => activeRowId.value);

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "trackingDate", label: "Tracking Date", field: "trackingDate", sortable: true, align: "left" },
  { name: "currentPercent", label: "Comp. (%)", field: "currentPercent", sortable: true, align: "left", tooltip: "Current Completion" },
  { name: "timeSpentAsOfToday", label: "TS (Hrs)", field: "timeSpentAsOfToday", sortable: true, align: "left", tooltip: "Time Spent as of Today(Hours)" },
  { name: "estimatedRemainingTime", label: "ERT (Hrs)", field: "estimatedRemainingTime", sortable: true, align: "left", tooltip: "Estimated Remaining Time Required (Hours)" },
  { name: "scope", label: "Current Status", field: "scope", sortable: true, align: "left" },
  { name: "roadblock", label: "Roadblock", field: "roadblock", sortable: true, align: "left" },
  { name: "estimatedTimeForCompletion", label: "ETC (Hrs)", field: "estimatedTimeForCompletion", sortable: true, align: "left", tooltip: "Estimated Time for Completion of Current Status (Hours)"}
]);

const getRequirementStatusTrackingByRequirementId = () => {
  loading.value = true;
  requirementStatusTrackingService.getRequirementStatusTrackingByRequirementId(props.requirementId).then((resp) => {
    rows.value = (resp.requirementStatusTrackingRecordsList || []).map(item => ({
        ...item,
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const {
} = useSiteTableState({
  storageKey: "requirement-Center-Requirement-Status-Tracking-Tabular-List",
  siteId: siteId,
  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
  async () => {
    await getRequirementStatusTrackingByRequirementId(
      props.requirementId
    );
  },
  { immediate: true }
);

</script>
