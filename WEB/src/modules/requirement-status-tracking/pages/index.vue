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
                          <label class="Cutomlabel q-mt-sm fs-13">Date From</label>
                        </div>
                        <formDate v-model="search.fromDate" :required="false" wrapper-class="col-lg-7 col-md-7 col-sm-12 col-xs-12 q-px-sm" />
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Date To</label>
                        </div>
                        <formDate v-model="search.toDate" :required="false" wrapper-class="col-lg-7 col-md-7 col-sm-12 col-xs-12 q-px-sm" />
                      </div>
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onAdvanceSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onAdvanceClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <div class="row items-center q-gutter-sm q-ml-xs no-wrap">
                <q-btn
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
                <q-th v-if="!isViewer" auto-width class="text-center hidden">
                  <q-btn
                    v-if="selectedRecordIds.length > 0"
                    icon="o_restart_alt"
                    flat
                    dense
                    no-caps
                    color="white"
                    size="sm"
                    @click.stop="clearSelection"
                  >
                    <q-tooltip>Unselect All Tracking Records</q-tooltip>
                  </q-btn>
                </q-th>
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
                      {{ pagination.sortBy === col.name ? (pagination.descending ? 'Sort Ascending' : 'Sort Descending') : 'Sort' }}
                    </q-tooltip>
                  </q-icon>
                  <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
                </q-th>
                <q-th auto-width class="text-center">Actions</q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr
                :props="props"
                :class="[props.row.isRecordStart ? 'record-start' : '', highlightedId === props.row.recordId ? 'highlight' : '']"                
                :set="(preProjectName = null, preProjectModuleName = null)"
              >
                <q-td v-if="!isViewer" auto-width class="text-center hidden">
                  <!-- Every row of a record shares one checkbox state: selection is by complete record. -->
                  <q-checkbox
                    dense
                    :model-value="selectedRecordIds.includes(props.row.recordId)"
                    :aria-label="`Select tracking record ${props.row.projectName} / ${props.row.projectModuleName} / ${props.row.trackingDate}`"
                    @update:model-value="onToggleRecord(props.row.recordId)"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('projectName')" class="common-q-td hoverable-cell">
                  <div class="row no-wrap items-center justify-between">
                    <span
                      v-if="preProjectName !== props.row.projectName"
                      :set="preProjectName = props.row.projectName"
                      class="cursor-pointer hoverable-cell"
                      @click="onProjectView(props.row.projectId)"
                    >
                      {{ props.row.projectName }}
                    </span>
                    <div
                      v-if="shouldShowIcons(props.row.projectName, index)"
                      class="row items-center q-gutter-sm q-ml-sm"
                      style="flex-shrink: 0;"
                    >
                      <q-icon
                        name="o_radio_button_checked"
                        size="xs"
                        class="cursor-pointer"
                        @click="
                          setActiveRowIdInLocalStorage(props.row.id);
                          $router.push({
                            path: '/project-center',
                            state: { projectId: props.row.projectId }
                          })
                        "
                      >
                        <q-tooltip>Project Center</q-tooltip>
                      </q-icon>
                    </div>
                  </div>
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('projectModuleName')"
                  class="common-q-td hoverable-cell"
                  @click="onProjectModuleView(props.row.projectModuleId)"
                >
                  <span
                  v-if="preProjectModuleName !== props.row.projectModuleName"
                  :set="preProjectModuleName = props.row.projectModuleName"
                  >
                    {{ props.row.projectModuleName }}
                  </span>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('trackingDate')" class="common-q-td text-center">
                  {{ props.row.trackingDate }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('requirementTitle')" class="common-q-td">
                  <span v-if="props.row.requirementNumber">#{{ props.row.requirementNumber }} </span>{{ props.row.requirementTitle }}
                  <q-badge v-if="props.row.isRequirementUnavailable" color="warning" class="q-ml-xs">
                    Moved or deleted
                    <q-tooltip>This requirement was deleted or moved out of the module after the row was saved.</q-tooltip>
                  </q-badge>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('currentPercent')" class="common-q-td text-right">
                  {{ formatNumber(props.row.currentPercent) }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('timeSpentAsOfToday')" class="common-q-td text-right">
                  {{ formatNumber(props.row.timeSpentAsOfToday) }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('estimatedRemainingTime')" class="common-q-td text-right">
                  {{ formatNumber(props.row.estimatedRemainingTime) }}
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('scope')" class="common-q-td" style="white-space: pre-wrap;"
                  :style="{
                      width: (resizeWidths?.scope || 120) + 'px',
                      minWidth: '80px',
                      maxWidth: (resizeWidths?.scope || 120) + 'px'
                    }"
                >
                  <div class="content-ellipsis">
                    {{
                      getAdditionalScope(props.row.scope)
                    }}
                    <q-tooltip>{{ props.row.scope }}</q-tooltip>
                  </div>                    
                </q-td>
                <q-td v-if="selectedColumnNames.includes('estimatedTimeForCompletion')" class="common-q-td text-right">
                  {{ formatNumber(props.row.estimatedTimeForCompletion) }}
                </q-td>
                <!-- <q-td v-if="selectedColumnNames.includes('updatedOnUtc')" class="common-q-td">
                  {{ props.row.updatedOnUtc }}
                </q-td>                 -->
                <q-td v-if="selectedColumnNames.includes('createdById')">{{ props.row.createdByName }}</q-td>
                <q-td v-if="selectedColumnNames.includes('createdOnUtc')">{{ props.row.createdOnUtc }}</q-td>
                <q-td v-if="selectedColumnNames.includes('updatedById')">{{ props.row.updatedByName }}</q-td>
                <q-td v-if="selectedColumnNames.includes('updatedOnUtc')">{{ props.row.updatedOnUtc }}</q-td>
                <q-td auto-width class="text-center actions">
                  <!-- Record actions sit on the record's first row so their complete-record scope is clear. -->
                  <template v-if="props.row.isRecordStart">
                    <q-icon
                      :name="isViewer ? 'o_visibility' : 'o_edit'"
                      class="cursor-pointer q-mr-sm"
                      @click="onEditRecord(props.row)"
                    >
                      <q-tooltip>{{ isViewer ? "View" : "Edit" }} tracking record ({{ props.row.recordRowCount }} requirement row(s))</q-tooltip>
                    </q-icon>
                    <q-icon
                      v-if="!isViewer"
                      name="o_delete_outline"
                      class="cursor-pointer"
                      color="negative"
                      @click="onDeleteRecord(props.row)"
                    >
                      <q-tooltip>Delete tracking record</q-tooltip>
                    </q-icon>
                  </template>
                </q-td>
              </q-tr>
              <q-separator />
            </template>
          </q-table>
        </div>
      </div>
    </q-card>
  </q-page>
  <!-- Multi-Column Level Sorting (record-level columns only) -->
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
import { flattenRecords, toggleRecordSelection, buildListPayload, formatNumber, SORTABLE_COLUMNS } from "../utils/trackingList.js";

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
  onSubmitRequirementStatusTrackingDelete,
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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const records = ref([]);
// Sortable columns use the API's record-level sort keys as both name and field (multi-sort uses field).
const columns = ref([
  { name: "projectName", label: "Project Name", field: "project.name", align: "left", sortable: true, default: true },
  { name: "projectModuleName", label: "Module Name", field: "projectModule.name", align: "left", sortable: true, default: true },
  { name: "trackingDate", label: "Date", field: "trackingDate", align: "center", sortable: true, default: true },
  { name: "requirementTitle", label: "Requirement Name", field: "requirementTitle", align: "left", sortable: true, default: true },
  { name: "currentPercent", label: "Current Completion (%)", field: "currentPercent", align: "right", sortable: true, default: true },
  { name: "timeSpentAsOfToday", label: "Time Spent as of Now (Hours)", field: "timeSpentAsOfToday", align: "right", sortable: true, default: true },
  { name: "estimatedRemainingTime", label: "Estimated Remaining Time Required (Hours)", field: "estimatedRemainingTime", align: "right", sortable: true, default: true },
  { name: "scope", label: "Additional Scope", field: "scope", align: "left", sortable: false, default: true },
  { name: "estimatedTimeForCompletion", label: "Estimated Time for Completion of Additional Scope (Hours)", field: "estimatedTimeForCompletion", align: "right", sortable: true, default: true },
  { name: "createdByName", label: "Created By", field: "createdById", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true, default: false },
  { name: "updatedByName", label: "Updated By", field: "updatedById", align: "left", sortable: true, default: false },
  { name: "updatedOnUtc", label: "Updated Date", field: "updatedOnUtc", align: "left", sortable: true, default: false }
]);
const sortableColumns = computed(() => columns.value.filter(col => SORTABLE_COLUMNS.includes(col.field)));

// Grouping is computed after the server's final sort/filter/page order, never by re-sorting.
// The table renders one row per requirement row; records carry their requirement values only inside `rows`.
const displayRows = computed(() => flattenRecords(records.value));

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
    console.log("resp", resp);
    records.value = resp?.records || [];
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
  storageKey: "requirement-status-tracking-Index",
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

function shouldShowIcons (projectName) {
  if (shownProjects.has(projectName)) {
    return false;
  } else {
    shownProjects.add(projectName);
    return true;
  }
}

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

const onEditRecord = (row) => {
  activeRowId.value = row.recordId;
  onEditRequirementStatusTracking(row.recordId, isViewer, refreshList);
};

const onDeleteRecord = (row) => {
  activeRowId.value = row.recordId;
  onSubmitRequirementStatusTrackingDelete(row, (deleted) => {
    if (deleted) selectedRecordIds.value = selectedRecordIds.value.filter(id => id !== row.recordId);
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
  ...(search.value.fromDate ? { "Date From": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "Date To": search.value.toDate } : {})
}));

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Project Module") {
    search.value.projectModuleIds = [];
    search.value.requirementIds = [];
  } else if (key === "Requirement") {
    search.value.requirementIds = [];
  } else if (key === "Date From") {
    search.value.fromDate = "";
  } else if (key === "Date To") {
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

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (newValue === oldValue) return;
  if (!newValue?.length) {
    search.value.projectModuleIds = [];
    return;
  }
  const isInitialLoad = !oldValue;
  await projectModulesByProjectIdForDropdown.load(false, false, newValue);
  // Clear modules only when the project was changed by the user.
  if (!isInitialLoad) search.value.projectModuleIds = [];
}, { immediate: true });

// Requirement options follow the selected modules (all requirement types).
watch(() => [...(search.value.projectModuleIds || [])], async (newValue) => {
  if (newValue.length === 0) {
    search.value.requirementIds = [];
    return;
  }
  await requirementsByProjectModuleIdForDropdown.load(newValue);
}, { immediate: true });

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
.content-ellipsis {
  display: block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
