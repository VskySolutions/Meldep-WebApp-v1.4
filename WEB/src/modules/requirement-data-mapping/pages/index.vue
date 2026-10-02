<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-3 col-sm-2 col-md-3 col-lg-4 col-xl-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="SDLC" />
              <q-breadcrumbs-el label="Requirement Data Mappings" />
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
          <div class="col-12 col-xs-6 col-sm-8 col-md-7 col-lg-6 col-xl-7">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
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
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.projectModuleIds"
                        label="Project Module"
                        :options="projectModulesByProjectIdForDropdown.list.value"
                        :filter="projectModulesByProjectIdForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.requirementIds"
                        label="Requirement"
                        :options="requirementsByProjectModuleIdForDropdown.list.value"
                        :filter="requirementsByProjectModuleIdForDropdown.filter"
                      />
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onAdvanceSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onAdvanceClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <div class="row items-center q-gutter-sm q-ml-xs">
                <q-btn
                  v-if="!isViewer"
                  icon="o_add"
                  outline
                  label="Add Data Mapping"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onAddRequirementDataMapping(false, refreshDataMappingList)"
                />
                 <!-- Reset Column Width -->
                <q-btn
                  icon="o_refresh"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="resetColumnsWidth()"
                >
                  <q-tooltip>Reset Columns Width</q-tooltip>
                </q-btn>
                <!-- Column Hide/Show -->
                <columnVisibilityMenu
                  :all-column-names="allColumnNames"
                  :selected-column-names="selectedColumnNames"
                  @update:selected-column-names="selectedColumnNames = $event"
                  @select-all-columns="selectAllColumns"
                  @default-columns="defaultColumns"
                />
                <!-- Button to Open Sorting Dialog -->
                <q-btn
                  color="primary"
                  icon="o_sort"
                  class="btnRounded q-ml-xs"
                  @click="showSortDialog = true"
                >
                  <q-badge v-if="selectedSortCount > 0" color="green" floating class="q-ml-xs">
                    {{ selectedSortCount }}
                  </q-badge>
                  <q-tooltip>Sort</q-tooltip>
                </q-btn>
                <q-btn
                  v-if="selectedProjectId"
                  icon="o_chevron_left"
                  outline
                  label="Back"
                  no-caps
                  class="text-primary btnRounded no-space-between q-ml-sm"
                  @click="$router.back()"
                />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="table-timesheet">
        <div class="table-scroll-container">
          <q-table
            ref="tableRef"
            v-model:pagination="pagination"
            :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
            :loading="loading"
            :columns="computedColumns"
            :rows="rows"
            row-key="id"
            separator="cell"
            binary-state-sort
            :rows-per-page-options="[20, 50, 100, 200, 500]"
            @request="getAllRequirementDataMappingGroups"
          >
            <template #loading>
              <q-inner-loading showing color="primary">
                <q-spinner-ios size="40px" class="q-mt-xl" />
              </q-inner-loading>
            </template>
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <!-- <q-th></q-th> -->
                <q-th
                  v-for="col in props.cols"
                  :key="col.name"
                  :style="{
                    width: (resizeWidths?.[col.name] || 120) + 'px',
                    minWidth: '80px',
                    position: 'relative'
                  }"
                >
                  {{ col.label }}
                  <!-- Sort icon only -->
                  <q-icon
                    v-if="col.sortable"
                    :name=" pagination.sortBy === col.name ? (pagination.descending ? 'o_arrow_downward' : 'o_arrow_upward') : 'o_unfold_more' "
                    size="16px"
                    class="cursor-pointer q-ml-sm"
                    @click.stop="sortColumn(col)"
                  >
                    <q-tooltip>
                      {{ pagination.sortBy === col.name ? (pagination.descending ? 'Sort Ascending' : 'Sort Descending') : 'Sort' }}
                    </q-tooltip>
                  </q-icon>
                  <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
                </q-th>
                <q-th></q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr
                :props="props"
                :class="activeRowId == props.row.id ? 'highlight' : ''"
              >
                <q-td :colspan="computedColumns.length" style="background: #dbf2ff;" class="text-center text-bold">
                  {{ props.row.requirementTitle }}
                </q-td>
                <q-td auto-width class="text-center actions" style="background: #dbf2ff;">
                  <q-icon
                    v-if="!isViewer"
                    name="o_edit"
                    class="cursor-pointer q-mr-sm"
                    @click="onEditRequirementDataMapping(props.row.requirementId, true, refreshDataMappingList)"
                  >
                    <q-tooltip>Edit</q-tooltip>
                  </q-icon>
                </q-td>
              </q-tr>
              <q-tr
                v-for="mapping in props.row.dataMappings"
                :key="mapping.id"
                :class="highlightedId == mapping.id ? 'highlight' : ''"
              >
                <q-td v-if="selectedColumnNames.includes('source')" class="text-left common-q-td">
                  {{ mapping.source }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('target')" class="text-left common-q-td" >
                  {{ mapping.target }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('note')" class="RichTextEditor common-q-td hidden">
                  <div v-html="mapping.requirementDataMappingNotes[0]?.note" />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('createdById')" class="text-left common-q-td">
                  {{ mapping.createdBy?.person.fullName }}
                </q-td>
                <q-td></q-td>
              </q-tr>
              <q-separator />
            </template>
          </q-table>
        </div>
      </div>
    </q-card>
  </q-page>
  <!-- Multi-Column Level Sorting -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="columns"
    :multi-sort="multiSort"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";

import requirementDataMappingService from "../requirementDataMapping.service";

// Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

import {
  initRequirementDataMappingDialogs,
  onAddRequirementDataMapping,
  onEditRequirementDataMapping
} from "src/modules/requirement-data-mapping/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const $q = useQuasar();
const authStore = useAuthStore();
const user = authStore.user;
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const showSortDialog = ref(false);
const selectedProjectId = history.state?.projectId;
const isViewer = user?.roles?.some(r => r?.toLowerCase() === "viewer") ?? false;

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "source", label: "Source", field: "source", align: "left", sortable: true, default: true },
  { name: "target", label: "Target", field: "target", align: "left", sortable: true, default: true }
  // { name: "note", label: "Note", field: "note", align: "left", sortable: false, default: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Timesheet
// ----------------------------------------------------------------------------------------------------------------

const getAllRequirementDataMappingGroups = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  loading.value = true;
  search.value.projectIds = search.value.projectIds === "" ? null : search.value.projectIds;
  search.value.projectModuleIds = search.value.projectModuleIds === "" ? null : search.value.projectModuleIds;
  search.value.requirementIds = search.value.requirementIds === "" ? null : search.value.requirementIds;

  const sorts = {};
  const multi = multiSort.value;
  for (let i = 0; i < multi.length; i++) {
    const s = multi[i];
    if (s.column && s.direction) {
      sorts[s.column] = s.direction;
    }
  }

  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    sorts,
    ...search.value
  };
  saveDataTableState({
    search: search.value,
    pagination: p,
    activeRowId: activeRowId.value,
    sorts
  });

  requirementDataMappingService.getAllRequirementDataMappingGroups(payload)
    .then((resp) => {
      rows.value = resp.dataMappingGroupList.map(item => ({
        ...item,
        requirementId: item.requirementId,
        requirementTitle: item.requirementTitle,
        dataMappings: item.dataMappings || []
      }));
      pagination.value = {
        ...pagination.value,
        page,
        rowsPerPage,
        sortBy,
        descending,
        rowsNumber: resp.total
      };
    })
    .finally(() => {
      loading.value = false;
      searchLoader.value = false;
    });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  resizeWidths,
  selectedColumnNames,
  saveDataTableState,
  saveResizableWidthState,
  saveColumnsState
} = useSiteTableState({
  storageKey: "requirement-data-mapping-Index",
  siteId: user?.siteId,

  defaultSearch: {
    searchText: "",
    projectIds: [],
    projectModuleIds: [],
    requirementIds: [],
    source: "",
    target: ""
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  defaultSorts: {},

  defaultResizableWidth: {},

  defaultColumns: columns.value
    .filter(col => col.default === true)
    .map(col => col.name)
});

if (history.state?.projectId) {
  search.value.projectIds = history.state.projectId;
}

const lsSorts = sorts.value || null;
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const highlightedId = computed(() => { return activeRowId.value; });

function setActiveRowIdInLocalStorage(id) {
  activeRowId.value = id;

  saveDataTableState({
    activeRowId: id
  });
}

// const handleDocumentClick = (event) => {
//   const highlightElement = document.querySelector(".highlight");
//   // Check if clicked inside the highlighted row or icons
//   if (highlightElement && !highlightElement.contains(event.target)) {
//     activeRowId.value = null;
//     const storedData = getLocalStorage(localStorageKey) || {};
//     setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
//   }
// };

// onBeforeUnmount(() => {
//   document.removeEventListener("click", handleDocumentClick);
// });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Column resize functionality (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  startResize,
  resetColumnsWidth,
  isResizing
} = useColumnResize({
  columns,
  resizeWidths,
  saveResizableWidthState
});
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Hide/Show Columns (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  selectAllColumns,
  defaultColumns,
  allColumnNames,
  computedColumns
} = useColumnManager({
  columns,
  selectedColumnNames,
  saveColumnsState,
  isResizing
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Sort Filter (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  multiSort,
  addSortLevel,
  removeSortLevel,
  applyMultiSort,
  selectedSortCount
} = useMultiSort({
  lsSorts,
  saveDataTableState,
  onApplySort: () => {
    refreshDataMappingList();
  }
});

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

const refreshDataMappingList = () => {
  getAllRequirementDataMappingGroups({ pagination: pagination.value });
};

const sortColumn = (col) => {
  if (!col.sortable) return;
  if (pagination.value.sortBy === col.name) {
    // Same column → toggle direction
    pagination.value.descending = !pagination.value.descending;
  }
  else {
    // New column → ascending
      pagination.value.sortBy = col.name;
      pagination.value.descending = false;
  }
  refreshDataMappingList();
};

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshDataMappingList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.projectIds = [];
  search.value.projectModuleIds = [];
  search.value.requirementIds = [];
  search.value.source = "",
  search.value.target = ""
  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initRequirementDataMappingDialogs();
// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// ----------------------------------------------------------------------------------------------------------------

const mapFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Modules"),
  ...mapFilterToLabel(search.value.requirementIds, requirementsByProjectModuleIdForDropdown.list, "Requirement"),
  ...(search.value.source ? { Source: search.value.source } : {}),
  ...(search.value.target ? { Target: search.value.target } : {})
}));

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Project Modules") {
    search.value.projectModuleIds = [];
  } else if (key === "Requirement") {
    search.value.requirementIds = [];
  } else if (key === "Source") {
    search.value.source = "";
  } else if (key === "Target") {
    search.value.target = "";
  }
  delete appliedFilters.value[key];
  refreshDataMappingList();
}

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Requirement": return search.value.requirementIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectNameDropdown } = projectModule();
const { projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { requirementsByProjectModuleIdForDropdown } = requirementModule();

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshDataMappingList();
});

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (!newValue || newValue === oldValue) return;

  search.value.projectModuleIds = [];
  await projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);
}, { immediate: true });

watch(() => search.value.projectModuleIds, (newValue, oldValue) => {
  if (search.value.projectModuleIds?.length === 0) {
    search.value.requirementIds = [];
    return;
  }
  if (search.value?.projectModuleIds?.length === 0 || newValue === oldValue) return;

  if (newValue == null) return;
    requirementsByProjectModuleIdForDropdown.load(newValue);
}, { immediate: true });

watch(activeRowId, (val) => {
  const formattedSorts = {};

  for (const s of multiSort.value) {
    if (s.column && s.direction) {
      formattedSorts[s.column] = s.direction;
    }
  }

  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    activeRowId: val,
    sorts: formattedSorts
  });
});
// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  projectNameDropdown.load();

  if (search.value.projectIds) projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);
  if (search.value.projectModuleIds?.length > 0) requirementsByProjectModuleIdForDropdown.load(search.value.projectModuleIds);

  if (!activeRowId.value) {
    activeRowId.value = null;
  }
  // document.addEventListener("click", handleDocumentClick);
  refreshDataMappingList();
});

</script>
<style scoped>
.table-timesheet .Custom-DataTable {
  min-width: max-content;
}
</style>
