<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Project Management" />
              <q-breadcrumbs-el label="Project Action Items" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-4">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center q-mr-xs">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    class="search-bar"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.requirementIds"
                        label="Requirement"
                        :disable="!search.projectIds"
                        :options="requirementsByProjectModuleIdForDropdown.list.value"
                        :filter="requirementsByProjectModuleIdForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Title</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.title"
                            fill-input
                            class="q-mx-sm w-100 h-auto"
                            :dense="true"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.customerIds"
                        label="Customer"
                        :options="customerNameDropdown.list.value"
                        :filter="customerNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.employeeIds"
                        label="Employee"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.priorityIds"
                        label="Priority"
                        :options="projectActionItemPriorityForDropdown.list.value"
                        :filter="projectActionItemPriorityForDropdown.filter"
                        :isShowAll="true"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Due Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-ml-sm w-100 h-auto">
                            <q-input
                              v-model="search.dueDate"
                              fill-input
                              dense
                              mask="##/##/####"
                            >
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="search.dueDate"
                                      mask="MM/DD/YYYY"
                                      @update:model-value="() => $refs.qDateProxy.hide()"
                                    />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
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
              <div class="q-ml-xs">
                <q-btn
                  icon="o_add"
                  outline
                  label="Create Project Action Item"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onProjectActionItemsAdd(refreshProjectActionItemsList)"
                />
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
                <!-- Button to Open Sorting Dialog -->
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
      <div class="table-project-action">
        <div class="table-scroll-container">
          <q-table
            ref="tableRef"
            v-model:pagination="pagination"
            :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
            :loading="loading"
            :rows="rows"
            :columns="computedColumns"
            row-key="id"
            separator="cell"
            no-data-label="No data available"
            binary-state-sort
            :rows-per-page-options="[20, 50, 100, 200, 500]"
            @request="getAllProjectActionItems"
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
                  :props="props"
                  :style="{
                    width: (resizeWidths?.[col.name] || 120) + 'px',
                    minWidth: '80px',
                    position: 'relative'
                  }"
                  @click="!isResizing && col.sortable"
                >
                  {{ col.label }}
                  <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
                </q-th>
                <q-th auto-width class="text-center">Actions</q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr
                :props="props"
                :class="highlightedId == props.row.id ? 'highlight' : ''"
                :set="(preProjectName = null, preTestPlanName = null, resetTracking())"
              >
                <q-td auto-width class="text-center hidden">
                  <q-icon
                    :name="isExpanded(props.row.id) ? '-' : '+'"
                    class="cursor-pointer custom-plus-minus-icon"
                    @click="toggleExpand(props.row.id)"
                  >
                    <q-tooltip>{{ isExpanded(props.row.id) ? 'Collapse' : 'Expand' }}</q-tooltip>
                  </q-icon>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('project.name')" style="white-space: normal;" class="hoverable-cell">
                  <div class="row no-wrap items-center justify-between">
                    <span style="flex: 1; word-break: break-word; white-space: normal;">
                      <span
                        v-if="preProjectName !== props.row.project.name"
                        :set="preProjectName = props.row.project.name"
                        @click="onProjectView(props.row.project.id)"
                      >{{ props.row.project.name }}
                      </span>
                    </span>
                    <div
                      v-if="shouldShowIcons(props.row.project.name, index)"
                      class="row items-center q-gutter-sm q-ml-sm"
                      style="flex-shrink: 0;"
                    >
                      <q-icon
                        name="o_radio_button_checked" size="xs"
                        class="cursor-pointer"
                        @click="setActiveRowIdInLocalStorage(props.row.id);
                                $router.push({ path: '/project-center', state: { projectId: props.row.project.id } })"
                      >
                        <q-tooltip>Project Center</q-tooltip>
                      </q-icon>
                      <q-icon
                        name="o_developer_board hidden" size="xs"
                        class="cursor-pointer"
                        @click="setActiveRowIdInLocalStorage(props.row.id);
                                $router.push({ path: '/project-planning/workboard', state: {projectId: props.row.project.id } })"
                      >
                        <q-tooltip>Work Board</q-tooltip>
                      </q-icon>
                    </div>
                  </div>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('requirement.title')"
                  class="common-q-td hoverable-cell"
                  style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"
                  @click="onRequirementView(props.row.requirement?.id)"
                >
                  <span v-if="props.row.requirement?.title">
                    {{ props.row.requirement?.title }}
                  </span>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('title')" class="hoverable-cell" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.title }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('customer.name')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.customer.name }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('employee.person.fullName')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.employee.person.fullName }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('dueDate')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.dueDate }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('priority.dropDownValue')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.priority.dropDownValue }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('testedByEmployee.person.firstName')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.testedByEmployee.person.fullName }}
                </q-td>
              <q-td
                v-if="selectedColumnNames.includes('createdBy.person.firstName')"
                class="common-q-td"
              >
                {{ props.row.createdBy.person.fullName }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('createdOnUtc')"
                class="common-q-td"
              >
                {{ props.row.createdOnUtc }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('updatedBy.person.firstName')"
                class="common-q-td"
              >
                {{ props.row.updatedBy.person.fullName }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('updatedOnUtc')"
                class="common-q-td"
              >
                {{ props.row.updatedOnUtc }}
              </q-td>
                <q-td class="text-center actions">
                  <q-icon
                    name="o_visibility"
                    class="cursor-pointer q-mr-sm"
                    size="xs"
                    @click="onProjectActionItemsView(props.row.id)"
                  >
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_edit"
                    class="cursor-pointer q-mr-sm"
                    size="xs"
                    @click="onProjectActionItemsEdit(props.row.id, refreshProjectActionItemsList)"
                  >
                    <q-tooltip>Edit</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_delete_outline"
                    class="cursor-pointer"
                    color="negative"
                    size="xs"
                    @click="onSubmitProjectActionItemsDelete(props.row.id, props.row.title, refreshProjectActionItemsList)"
                  >
                    <q-tooltip>Delete</q-tooltip>
                  </q-icon>
                </q-td>
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
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";

import projectActionItemsService from "modules/project-action-items/projectActionItems.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectActionItemModule from "src/modules/project-action-items/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectActionItemsDialogs,
  onProjectActionItemsView,
  onProjectActionItemsAdd,
  onProjectActionItemsEdit
} from "src/modules/project-action-items/utils/dialogs.js";

import {
  initRequirementDialogs,
  onRequirementView
} from "src/modules/requirement/utils/dialogs.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// Shared project action items
import {
  initProjectActionItemsActions,
  onSubmitProjectActionItemsDelete
} from "src/modules/project-action-items/utils/actions.js";

// Common variables
const expandedRows = ref([]);
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const showSortDialog = ref(false);
const { toDate } = useFilters();
const currentSiteId = computed(() => user?.siteId || null);

// Table variables
const rows = ref([]);
const shownProjects = new Set();
const columns = ref([
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true, default: true },
  { name: "requirement.title", label: "Requirement", field: "requirement.title", align: "left", sortable: true, default: true },
  { name: "title", label: "Title", field: "title", align: "left", sortable: true, default: true },
  { name: "customer.name", label: "Customer", field: "customer.name", align: "left", sortable: true, default: true },
  { name: "employee.person.fullName", label: "Employee", field: "employee.person.fullName", align: "left", sortable: true, default: true },
  { name: "dueDate", label: "Due Date", field: "dueDate", align: "left", sortable: true, default: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true, default: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "center", sortable: true, default: true },
  { name: "updatedBy.person.firstName", label: "Updated By", field: "updatedBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "left", sortable: true, default: false }
]);

const getAllProjectActionItems = async ({ pagination: p }) => {
  try {
    loading.value = true;
    const { page, rowsPerPage, sortBy, descending } = p;

    search.value.dueDate = search.value.dueDate
      ? toDate(search.value.dueDate)
      : null;

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

    const resp = await projectActionItemsService.getAllProjectActionItems(payload);

    rows.value = resp.projectActionItemList.map(items => {
      return {
        ...items
      };
    });

    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });

    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: activeRowId.value
    });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

