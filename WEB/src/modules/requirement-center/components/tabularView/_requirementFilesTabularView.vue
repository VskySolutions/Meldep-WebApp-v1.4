<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <q-card-section class="row items-center justify-end q-pb-sm">
      
      <div class="row items-center q-gutter-sm">
        <q-btn
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary q-ml-md"
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
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">
              <a :href="props.row.filePath" target="_blank" class="text-bluee">
                {{ props.row.filePath }}
              </a>
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">
              {{ props.row.fileName }}
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 30%;">
              {{ props.row.note }}
            </q-td>
          </q-tr>
        </template>
      </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch } from 'vue';
import { useAuthStore } from "stores/auth";

import requirementService from "modules/requirement/requirement.service";

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
  { name: "filePath", label: "File Path", field: "filePath", align: "left", sortable: true },
  { name: "fileName", label: "File Name", field: "fileName", align: "left", sortable: true },
  { name: "note", label: "Notes", field: "note", align: "left", sortable: true }
]);

const getRequirement = () => {
  loading.value = true;
  requirementService.getRequirementDetails(props.requirementId).then((resp) => {
    rows.value = resp.filePathDetails.map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const {
} = useSiteTableState({
  storageKey: "requirement-Center-Requirement-Files-Tabular-List",
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
    await getRequirement(
      props.requirementId
    );
  },
  { immediate: true }
);

</script>
