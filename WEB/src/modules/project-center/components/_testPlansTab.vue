<template>
  <fieldset>
    <legend>Project Test Plan</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input v-model="filterTestPlan" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      ref="tableRef5" v-model:pagination="paginationTestPlan" :class="rowsTestPlan.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="filteredTestPlan" :columns="columnsTestPlan" row-key="id" separator="cell"
      no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getAllTestPlan"
    >
      <template #header="propsTestPlan">
        <q-tr :props="propsTestPlan" class="bg-primary text-white">
          <q-th v-for="col in propsTestPlan.cols" :key="col.name" :props="propsTestPlan">{{ col.label }}</q-th>
          <!-- <q-th auto-width class="text-center">Actions</q-th> -->
        </q-tr>
      </template>
      <template #body="propsTestPlan">
        <q-tr :props="propsTestPlan" :class="activeRowIdTestPlan == propsTestPlan.row.id ? 'highlight' : ''" :set="(preProjectName = null)">
          <q-td style="width: 3%;" class="hidden">{{ propsTestPlan.row.testPlanNumber }}</q-td>
          <!-- <q-td>{{ props.row.project.name }}</q-td> -->
          <!-- <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;"><span v-if="preProjectName !== propsTestPlan.row.project.name" :set="preProjectName = propsTestPlan.row.project.name">{{ propsTestPlan.row.project.name }}</span></q-td> -->
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ propsTestPlan.row.name }}</q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 12%;">{{ propsTestPlan.row.planMaker.person.fullName }}</q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 12%;">{{ propsTestPlan.row.planReviewer.person.fullName }}</q-td>
          <!-- <q-td style="width: 5%;" class="text-center actions">
            <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onView(propsTestPlan.row.id)" size="xs">
              <q-tooltip>View</q-tooltip>
            </q-icon>
            <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(propsTestPlan.row.id)" size="xs">
              <q-tooltip>Edit</q-tooltip>
            </q-icon>
            <q-icon name="o_checklist" class="cursor-pointer q-mr-sm" @click="$router.push('/test-case?planId='+propsTestPlan.row.id+'&&projectId='+propsTestPlan.row.projectId)" size="xs">
              <q-tooltip>Test Case</q-tooltip>
            </q-icon>
            <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onDelete(propsTestPlan.row)">
              <q-tooltip>Delete</q-tooltip>
            </q-icon>
          </q-td> -->
        </q-tr><q-separator />
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import projectService from "modules/project/projects.service";
import { ref, onMounted, computed } from "vue";

const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;
const loading = ref(true);
const tableRef5 = ref();
const rowsTestPlan = ref([]);
const activeRowIdTestPlan = ref(null);
const filterTestPlan = ref("");
const paginationTestPlan = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columnsTestPlan = ref([
  // { name: "testPlanNumber", label: "Id", field: "testPlanNumber", align: "left", sortable: true },
  // { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "planMaker.person.fullName", label: "Plan Maker", field: "planMaker.person.fullName", align: "left", sortable: false },
  { name: "planReviewer.person.fullName", label: "Plan Reviewer", field: "planReviewer.person.fullName", align: "left", sortable: false }
]);

// On page rendering
onMounted(() => {
  const propsTestPlan = { pagination: paginationTestPlan.value };
  getAllTestPlan(propsTestPlan);
});

const getAllTestPlan = (propsTestPlan) => {
  const { page, rowsPerPage, sortBy, descending } = propsTestPlan.pagination;
  loading.value = true;
  const payloadTestPlan = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllTestPlanForDashboard(payloadTestPlan).then((resp) => {
    rowsTestPlan.value = resp.data;
    paginationTestPlan.value.page = page;
    paginationTestPlan.value.rowsPerPage = rowsPerPage;
    paginationTestPlan.value.sortBy = sortBy;
    paginationTestPlan.value.descending = descending;
    paginationTestPlan.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// for static search
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

const testPlanColumns = columnsTestPlan.value;
const filteredTestPlan = computed(() => filterRows(rowsTestPlan.value, filterTestPlan.value, testPlanColumns));
</script>
