<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="SDLC" />
              <q-breadcrumbs-el v-if="search.projectIds.length > 0 && search.planIds.length > 0" label="Test Plans" clickable to="/test-plan" />
              <q-breadcrumbs-el label="Test Cases" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Test Case Id</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.testCaseNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.planIds"
                        label="Test Plan Name"
                        :options="testPlansByProjectIdForDropdown.list.value"
                        :filter="testPlansByProjectIdForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.testedBys"
                        label="Tested By"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Test Case Status"
                        :options="testCaseStatusForDropdown.list.value"
                        :filter="testCaseStatusForDropdown.filter"
                        :isShowAll="true"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Created From Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-ml-sm w-100 h-auto">
                            <q-input
                              v-model="search.fromDate" fill-input dense mask="##/##/####"
                            >
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="search.fromDate" mask="MM/DD/YYYY"
                                      @update:model-value="() => $refs.qDateProxy.hide()"
                                    />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Created To Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-ml-sm w-100 h-auto">
                            <q-input v-model="search.toDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="search.toDate"
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
              <q-menu v-model="showManageDropdownOptions" anchor="bottom right" self="top right" no-parent-event style="width: 320px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Manage Dropdown Options</div>
                  <q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in dropdownTypes"
                      :key="opt.id"
                      clickable
                      :active="selectedField === opt.id"
                      active-class="bg-primary text-white"
                      @click="$router.push({ path: '/manage-dropdowns', state: { id: opt.id, groupName: opt.groupName, moduleName: opt.moduleName } })"
                    >
                      <q-item-section>{{ opt.type }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Create Test Case" no-caps class="text-primary btnRounded" @click="onTestCaseAdd(refreshTestCaseList)" />
                <q-btn v-if="search.projectIds.length > 0 && search.planIds.length > 0" icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.back()" />
                <q-btn v-if="role === 'admin'" icon="o_playlist_add" outline no-caps class="text-primary btnRounded q-ml-sm" @click="showManageDropdownOptions = !showManageDropdownOptions">
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />

      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getAllTestCase"
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
            <q-td style="width: 3%;">#{{ props.row.testCaseNumber }}</q-td>
            <!-- <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;"><span v-if="preProjectName !== props.row.project.name" :set="preProjectName = props.row.project.name">{{ props.row.project.name }}</span></q-td> -->
            <q-td style="width: 15%; white-space: normal;" class="hoverable-cell">
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
                    v-if="props.row.isEditable"
                    name="o_developer_board" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id);
                            $router.push({ path: '/project-planning/workboard', state: {projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Work Board</q-tooltip>
                  </q-icon>
                </div>
              </div>
            </q-td>
            <q-td class="hoverable-cell" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">
              <span
                v-if="preTestPlanName !== props.row.testPlan.name"
                :set="preTestPlanName = props.row.testPlan.name"
                @click="onTestPlanView(props.row.testPlan.id)"
              >
                {{ props.row.testPlan.name }}
              </span>
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 25%;">
              {{ props.row.name }}
            </q-td>
            <q-td
              class="common-q-td"
              :class="{ 'hoverable-cell' : props.row.isEditable }"
              @click="activeEdit = { rowId: props.row.id, field: 'status' }"
              style="width: 5%;"
            >
              <quickEditSingleSelect
                field="status"
                :row-id="props.row.id"
                :value="props.row.status.id"
                :display-value="props.row.status.dropDownValue"
                :editable="props.row.isEditable"
                :options="testCaseStatusDropdownSingleSelect.list.value"
                :active-edit="activeEdit"
                :show-history="false"
                @cancel="activeEdit = { rowId: null, field: null }"
                @submit="({ rowId, value }) => onSubmitTestCaseStatus(rowId, value, refreshTestCaseList)"
              />
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">
              {{ props.row.testedByEmployee.person.fullName }}
            </q-td>
            <q-td class="text-center" style="width: 5%;">
              {{ props.row.createdOnUtc }}
            </q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="onTestCaseView(props.row.id, props.row.testPlan.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                v-if="props.row.isEditable"
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="onTestCaseEdit(props.row.id, refreshTestCaseList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
                v-if="props.row.isEditable"
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                size="xs"
                @click="onSubmitTestCaseDelete(props.row.id, props.row.name, refreshTestCaseList)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
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
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useRoute } from "vue-router";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";
import { getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

import testcasesService from "modules/test-case/testCase.service";
import manageDropdownsService from "modules/dropdown/dropdown.service";

// SOP Change :- Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import testPlanModule from "src/modules/test-plan/utils/dropdowns.js";
import testCaseModule from "src/modules/test-case/utils/dropdowns.js";

// SOP Change :- Shared Project Dialogs
import {
  initTestCaseDialogs,
  onTestCaseView,
  onTestCaseAdd,
  onTestCaseEdit
} from "src/modules/test-case/utils/dialogs.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

import {
  initTestPlanDialogs,
  onTestPlanView
} from "src/modules/test-plan/utils/dialogs.js";

// Shared Test Case Actions
import {
  initTestCaseActions,
  onSubmitTestCaseStatus,
  onSubmitTestCaseDelete
} from "src/modules/test-case/utils/actions.js";

// Common variables
const expandedRows = ref([]);
const loading = ref(true);
const route = useRoute();
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const dropdownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const { toDate } = useFilters();
const activeEdit = ref({ rowId: null, field: null });

// local storage values
const localStorageKey = "Test Case";
const filterLocalStorage = getLocalStorage(localStorageKey);
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds : [];
const planIds = filterLocalStorage ? filterLocalStorage.planIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Table variables
const tableRef = ref();
const rows = ref([]);
const shownProjects = new Set();
const columns = ref([
  { name: "testCaseNumber", label: "Id", field: "testCaseNumber", align: "left", sortable: true },
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "testPlan.name", label: "Test Plan Name", field: "testPlan.name", align: "left", sortable: true },
  { name: "name", label: "Test Case Name", field: "name", align: "left", sortable: true },
  { name: "status.dropDownValue", label: "Test Case Status", field: "status.dropDownValue", align: "left", sortable: true },
  { name: "testedByEmployee.person.firstName", label: "Tested By", field: "testedByEmployee.person.firstName", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "center", sortable: true }
]);

const highlightTestCaseId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightTestCaseId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

function setActiveRowIdInLocalStorage (id) {
  const storedData = getLocalStorage(localStorageKey) || {};
  setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
  activeRowId.value = id;
}

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    const storedData = getLocalStorage(localStorageKey) || {};
    setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
  }
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshTestCaseList = () => {
  getAllTestCase({ pagination: pagination.value });
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

function getDropdownTypeByModuleName (moduleName) {
  manageDropdownsService.getDropdownTypeByModuleName(moduleName).then((resp) => {
    dropdownTypes.value = resp;
  });
}

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

// Added colors for dropdown list
// function getStatusColor (statusText) {
//   if (statusText) {
//     switch (statusText) {
//     case "Reopen":
//       return "purple-4";
//     case "Pass":
//       return "green-4";
//     case "Fail":
//       return "red-4";
//     case "Testing":
//       return "deep-orange-4";
//     case "New":
//       return "blue-4";
//     default:
//       return "#ffffff";
//     }
//   }
// }

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel
// ------------------------------------------------------------------------------------

const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

// Search variables
const search = ref({
  searchText: getFilterValue("searchText", ""),
  testedBys: getFilterValue("testedBys", []),
  testCaseNumber: getFilterValue("testCaseNumber", 0),
  statusIds: getFilterValue("statusIds", []),
  fromDate: getFilterValue("fromDate", ""),
  toDate: getFilterValue("toDate", toDate),
  projectIds: route.query.projectId && route.query.projectId !== "" ? (Array.isArray(route.query.projectId) ? route.query.projectId : [route.query.projectId]) : projectIds,
  planIds: route.query.planId && route.query.planId !== "" ? (Array.isArray(route.query.planId) ? route.query.planId : [route.query.planId]) : planIds
});

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshTestCaseList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.testCaseNumber = undefined;
  search.value.projectIds = [];
  search.value.planIds = [];
  search.value.testedBys = [];
  search.value.statusIds = [];
  search.value.fromDate = null;
  search.value.toDate = null;
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get Test Case List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const getAllTestCase = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.fromDate = search.value.fromDate ? toDate(search.value.fromDate) : null;
  search.value.toDate = search.value.toDate ? toDate(search.value.toDate) : null;
  const number = search.value.testCaseNumber ? search.value.testCaseNumber.replace(/[^0-9]/g, "").replace(/^0+(?!$)/, "") : "";
  search.value.testCaseNumber = number || "0";
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  testcasesService.getAllTestCase(payload).then((resp) => {
    rows.value = resp.data.map(testCase => {
      const hasFullAccess = testCase?.project?.projectUserMappings[0]?.fullAccess ?? false;
      return {
        ...testCase,
        isNotes: testCase?.project?.projectUserMappings[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess
      };
    });
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

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initTestCaseDialogs(activeRowId);
initProjectDialogs(activeRowId);
initTestPlanDialogs(activeRowId);
initTestCaseActions(activeRowId);

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
  ...mapFilterToLabel(search.value.planIds, testPlansByProjectIdForDropdown.list, "Test Plan Name"),
  ...mapFilterToLabel(search.value.testedBys, activeEmployeesDropdown.list, "Tested By"),
  ...mapFilterToLabel(search.value.statusIds, testCaseStatusForDropdown.list, "Test Case Status"),
  ...(search.value.testCaseNumber > 0 ? { "Test Case Id": search.value.testCaseNumber } : {}),
  ...(search.value.fromDate ? { "Created From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "Created To Date": search.value.toDate } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Test Plan Name": return search.value.planIds?.length || 0;
  case "Tested By": return search.value.testedBys?.length || 0;
  case "Test Case Status": return search.value.statusIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Test Case Id") {
    search.value.testCaseNumber = "";
  } else if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Test Plan Name") {
    search.value.planIds = [];
  } else if (key === "Tested By") {
    search.value.testedBys = [];
  } else if (key === "Test Case Status") {
    search.value.statusIds = [];
  } else if (key === "Created From Date") {
    search.value.fromDate = "";
  } else if (key === "Created To Date") {
    search.value.toDate = "";
  }
  delete appliedFilters.value[key];
  refreshTestCaseList();
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const {
  projectNameDropdown
} = projectModule();

const { activeEmployeesDropdown } = employeeModule();
const { testPlansByProjectIdForDropdown } = testPlanModule();
const {
  testCaseStatusForDropdown,
  testCaseStatusDropdownSingleSelect
} = testCaseModule();

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshTestCaseList();
});

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  search.value.projectModuleIds = [];
  await testPlansByProjectIdForDropdown.load(search.value.projectIds);
}, { immediate: true });

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdown.load();
  projectNameDropdown.load();
  testCaseStatusForDropdown.load("Test Case Status");
  testCaseStatusDropdownSingleSelect.load("Test Case Status");
  if (search.value.projectIds.length > 0) testPlansByProjectIdForDropdown.load(search.value.projectIds);
  getDropdownTypeByModuleName("SDLC");
  if (!activeRowId.value && highlightTestCaseId) {
    activeRowId.value = highlightTestCaseId;
  }
  document.addEventListener("click", handleDocumentClick);
});

</script>
