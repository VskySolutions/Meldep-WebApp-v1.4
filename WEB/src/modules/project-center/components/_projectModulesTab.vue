<template>
  <fieldset>
    <legend class="text-h6">Project Module</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input v-model="filter" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="filteredModule" :columns="columns" row-key="id" separator="cell"
      :filter="filter" no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getProjectModules"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
        <!-- <q-th auto-width class="text-center">Actions</q-th> -->
        </q-tr>
      </template>
      <template #body="props">
        <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
          <q-td style="width: 5%;" class="hidden">{{ props.row.projectModuleNumber }}</q-td>
          <!-- <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ props.row.project.name }}</q-td> -->
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ props.row.name }}</q-td>
          <q-td style="width: 5%;">
            <q-select
              v-model="props.row.projectModuleStatus.id" outlined stack-label hide-bottom-space :dense="true"
              :options="projectModuleStatusList" class="project-module-status-list" option-value="value" option-label="text" emit-value map-options :bg-color="getStatusColor(props.row.projectModuleStatus.dropDownValue)" :disable="isClose" @update:model-value="onSubmitModule(props.row.id, props.row.projectModuleStatus.id)"
            />
          </q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">{{ props.row.createdBy.person.fullName }}</q-td>
          <q-td style="width: 5%;">{{ props.row.createdOnUtc }}</q-td>
        <!-- <q-td>{{ props.row.createdOnUtc }}</q-td> -->
        <!-- <q-td style="width: 5%;" class="text-center actions"> -->
          <!-- <q-icon name="o_note" class="cursor-pointer q-mr-sm">
            <q-tooltip>Notes</q-tooltip>
          </q-icon> -->
          <!-- <q-icon name="o_assignment" class="cursor-pointer q-mr-sm" @click="onAddNote(props.row.id, 'Project Module')">
            <q-tooltip>Note</q-tooltip>
          </q-icon>
          <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onView(props.row.id)" size="xs">
            <q-tooltip>View</q-tooltip>
          </q-icon>
          <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(props.row.id)" size="xs">
            <q-tooltip>Edit</q-tooltip>
          </q-icon>
          <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onDelete(props.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-icon> -->
        <!-- </q-td> -->
        </q-tr><q-separator />
      </template>
    </q-table>
  </fieldset>
</template>
<script setup>
import { ref, onMounted, computed } from "vue";
import { notifySuccess } from "assets/utils";
import projectModulesService from "modules/project-modules/projectModules.service";
import commonService from "services/common.service";
import projectService from "modules/project/projects.service";

const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;

// for project module
const loading = ref(true);
const rows = ref([]);
const filter = ref("");
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "projectModuleNumber", label: "Id", field: "projectModuleNumber", align: "left", sortable: true, headerStyle: "width: 100px; display: none" },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "projectModuleStatus.dropDownValue", label: "Status", field: "projectModuleStatus.dropDownValue", align: "left", sortable: false },
  { name: "createdBy.person.fullName", label: "Created By", field: "createdBy.person.fullName", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true }
]);

onMounted(() => {
  const props = { pagination: pagination.value };
  getProjectModules(props);
  getDropDownStatus("WO Status");
});

// Get all project Module Type List
const projectModuleStatusList = ref([]);
function getDropDownStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    projectModuleStatusList.value = responseData;
  });
}

// Get/Map project module list to table
const getProjectModules = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllProjectModulesForDashboard(payload).then((resp) => {
    rows.value = resp.data;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

function onSubmitModule (id, projectModuleStatusId) {
  setTimeout(function () {
    projectModulesService.updateProjectModuleStatus(id, projectModuleStatusId).then(resp => {
      notifySuccess({ message: "Project module status is saved successfully." });
      getProjectModules({ pagination: pagination.value });
    });
  });
}

// Added colors for task status dropdown list
function getStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Open":
      return "purple-4";
    case "Close":
      return "grey-4";
    case "Completed":
      return "green-4";
    case "In Progress":
      return "yellow-4";
    case "New":
      return "blue-4";
    default:
      return "#ffffff";
    }
  }
}
// for static filter
const modulesColumns = columns.value;
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
// Computed properties for each table’s filtered data
const filteredModule = computed(() => filterRows(rows.value, filter.value, modulesColumns));
</script>
