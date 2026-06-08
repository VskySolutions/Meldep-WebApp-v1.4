<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-2 col-sm-1 col-md-2 col-lg-3 col-xl-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el :label="!search.isTemplate ? 'Projects' : 'Project Templates'" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-7 col-sm-9 col-md-8 col-lg-7 col-xl-7">
            <div class="row items-center justify-end no-wrap">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Search by</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-radio v-model="search.isTemplate" checked-icon="o_task_alt" unchecked-icon="o_panorama_fish_eye" :val="false" label="Projects" @click="onChangeProjectOrTemplate()" />
                          <q-radio v-model="search.isTemplate" checked-icon="o_task_alt" unchecked-icon="o_panorama_fish_eye" :val="true" label="Templates" @click="onChangeProjectOrTemplate()" />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              <div class="q-gutter-md">
                <q-btn icon="o_add" outline label="Assign Bulk" no-caps class="text-primary btnRounded q-ml-lg" @click="onAssignBulkUserToProject(refreshProjectUserList)">
                  <q-tooltip>Assign Bulk</q-tooltip>
                </q-btn>
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.back()" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-separator />
      <q-table
        ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getProjectUsers"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td style="width: 20%;">
              {{ props.row.name }}
            </q-td>
            <q-td
              style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;"
            >
              <div v-if="props.row.projectUsers?.length" class="col-9 flex TaskActivity cursor-grab">
                <div v-for="(user, index) in props.row.projectUsers" :key="index" class="Person q-mr-xs">
                  <span>{{ getInitials(user.text || user.name) }}</span>
                  <q-tooltip>
                    <div>
                      <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                      <span>{{ user.text }}</span>
                    </div>
                    <div v-if="user.fullAccess">
                      <q-icon name="o_edit" color="white" size="xs" class="q-mr-xs" />
                      <span><q-icon name="o_done" color="white" size="xs" class="q-mr-xs" /></span>
                    </div>
                    <div v-if="user.viewOnly">
                      <q-icon name="o_visibility" color="white" size="xs" class="q-mr-xs" />
                      <span><q-icon name="o_done" color="white" size="xs" class="q-mr-xs" /></span>
                    </div>
                    <div v-if="user.notes">
                      <q-icon name="o_assignment" color="white" size="xs" class="q-mr-xs" />
                      <span><q-icon name="o_done" color="white" size="xs" class="q-mr-xs" /></span>
                    </div>
                  </q-tooltip>
                </div>
              </div>
            </q-td>
            <q-td style="width: 3%;" class="text-center actions">
              <div class="relative-position inline-block custom-badge">
                <span
                  class="cursor-pointer flex items-center justify-center p-pm-icon-label"
                  @click="onAssignUserToProject(props.row.id, props.row.name, refreshProjectUserList)"
                >
                  P
                  <q-tooltip>Assign Users To Project</q-tooltip>
                </span>
                <q-badge
                  v-if="Array.isArray(props.row.projectUsers) && props.row.projectUsers.length"
                  color="green"
                  floating
                >
                  {{ props.row.projectUsers.length }}
                </q-badge>
              </div>
              <div class="relative-position inline-block custom-badge q-ml-md">
                <span
                  :class="['cursor-pointer', 'flex', 'items-center', 'justify-center', 'p-pm-icon-label', { 'disabled-btn': !(props.row.projectUsers?.length) }]"
                  @click="onAssignBulkUsersToProjectModule(props.row.id, props.row.name, refreshProjectUserList)"
                >
                  PM
                  <q-tooltip>Assign Users To Project Module</q-tooltip>
                </span>

                <q-badge
                  v-if="props.row.projectModulesUser && props.row.projectModulesUser.length > 0"
                  color="green"
                  floating
                >
                  {{ props.row.projectModulesUser.length }}
                </q-badge>
              </div>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import projectService from "modules/project/projects.service";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onAssignUserToProject,
  onAssignBulkUserToProject,
  onAssignBulkUsersToProjectModule
} from "src/modules/project/utils/dialogs.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "name", label: "Project Name", field: "name", align: "left", sortable: true },
  { name: "projectUsers", label: "Assign To", field: "projectUsers", align: "left", sortable: false }
]);

// local storage values
const localStorageKey = "Projects";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds : [];
const isTemplate = filterLocalStorage ? filterLocalStorage.isTemplate : false;
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  projectIds,
  isTemplate
});

// Get/Map project list to table
const getProjectUsers = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  // Check if there are any active filters
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  projectService.getProjectUsers(payload).then((resp) => {
    rows.value = resp.data.map(project => ({
      ...project,
      projectUsers: project.projectUserMappings
        ? project.projectUserMappings
          .map(mapping => {
            const id = mapping.user?.id;
            const name = mapping.user?.person?.fullName;
            const fullAccess = mapping.fullAccess;
            const viewOnly = mapping.viewOnly;
            const notes = mapping.notes;
            return {
              value: String(id),
              text: name || "Unknown",
              fullAccess,
              viewOnly,
              notes
            };
          })
        : [],
      projectModulesUser: Array.isArray(project.projectModules)
        ? project.projectModules.flatMap(module =>
          module.projectModulesUserMappings?.map(mapping => ({
            value: String(mapping.user?.id)
          })) || []
        )
        : []
    }));
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshProjectUserList = () => {
  getProjectUsers({ pagination: pagination.value });
};

function loadProjectNameDropdown() {
  projectNameDropdown.load(search.value.isTemplate, true);
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getProjectUsers(propps);
};

// Clear search
const onClear = () => {
  search.value.projectIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

function getInitials (fullName) {
  return fullName
    .split(" ")
    .map(word => word[0])
    .join("")
    .toUpperCase();
}

const onChangeProjectOrTemplate = () => {
  search.value.projectIds = [];
  refreshProjectUserList();
  loadProjectNameDropdown();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initProjectDialogs(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown
} = projectModule();

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const mapFilterToLabel = (ids, list, label) => {
  if (!Array.isArray(ids) || !ids.length) return {};

  const text = ids
    .map(id => {
      const match = list.value.find(item => item.value === id);
      return match ? match.text : id;
    })
    .join(", ");

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name")
}));

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  }
  delete appliedFilters.value[key];
  getProjectUsers({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getProjectUsers({ pagination: pagination.value });
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  projectNameDropdown.load();
  loadProjectNameDropdown();
});
</script>
<style scoped>
.TaskActivity{
  padding-right: 10px;
}
.TaskActivity .Person {
    border-radius: 50%;
    background-color: #5d5d5d;
    color: white;
    font-size: 12px;
    font-weight: 600;
    padding: 2px 3px;
    margin-right: 3px;
    transition: 0.5s all ease-in-out;
}
.cursor-grab{
  cursor: grab;
  background: none !important;
}
.p-pm-icon-label {
  width: 24px;
  height: 24px;
  border: 0.8px solid #000;
  font-size: 12px;
  font-weight: bold;
  border-radius: 2px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.disabled-btn {
  opacity: 0.5;        /* visually show it’s disabled */
  pointer-events: none; /* prevent clicks */
  cursor: not-allowed;
}
.custom-badge > .q-badge--floating {
    position: absolute;
    top: -4px;
    right: -13px;
    cursor: inherit;
}
</style>
