<template>
  <fieldset>
    <legend>{{ model.person ? 'Customer' : 'Company' }} Projects</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input v-model="filterProject" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      ref="tableRef" v-model:pagination="pagination" :class="ProjectRows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="filteredProject" :filter="filterProject" :columns="columns" row-key="id" separator="cell"
      no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getProjects"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;" class="hoverable-cell">{{ props.row.name }}</q-td>
          <q-td style="width: 8%;">{{ toDate(props.row.startDate) }}</q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">
            <span v-if="props.row.projectUserMappings.filter(mapping => mapping.employeeDesignation.dropDownValue === 'Project Coordinator').length > 0">
              <span v-for="(userMapping, index) in props.row.projectUserMappings.filter(mapping => mapping.employeeDesignation.dropDownValue === 'Project Coordinator')" :key="userMapping.id">
                <span text-color="black">{{ userMapping.user.person.fullName }}</span>
                <span v-if="index !== props.row.projectUserMappings.filter(mapping => mapping.employeeDesignation.dropDownValue === 'Project Coordinator').length - 1"><br></span>
              </span>
            </span>
            <span v-else>
              --
            </span>
          </q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">
            <span v-if="props.row.projectLeads.length > 0">
              <span v-for="(lead, index) in props.row.projectLeads" :key="index">
                <span text-color="black">{{ lead }}</span>
                <span v-if="index !== props.row.projectLeads.length - 1"><br></span>
              </span>
            </span>
            <span v-else>
              --
            </span>
          </q-td>
          <q-td style="width: 5%;">{{ props.row.projectPriority.dropDownValue }}</q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 5%;">
            <div>{{ props.row.projectStatus.dropDownValue }}</div>
          </q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 5%;">{{ props.row.projectType.dropDownValue }}</q-td>
        </q-tr><q-separator />
      </template>
    </q-table>
  </fieldset>
</template>
<script setup>
import { ref, computed, onMounted } from "vue";
import useFilters from "composables/useFilters";

import projectService from "modules/project/projects.service";
import commonService from "services/common.service";

const props = defineProps({ customerId: { type: String, default: "" }, isPerson: { type: Boolean, default: false } });

const customerId = props.customerId;
const model = ref({
  description: "",
  person: props.isPerson
});

// Search variables
const search = ref({
  customerId: customerId
});

const { toDate } = useFilters();
const loading = ref(true);
const tableRef = ref();
const ProjectRows = ref([]);
const activeRowId = ref(null);
const filterProject = ref("");
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Project Name", field: "name", align: "left", sortable: true },
  { name: "startDate", label: "Start Date", field: "startDate", align: "left", sortable: true },
  { name: "projectCoordinator.id", label: "Project Coordinator", field: "projectCoordinator.id", align: "left", sortable: true },
  { name: "projectLeads", label: "Project Lead", field: row => row.projectLeads.join(", "), align: "left", sortable: false },
  { name: "projectPriority.dropDownValue", label: "Project Priorirty", field: "projectPriority.dropDownValue", align: "left", sortable: true },
  { name: "projectStatus.dropDownValue", label: "Status", field: "projectStatus.dropDownValue", align: "left", sortable: true },
  { name: "projectType.dropDownValue", label: "Project Type", field: "projectType.dropDownValue", align: "left", sortable: true }
]);

const projectColumns = columns.value; // Pass the full columns array
// Get/Map project list to table
const getProjects = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  projectService.getProjects(payload).then((resp) => {
    ProjectRows.value = resp.data.map(project => ({
      ...project,
      checkboxStatus: false, // Initialize checkboxStatus for each row
      projectLeads: project.projectUserMappings ? project.projectUserMappings.filter(mapping => mapping.employeeDesignation.dropDownValue === "Project Lead")
        .map(mapping => mapping.user.person.fullName) : []
    }));
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

const filteredProject = computed(() => commonService.filterRowsBySearchTerm(ProjectRows.value, filterProject.value, projectColumns));

onMounted(() => {
  const props = { pagination: pagination.value };
  getProjects(props);
});

</script>