const defaultSearch = {
  searchText: "",
  projectIds: [],
  requirementIds: [],
  priorityIds: [],
  dueDate: "",
  title: "",
  customerIds: [],
  employeeIds: []
};

const defaultPagination = {
  sortBy: "createdOnUtc",
  descending: true,
  rowsPerPage: 20,
  page: 1
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
  storageKey: "project-Action-Items-Index",
  siteId: currentSiteId,

  defaultSearch,
  defaultPagination,
  defaultSorts: {},
  defaultResizableWidth: {},

  defaultColumns: columns.value
    .filter(col => col.default === true)
    .map(col => col.name)
});

if (history.state?.projectId) {
  search.value.projectIds = Array.isArray(history.state.projectId)
    ? history.state.projectId
    : [history.state.projectId];
}

if (history.state?.requirementId) {
  search.value.requirementIds = Array.isArray(history.state.requirementId)
    ? history.state.requirementId
    : [history.state.requirementId];
}

const highlightedId = computed(() => {
  return activeRowId.value;
});

function setActiveRowIdInLocalStorage(id) {
  activeRowId.value = id;

  saveDataTableState({
    activeRowId: id
  });
}

const handleDocumentClick = (event) => {
  if (event.target.closest(".q-dialog")) {
    return;
  }

  const highlightElement = document.querySelector(".highlight");

  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;

    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: null
    });
  }
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshProjectActionItemsList = () => {
  getAllProjectActionItems({ pagination: pagination.value });
};

