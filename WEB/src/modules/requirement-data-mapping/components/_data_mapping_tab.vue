<template>
  <div class="">
    <fieldset class="q-mb-lg">
      <legend>Data Mappings</legend>
      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        bordered class="no-shadow"
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        :filter="filter"
        separator="cell"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
      >
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">
              {{ col.label }}
            </q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td class="text-left common-q-td">
              {{ props.row.source }}
            </q-td>
            <q-td class="text-left common-q-td" >
              {{ props.row.target }}
            </q-td>
            <q-td class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="onRequirementDataMappingNoteEdit(props.row.id, false)"
              >
                <q-tooltip>
                  View Notes
                </q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
        </template>
      </q-table>
    </fieldset>
  </div>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";

import requirementDataMappingService from "../requirementDataMapping.service";

import {
  initRequirementDataMappingDialogs,
  onRequirementDataMappingNoteEdit
} from "src/modules/requirement-data-mapping/utils/dialogs.js";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const rows = ref([]);
const filter = ref("");
const loading = ref(true);
const activeRowId = ref(null);

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "source", label: "Source", field: "source", align: "left", sortable: true },
  { name: "target", label: "Target", field: "target", align: "left", sortable: true }
]);

// get data mapping details on edit mode
const getAllDataMappingByRequirementId = (propss) => {
  const requirementId = props.id;
  loading.value = true;

  const { page, rowsPerPage, sortBy, descending } = propss.pagination;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    requirementId
  };

  requirementDataMappingService.getAllDataMappingByRequirementId(payload)
    .then((resp) => {
      rows.value = resp.requirementDataMappingsList.map(item => ({
        ...item
      }));
    })
    .finally(() => {
      loading.value = false;
    });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initRequirementDataMappingDialogs();

// On page rendering
onMounted(() => {
  const propps = { pagination: pagination.value };
  getAllDataMappingByRequirementId(propps);
});
</script>
