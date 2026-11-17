<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-3 col-sm-2 col-md-3 col-lg-4 col-xl-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="SDLC" />
              <q-breadcrumbs-el label="Requirement Status Tracking" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs cursor-pointer" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-6 col-sm-8 col-md-7 col-lg-6 col-xl-6">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <!-- Dependent filters: modules of the selected projects; requirements of the selected projects and modules -->
                      <multiSelectDropdown
                        v-model="search.projectModuleIds"
                        label="Project Module"
                        :disable="!search.projectIds?.length"
                        :options="projectModulesByProjectIdForDropdown.list.value"
                        :filter="projectModulesByProjectIdForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.requirementIds"
                        label="Requirement"
                        :disable="!search.projectModuleIds?.length"
                        :options="requirementsByProjectModuleIdForDropdown.list.value"
                        :filter="requirementsByProjectModuleIdForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">From Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.fromDate" fill-input dense clearable placeholder="MM/DD/YYYY" :error="!!fromDateError" :error-message="fromDateError" hide-bottom-space>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="fromDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.fromDate" mask="MM/DD/YYYY" :options="fromDateOptions" @update:model-value="() => fromDateProxy?.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                     <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">To Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.toDate" fill-input dense clearable placeholder="MM/DD/YYYY" :error="!!toDateError" :error-message="toDateError" hide-bottom-space>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="toDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.toDate" mask="MM/DD/YYYY" :options="toDateOptions" @update:model-value="() => toDateProxy?.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps :disable="!!fromDateError || !!toDateError" @click="() => { showFilter = false; onAdvanceSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onAdvanceClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <div class="row items-center q-gutter-sm q-ml-xs no-wrap">
                <q-btn v-can
                  v-if="!isViewer"
                  icon="o_add"
                  outline
                  label="Add Requirement Status Tracking"
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="onAddRequirementStatusTracking(refreshList)"
                />
                <!-- Multi Actions on complete records -->
                <q-btn
                  v-if="!isViewer"
                  icon="o_checklist"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs hidden"
                  :disable="selectedRecordIds.length === 0"
                  @click.stop="showMultiActions = !showMultiActions"
                >
                  <q-badge v-if="selectedRecordIds.length > 0" :label="selectedRecordIds.length" class="primary" floating />
                  <q-tooltip>{{ selectedRecordIds.length ? "Multi Actions" : "Select tracking records to use Multi Actions" }}</q-tooltip>
                  <q-menu v-model="showMultiActions" anchor="bottom right" self="top right" style="width: 300px;">
                    <q-card class="q-pa-sm">
                      <div class="text-subtitle2 q-mt-sm q-mb-sm">{{ selectedRecordIds.length }} tracking record(s) selected</div><q-separator />
                      <q-list style="min-width: 200px">
                        <q-item :disable="true">
                          <q-item-section avatar><q-icon name="o_edit_note" size="xs" color="grey-7" /></q-item-section>
                          <q-item-section>Bulk Edit</q-item-section>
                          <q-tooltip>Bulk Edit is not enabled yet: the editable fields and row rules are awaiting confirmation.</q-tooltip>
                        </q-item>
                        <q-item v-close-popup clickable @click="onBulkDelete">
                          <q-item-section avatar><q-icon name="o_delete_outline" size="xs" color="negative" /></q-item-section>
                          <q-item-section>Bulk Delete</q-item-section>
                        </q-item>
                      </q-list>
                    </q-card>
                  </q-menu>
                </q-btn>
                <q-btn
                  v-if="selectedRecordIds.length > 0"
                  icon="o_restart_alt"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs hidden"
                  @click.stop="clearSelection"
                >
                  <q-tooltip>Unselect All Tracking Records</q-tooltip>
                </q-btn>
                <!-- Reset Column Width -->
                <q-btn
                  icon="o_refresh"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
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
                <!-- Multi-Column Level Sorting -->
                <q-btn
                  color="primary"
                  icon="o_sort"
                  class="btnRounded q-ml-sm"
                  @click="showSortDialog = true"
                >
                  <q-badge v-if="selectedSortCount > 0" color="green" floating class="q-ml-xs">
                    {{ selectedSortCount }}
                  </q-badge>
                  <q-tooltip>Sort</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="table-tracking">
        <div class="table-scroll-container">
          <q-table
            ref="tableRef"
            v-model:pagination="pagination"
            :class="records.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
            :loading="loading"
            :columns="computedColumns"
            :rows="displayRows"
            row-key="key"
            separator="cell"
            binary-state-sort
            no-data-label="No data available"
            rows-per-page-label="Requirements per page:"
            :rows-per-page-options="[20, 50, 100, 200, 500]"
            @request="getAllRequirementStatusTracking"
          >
            <template #loading>
              <q-inner-loading showing color="primary">
                <q-spinner-ios size="40px" class="q-mt-xl" />
              </q-inner-loading>
            </template>
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
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
                  <q-icon
                    v-if="col.sortable"
                    :name=" pagination.sortBy === col.name ? (pagination.descending ? 'o_arrow_downward' : 'o_arrow_upward') : 'o_unfold_more' "
                    size="16px"
                    class="cursor-pointer q-ml-sm"
                    @click.stop="sortColumn(col)"
                  >
                    <q-tooltip>
                      {{ pagination.sortBy === col.name ? (pagination.descending ? 'Sort Ascending' : 'Sort Descending') : 'Sort' }} requirements
                    </q-tooltip>
                  </q-icon>
                  <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
                  <q-icon v-if="col.tooltip" name="o_info" size="xs" class="q-mx-xs">
                    <q-tooltip class="text-caption">
                      {{ col.tooltip }}
                    </q-tooltip>
                  </q-icon>
                </q-th>
                <q-th auto-width class="text-center">Actions</q-th>
              </q-tr>
            </template>
            <template #body="props">
              <!-- Requirement group header: the tracking records below belong to this requirement -->
              <q-tr v-if="props.row.isGroup" :props="props" class="requirement-group-row">
                <q-td :colspan="props.cols.length" class="requirement-group-cell">
                  <div class="row items-center no-wrap q-gutter-x-sm">
                    <q-icon name="o_assignment" size="xs" color="primary" />
                    <span
                      class="text-weight-bold cursor-pointer"
                      @click="setActiveRowIdInLocalStorage(props.row.key); $router.push({ path: '/requirement-center', state: { requirementId: props.row.requirementId } })"
                    >
                      <span v-if="props.row.requirementNumber">#{{ props.row.requirementNumber }} - </span>{{ props.row.requirementTitle }}
                      <q-tooltip>Requirement Center</q-tooltip>
                    </span>
                    <q-badge v-if="props.row.isRequirementUnavailable" color="warning">
                      Moved or deleted
                      <q-tooltip>This requirement was deleted or moved out of the module after the rows were saved.</q-tooltip>
                    </q-badge>
                    <span class="text-caption text-grey-8">
                      <span class="cursor-pointer" @click="onProjectView(props.row.projectId)">{{ props.row.projectName }}<q-tooltip>View Project</q-tooltip></span>
                      /
                      <span class="cursor-pointer" @click="onProjectModuleView(props.row.projectModuleId)">{{ props.row.projectModuleName }}<q-tooltip>View Module</q-tooltip></span>
                    </span>
                    <q-badge color="grey-7" outline>{{ props.row.rowCount }} tracking record(s)</q-badge>
                  </div>
                </q-td>
                <q-td auto-width class="text-center actions requirement-group-cell">
                  <q-icon
                    :name="isViewer ? 'o_visibility' : 'o_edit'"
                    class="cursor-pointer q-mr-sm"
                    @click="onEditGroup(props.row)"
                  >
                    <q-tooltip>{{ isViewer ? "View" : "Edit" }} all tracking records of this requirement</q-tooltip>
                  </q-icon>
                  <q-icon v-can
                    v-if="!isViewer"
                    name="o_delete_outline"
                    class="cursor-pointer"
                    color="negative"
                    @click="onDeleteGroup(props.row)"
                  >
                    <q-tooltip>Delete all tracking records of this requirement</q-tooltip>
                  </q-icon>
                </q-td>
              </q-tr>
              <!-- Tracking record of the requirement above -->
              <q-tr v-else :props="props" :class="highlightedId === props.row.id ? 'highlight' : ''">
                <q-td v-if="selectedColumnNames.includes('trackingDate')" class="common-q-td text-center">
                  {{ props.row.trackingDate }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('currentPercent')" class="common-q-td text-right">
                  {{ formatNumber(props.row.currentPercent) }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('timeSpentAsOfToday')" class="common-q-td text-right">
                  {{ props.row.timeSpentAsOfToday }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('estimatedRemainingTime')" class="common-q-td text-right">
                  {{ props.row.estimatedRemainingTime }}
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('scope')"
                  class="common-q-td"
                  :style="{ width: (resizeWidths?.scope || 200) + 'px', minWidth: '80px', maxWidth: (resizeWidths?.scope || 200) + 'px' }"
                >
                  <div class="content-ellipsis">
                    {{ htmlToText(props.row.scope) }}
                    <q-tooltip v-if="htmlToText(props.row.scope)" max-width="500px">{{ htmlToText(props.row.scope) }}</q-tooltip>
                  </div>
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('roadblock')"
                  class="common-q-td"
                  :style="{ width: (resizeWidths?.roadblock || 200) + 'px', minWidth: '80px', maxWidth: (resizeWidths?.roadblock || 200) + 'px' }"
                >
                  <div class="content-ellipsis">
                    {{ htmlToText(props.row.roadblock) }}
                    <q-tooltip v-if="htmlToText(props.row.roadblock)" max-width="500px">{{ htmlToText(props.row.roadblock) }}</q-tooltip>
                  </div>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('estimatedTimeForCompletion')" class="common-q-td text-right">
                  {{ props.row.estimatedTimeForCompletion }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('createdByName')" class="common-q-td">{{ props.row.createdByName }}</q-td>
                <q-td v-if="selectedColumnNames.includes('createdOnUtc')" class="common-q-td">{{ props.row.createdOnUtc }}</q-td>
                <q-td v-if="selectedColumnNames.includes('updatedByName')" class="common-q-td">{{ props.row.updatedByName }}</q-td>
                <q-td v-if="selectedColumnNames.includes('updatedOnUtc')" class="common-q-td">{{ props.row.updatedOnUtc }}</q-td>
                <!-- Actions are on the requirement row above (Edit opens all of the requirement's records). -->
                <q-td auto-width class="text-center actions" />
              </q-tr>
              <q-seperator />
            </template>
          </q-table>
        </div>
      </div>
    </q-card>
  </q-page>
  <!-- Multi-Column Level Sorting (requirement-group level) -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="sortableColumns"
    :multi-sort="multiSort"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { useAuthStore } from "stores/auth";
import { notifyError } from "assets/utils";

import requirementStatusTrackingService from "../requirementStatusTracking.service";
import { groupRequirementRows, toggleRecordSelection, buildListPayload, formatNumber, htmlToText, keepAvailable, dateRangeErrors, toApiDate, SORT_OPTIONS } from "../utils/trackingList.js";

// Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

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
  initRequirementStatusTrackingDialogs,
  onAddRequirementStatusTracking,
  onEditRequirementStatusTracking
} from "src/modules/requirement-status-tracking/utils/dialogs.js";

import {
  onSubmitRequirementStatusTrackingDeleteForRequirement,
  onSubmitRequirementStatusTrackingBulkDelete
} from "src/modules/requirement-status-tracking/utils/actions.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// Shared Project Module Dialogs
import {
  initProjectModuleDialogs,
  onProjectModuleView
} from "src/modules/project-modules/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const authStore = useAuthStore();
const user = authStore.user;
const loading = ref(true);
const loadError = ref(false);
const showFilter = ref(false);
const searchLoader = ref(false);
const showSortDialog = ref(false);
const showMultiActions = ref(false);
const shownProjects = new Set();
const isViewer = user?.roles?.some(r => r?.toLowerCase() === "viewer") ?? false;


const highlightedId = computed(() => { return activeRowId.value; });

function setActiveRowIdInLocalStorage(id) {
  activeRowId.value = id;

  saveDataTableState({
    activeRowId: id
  });
}

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
// Requirement groups from the API, each with its tracking records (newest tracking date first).
const records = ref([]);
// Columns of a tracking record; the Requirement Title is shown on the group header row above its records.
// Sorting orders requirement groups: Tracking Date = latest tracking date, Created/Updated Date = group's earliest/latest.
const columns = ref([
  { name: "trackingDate", label: "Tracking Date", field: "trackingDate", align: "center", sortable: true, default: true },
  { name: "currentPercent", label: "Current Completion (%)", field: "currentPercent", align: "right", sortable: false, default: true },
  { name: "timeSpentAsOfToday", label: "Time Spent (Hrs)", field: "timeSpentAsOfToday", align: "right", sortable: false, default: true, tooltip: "Time Spent as of Today (Hours)" },
  { name: "estimatedRemainingTime", label: "Est. Remaining (Hrs)", field: "estimatedRemainingTime", align: "right", sortable: false, default: true, tooltip: "Estimated Remaining Time Required (Hours)" },
  { name: "scope", label: "Scope", field: "scope", align: "left", sortable: false, default: true, tooltip: "Additional Scope" },
  { name: "roadblock", label: "Roadblock", field: "roadblock", align: "left", sortable: false, default: true },
  { name: "estimatedTimeForCompletion", label: "Est. Completion Time (Hrs)", field: "estimatedTimeForCompletion", align: "right", sortable: false, default: true, tooltip: "Estimated Time for Completion of Additional Scope (Hours)" },
  { name: "createdByName", label: "Created By", field: "createdByName", align: "left", sortable: false, default: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true, default: true },
  { name: "updatedByName", label: "Updated By", field: "updatedByName", align: "left", sortable: false, default: true },
  { name: "updatedOnUtc", label: "Updated Date", field: "updatedOnUtc", align: "left", sortable: true, default: true }
]);
// Multi-sort dialog: requirement-group sort keys (incl. Requirement Title, Project, Module).
const sortableColumns = SORT_OPTIONS;

// Header row per requirement + its tracking records, in the server's final sort/filter/page order.
const displayRows = computed(() => groupRequirementRows(records.value));

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Tracking Records (paged by complete record)
// ----------------------------------------------------------------------------------------------------------------

const getAllRequirementStatusTracking = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  const { payload, error } = buildListPayload(search.value, { page, rowsPerPage, sortBy, descending }, multiSort.value);
  if (error) {
    notifyError({ message: error });
    searchLoader.value = false;
    return;
  }

  loading.value = true;
  saveDataTableState({
    search: search.value,
    pagination: p,
    activeRowId: activeRowId.value,
    sorts: payload.sorts
  });

  try {
    const resp = await requirementStatusTrackingService.getAllRequirementStatusTracking(payload);
    records.value = resp?.requirements || [];
    loadError.value = false;
    pagination.value = { ...pagination.value, page, rowsPerPage, sortBy, descending, rowsNumber: resp?.total || 0 };
  } catch (err) {
    // The http interceptor shows the server error notification.
    records.value = [];
    loadError.value = true;
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
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
  // Tracking-specific key so column/sort/width settings never mix with Requirement, Issue or Data Mapping lists.
  // New key for the requirement-grouped layout so saved column/sort settings of the old layout don't hide columns.
  storageKey: "requirement-status-tracking-grouped-Index",
  siteId: user?.siteId,

  defaultSearch: {
    searchText: "",
    projectIds: [],
    projectModuleIds: [],
    requirementIds: [],
    fromDate: "",
    toDate: ""
  },

  defaultPagination: {
    sortBy: "updatedOnUtc",
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

const lsSorts = sorts.value || null;

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
    refreshList();
  }
});

// function resetTracking () {
//   shownProjects.clear(); // Clear the set before rendering rows
// }

function isFirstProjectRow(row) {
  const currentIndex = displayRows.value.findIndex(
    item => item.id === row.id
  );

  if (currentIndex === -1) return false;

  if (currentIndex === 0) return true;

  const currentProjectId = row.projectId;
  const previousProjectId = displayRows.value[currentIndex - 1]?.projectId;

  return currentProjectId !== previousProjectId;
}

const groupedTrackingRecords = computed(() => {
  const groups = {}

  displayRows.value.forEach(item => {
    const key = `${item.projectId}_${item.projectModuleId}_${item.trackingDate}`

    if (!groups[key]) groups[key] = []
    groups[key].push(item)
  })

  return Object.values(groups).flatMap(group =>
    group.map((item, index) => ({
      ...item,
      isRecordStart: index === 0,
      recordRowCount: group.length
    }))
  )
})

const getAdditionalScope = (htmlText) => {
  if (!htmlText) return "";

  const textarea = document.createElement("textarea");

  textarea.innerHTML = htmlText
    .replace(/<[^>]*>/g, " ")
    .replace(/&nbsp;/g, " ");

  return textarea.value
    .replace(/\s+/g, " ")
    .trim();
};

const refreshList = () => {
  getAllRequirementStatusTracking({ pagination: pagination.value });
};

// Filters and search start again from page 1.
const reloadFromFirstPage = () => {
  pagination.value = { ...pagination.value, page: 1 };
  refreshList();
};

const sortColumn = (col) => {
  if (!col.sortable) return;
  if (pagination.value.sortBy === col.name) {
    pagination.value.descending = !pagination.value.descending;
  } else {
    pagination.value.sortBy = col.name;
    pagination.value.descending = false;
  }
  refreshList();
};

const onAdvanceSearch = () => {
  reloadFromFirstPage();
};

const onAdvanceClear = () => {
  search.value.projectIds = [];
  search.value.projectModuleIds = [];
  search.value.requirementIds = [];
  search.value.fromDate = "";
  search.value.toDate = "";
  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};

// ----------------------------------------------------------------------------------------------------------------
// Selection & record actions (always complete-record ids)
// ----------------------------------------------------------------------------------------------------------------

const selectedRecordIds = ref([]);

const onToggleRecord = (recordId) => {
  selectedRecordIds.value = toggleRecordSelection(selectedRecordIds.value, recordId);
};

const clearSelection = () => {
  selectedRecordIds.value = [];
  showMultiActions.value = false;
};

// Edit opens the requirement's tracking form (all its tracking dates), as the Requirement list does.
const onEditGroup = (group) => {
  activeRowId.value = group.key;
  onEditRequirementStatusTracking(group.projectId, group.projectModuleId, group.requirementId, refreshList);
};

// Delete removes all tracking records of the requirement (individual records are removed inside Edit).
const onDeleteGroup = (group) => {
  activeRowId.value = group.key;
  onSubmitRequirementStatusTrackingDeleteForRequirement(group, (deleted) => {
    if (deleted) selectedRecordIds.value = [];
    refreshList();
  });
};

// One request for all selected records; on failure nothing was deleted, so the selection is kept.
const onBulkDelete = () => {
  onSubmitRequirementStatusTrackingBulkDelete(selectedRecordIds.value, (deleted) => {
    if (deleted) selectedRecordIds.value = [];
    refreshList();
  });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs
// ------------------------------------------------------------------------------------
initRequirementStatusTrackingDialogs();
initProjectDialogs(activeRowId);
initProjectModuleDialogs(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// ----------------------------------------------------------------------------------------------------------------

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
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Module"),
  ...mapFilterToLabel(search.value.requirementIds, requirementsByProjectModuleIdForDropdown.list, "Requirement"),
  ...(search.value.fromDate ? { "From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "To Date": search.value.toDate } : {})
}));

function onClearFilters (key) {
  if (key === "Project Name") {
    // Dependent filters belong to the selected projects, so they are cleared too.
    search.value.projectIds = [];
    search.value.projectModuleIds = [];
    search.value.requirementIds = [];
  } else if (key === "Project Module") {
    search.value.projectModuleIds = [];
    search.value.requirementIds = [];
  } else if (key === "Requirement") {
    search.value.requirementIds = [];
  } else if (key === "From Date") {
    search.value.fromDate = "";
  } else if (key === "To Date") {
    search.value.toDate = "";
  }
  reloadFromFirstPage();
}

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Requirement": return search.value.requirementIds?.length || 0;
  default: return null;
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectNameDropdown } = projectModule();
const { projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { requirementsByProjectModuleIdForDropdown } = requirementModule();

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  reloadFromFirstPage();
});

// Project Module options follow the selected projects; module selections that no longer belong to a selected
// project are dropped (selections that still apply are kept). Stale responses are ignored.
let moduleLoadSeq = 0;
watch(() => [...(search.value.projectIds || [])], async (projectIds) => {
  const seq = ++moduleLoadSeq;
  if (!projectIds.length) {
    projectModulesByProjectIdForDropdown.list.value = [];
    search.value.projectModuleIds = [];
    return;
  }
  await projectModulesByProjectIdForDropdown.load(false, false, projectIds);
  if (seq !== moduleLoadSeq) return;
  search.value.projectModuleIds = keepAvailable(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list.value);
}, { immediate: true });

// Requirement options follow the selected projects AND project modules (all requirement types); requirement
// selections outside them are dropped.
let requirementLoadSeq = 0;
watch(() => [...(search.value.projectModuleIds || []), "|", ...(search.value.projectIds || [])], async () => {
  const seq = ++requirementLoadSeq;
  const moduleIds = search.value.projectModuleIds || [];
  if (!moduleIds.length) {
    requirementsByProjectModuleIdForDropdown.list.value = [];
    search.value.requirementIds = [];
    return;
  }
  await requirementsByProjectModuleIdForDropdown.load(moduleIds.join(","), (search.value.projectIds || []).join(","));
  if (seq !== requirementLoadSeq) return;
  search.value.requirementIds = keepAvailable(search.value.requirementIds, requirementsByProjectModuleIdForDropdown.list.value);
}, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- From Date / To Date (Tracking Date range, both inclusive)
// ----------------------------------------------------------------------------------------------------------------
const fromDateProxy = ref(null);
const toDateProxy = ref(null);
const fromDateError = computed(() => dateRangeErrors(search.value.fromDate, search.value.toDate).fromDate);
const toDateError = computed(() => dateRangeErrors(search.value.fromDate, search.value.toDate).toDate);
// Calendar options: From Date cannot be after To Date and vice versa (q-date passes YYYY/MM/DD).
const fromDateOptions = (date) => !toApiDate(search.value.toDate) || date.replace(/\//g, "-") <= toApiDate(search.value.toDate);
const toDateOptions = (date) => !toApiDate(search.value.fromDate) || date.replace(/\//g, "-") >= toApiDate(search.value.fromDate);

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  projectNameDropdown.load();
  tableRef.value.requestServerInteraction();
});
</script>
<style scoped>
.table-tracking .Custom-DataTable {
  min-width: max-content;
}
/* .table-tracking .record-start td {
  border-top: 2px solid var(--q-primary);
} */
.content-cell {
  overflow: hidden;
  white-space: nowrap;
}
.requirement-group-cell {
  background: #dbf2ff;
  border-top: 2px solid var(--q-primary);
}
.content-ellipsis {
  display: block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