const isExpanded = (rowId) => {
  return expandedRows.value.includes(rowId);
};

const toggleExpand = (rowId) => {
  if (expandedRows.value.includes(rowId)) {
    expandedRows.value = expandedRows.value.filter(id => id !== rowId);
  } else {
    expandedRows.value.push(rowId);
  }
};

function resetTracking () {
  shownProjects.clear(); // Clear the set before rendering rows
}

function shouldShowIcons (projectName) {
  if (shownProjects.has(projectName)) {
    return false;
  } else {
    shownProjects.add(projectName);
    return true;
  }
}

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshProjectActionItemsList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.projectIds = [];
  search.value.requirementIds = [];
  search.value.priorityIds = [];
  search.value.title = "";
  search.value.customerIds = [];
  search.value.employeeIds = [];
  search.value.dueDate = null;
  saveDataTableState({
    search: {
      ...defaultSearch
    }
  });
  onAdvanceSearch();
};

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
    refreshProjectActionItemsList();
  }
});

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initProjectActionItemsDialogs(activeRowId);
initRequirementDialogs(activeRowId);
initProjectDialogs(activeRowId);
initProjectActionItemsActions(activeRowId);

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
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.requirementIds, requirementsByProjectModuleIdForDropdown.list, "Requirement"),
  ...mapFilterToLabel(search.value.priorityIds, projectActionItemPriorityForDropdown.list, "Priority"),
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Employee"),
  ...(search.value.title ? { "Title": search.value.title } : {}),
  ...(search.value.assignedTo ? { "Assigned To": search.value.assignedTo } : {}),
  ...(search.value.dueDate ? { "Due Date": search.value.dueDate } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Requirement": return search.value.requirementIds?.length || 0;
  case "Priority": return search.value.priorityIds?.length || 0;
  case "Customer": return search.value.customerIds?.length || 0;
  case "Employee": return search.value.employeeIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Requirement") {
    search.value.requirementIds = [];
  } else if (key === "Priority") {
    search.value.priorityIds = [];
  } else if (key === "Title") {
    search.value.title = "";
  } else if (key === "Customer") {
    search.value.customerIds = [];
  } else if (key === "Employee") {
    search.value.employeeIds = [];
  } else if (key === "Due Date") {
    search.value.dueDate = "";
  }

  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    activeRowId: activeRowId.value
  });
  refreshProjectActionItemsList();
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const { projectNameDropdown } = projectModule();

const {
  projectActionItemPriorityForDropdown
} = projectActionItemModule();

const { requirementsByProjectModuleIdForDropdown } = requirementModule();
const { activeEmployeesDropdown } = employeeModule();
const { customerNameDropdown } = customerModule();

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshProjectActionItemsList();
});

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

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  search.value.requirementIds = [];
  requirementsByProjectModuleIdForDropdown.load('', newValue);
}, { immediate: true });

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(async () => {
  refreshProjectActionItemsList();
  projectNameDropdown.load();
  customerNameDropdown.load();
  activeEmployeesDropdown.load();
  if (search.value.projectIds.length > 0) requirementsByProjectModuleIdForDropdown.load('', search.value.projectIds);
  await projectActionItemPriorityForDropdown.load("Project Action Item Priority");
  
  // const setPriority = projectActionItemPriorityForDropdown.getValuesByLabels(["Medium"]);
  // if (setPriority.length && !search.value.priorityIds?.length) {
  //   search.value.priorityIds = setPriority;
  // }

  document.addEventListener("click", handleDocumentClick);
});

</script>
<style scoped>
.table-project-action .Custom-DataTable {
  min-width: max-content;
}
</style>
