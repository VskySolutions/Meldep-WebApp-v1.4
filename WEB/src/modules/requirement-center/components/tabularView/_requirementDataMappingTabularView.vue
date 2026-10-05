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
            </q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td style="width: 15%;">
              {{ props.row.source }}
            </q-td>
            <q-td style="width: 15%;">
              {{ props.row.target }}
            </q-td>
            <q-td style="width: 20%;">
              {{ props.row.createdBy.person.fullName }}
            </q-td>
            <q-td style="width: 25%;">
              {{ props.row.createdOnUtc }}
            </q-td>
          </q-tr>
        </template>
      </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch } from 'vue';
import { useAuthStore } from "stores/auth";

import requirementDataMappingService from "modules/requirement-data-mapping/requirementDataMapping.service";

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

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "source", label: "File Path", field: "source", align: "left", sortable: true },
  { name: "target", label: "File Name", field: "target", align: "left", sortable: true },
  { name: "createdBy.person.fullName", label: "Created By", field: "createdBy.person.fullName", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true },

]);

const getRequirementDataMapping = () => {
  loading.value = true;
  requirementDataMappingService.getRequirementDataMapping(props.requirementId).then((resp) => {
    rows.value = (resp.requirementDataMappingsList || []).map(item => ({
        ...item,
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const {
} = useSiteTableState({
  storageKey: "requirement-Center-Requirement-Data-Mapping-Tabular-List",
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
    await getRequirementDataMapping(
      props.requirementId
    );
  },
  { immediate: true }
);

</script>
