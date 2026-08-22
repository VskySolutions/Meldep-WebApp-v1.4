<template>
  <fieldset>
    <legend>Action Items</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input
        v-model="filterItems" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search"
        dense clearable
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      ref="tableRef1"
      v-model:pagination="paginationItems"
      :class="ItemRows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
      flat
      bordered
      :loading="loading"
      :rows="filteredItems"
      :columns="columnsItems"
      row-key="id"
      separator="cell"
      :filter="filterItems"
      binary-state-sort
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      @request="getAllProjectActionItemsForDashboard"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr
          :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''"
          :set="(preProjectName = null, preRequirementTitle = null)"
        >
          <q-td class="hoverable-cell" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">
            <span
            v-if="preRequirementTitle !== props.row.requirement.title"
            :set="preRequirementTitle = props.row.requirement.title"
            @click="onRequirementView(props.row.requirementId)"
            >
            {{ props.row.requirement.title }}
            </span>
          </q-td>
          <q-td style="width: 20%; white-space: normal;">
            <div class="row no-wrap items-center justify-between">
              <span style="flex: 1; word-break: break-word; white-space: normal;">
                <span>{{ props.row.title }}</span>
              </span>
            </div>
          </q-td>
          <q-td style="width: 20%; white-space: normal;">
            <div class="row no-wrap items-center justify-between">
              <span style="flex: 1; word-break: break-word; white-space: normal;">
                <span>{{ props.row.customer.name }}</span>
              </span>
            </div>
          </q-td>
          <q-td style="width: 20%; white-space: normal;">
            <div class="row no-wrap items-center justify-between">
              <span style="flex: 1; word-break: break-word; white-space: normal;">
                <span>{{ props.row.employee.person.fullName }}</span>
              </span>
            </div>
          </q-td>
          <q-td style="width: 10%; white-space: normal;">
            <div class="row no-wrap items-center justify-between">
              <span style="flex: 1; word-break: break-word; white-space: normal;">
                <span>{{ props.row.dueDate }}</span>
              </span>
            </div>
          </q-td>
          <q-td style="width: 10%; white-space: normal;">
            <div class="row no-wrap items-center justify-between">
              <span style="flex: 1; word-break: break-word; white-space: normal;">
                <span>{{ props.row.priority.dropDownValue }}</span>
              </span>
            </div>
          </q-td>
        </q-tr>
        <q-separator />
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useQuasar } from "quasar";
import useFilters from "composables/useFilters";

import projectActionItemsService from "modules/project-action-items/projectActionItems.service";

import {
  initRequirementDialogs,
  onRequirementView
} from "src/modules/requirement/utils/dialogs.js";

const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;
const { toDate } = useFilters();
const $q = useQuasar();

const loading = ref(true);
const tableRef1 = ref();
const ItemRows = ref([]);
const activeRowId = ref(null);
const filterItems = ref("");
const paginationItems = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

const columnsItems = ref([
  { name: "requirement.title", label: "Requirement", field: "requirement.title", align: "left", sortable: true },
  { name: "title", label: "Title", field: "title", align: "left", sortable: true },
  { name: "customerId", label: "Customer", field: "customer.name", align: "left", sortable: true },
  { name: "employeeId", label: "Employee", field: "employee.person.fullName", align: "left", sortable: true },
  { name: "dueDate", label: "Due Date", field: "dueDate", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true }
]);

const getAllProjectActionItemsForDashboard = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectActionItemsService.getAllProjectActionItemsForDashboard(payload).then((resp) => {
    ItemRows.value = resp.projectActionItemList;
    paginationItems.value.page = page;
    paginationItems.value.rowsPerPage = rowsPerPage;
    paginationItems.value.sortBy = sortBy;
    paginationItems.value.descending = descending;
    paginationItems.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initRequirementDialogs(activeRowId);

// for static filter
const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data; // If no filter, return all data
  const lowerCaseTerm = searchTerm.toLowerCase();
  return data.filter(row =>
    columns.some(column => {
      const value = column.field.split(".").reduce((obj, key) => obj?.[key], row); // Handle nested fields
      return String(value || "").toLowerCase().includes(lowerCaseTerm);
    })
  );
};

const itemsColumns = columnsItems.value;
const filteredItems= computed(() => filterRows(ItemRows.value, filterItems.value, itemsColumns));

// On page rendering
onMounted(() => {
  const props = { pagination: paginationItems.value };
  getAllProjectActionItemsForDashboard(props);
});

</script>
