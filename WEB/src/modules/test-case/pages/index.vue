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
              <q-breadcrumbs-el v-if="search.projectIds?.length > 0 && search.planIds?.length > 0" label="Test Plans" clickable to="/test-plan" />
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
                      <multiSelectDropdown
                        v-model="search.planIds"
                        label="Test Plan Name"
                        :options="testPlansByProjectIdForDropdown.list.value"
                        :filter="testPlansByProjectIdForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Release Version</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.versionNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
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
                <q-btn
                  icon="o_add"
                  outline
                  label="Create Test Case"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onTestCaseAdd(refreshTestCaseList)"
                />
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="showManageDropdownOptions = !showManageDropdownOptions"
                >
                  <q-tooltip>Manage Dropdowns</q-tooltip>
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
                <q-btn
                  v-if="search.projectIds?.length > 0 && search.planIds?.length > 0"
                  icon="o_chevron_left"
                  outline
                  label="Back"
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="$router.back()"
                />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="table-test-case">
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
            @request="getAllTestCase"
          >
            <template #loading>
              <q-inner-loading showing color="primary">
                <q-spinner-ios size="40px" class="q-mt-xl" />
              </q-inner-loading>
            </template>
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <!-- <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th> -->
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
                <q-td v-if="selectedColumnNames.includes('testCaseNumber')">
                  #{{ props.row.testCaseNumber }}
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
                <q-td v-if="selectedColumnNames.includes('projectModule.name')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.projectModule.name }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('requirement.title')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.requirement.title }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('testPlan.name')" class="hoverable-cell" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  <span
                    v-if="preTestPlanName !== props.row.testPlan.name"
                    :set="preTestPlanName = props.row.testPlan.name"
                    @click="onTestPlanView(props.row.testPlan.id)"
                  >
                    {{ props.row.testPlan.name }}
                  </span>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('name')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.name }}
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('status.dropDownValue')"
                  class="common-q-td"
                  :class="{ 'hoverable-cell' : props.row.isEditable }"
                  @click="activeEdit = { rowId: props.row.id, field: 'status' }"
                >
                  <quickEditSingleSelect
                    field="status"
                    :row-id="props.row.id"
                    :value="props.row.status.id"
                    :display-value="props.row.status.dropDownValue"
                    :editable="props.row.isEditable"
                    :options="testCaseStatusDropdownSingleSelect.list.value"
                    :active-edit="activeEdit"
                    :show-history="true"
                    :loading="updatingRow.status === props.row.id"
                    @cancel="activeEdit = { rowId: null, field: null }"
                    @submit="({ rowId, value }) => onSubmitTestCaseStatus(rowId, value, props.row.projectReleaseTrackingReqPlanTaskIssueMappingId, refreshTestCaseList)"
                    @history="() => onTestCaseStatusChangeLog(null, props.row.id, props.row.projectReleaseTrackingReqPlanTaskIssueMappingId, props.row.name, 'Test Case Status')"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('testedByEmployee.person.firstName')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  {{ props.row.testedByEmployee.person.fullName }}
                </q-td>
              <q-td
                v-if="selectedColumnNames.includes('createdByUser.person.firstName')"
                class="common-q-td"
              >
                {{ props.row.createdByUser.person.fullName }}
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
                    @click="onTestCaseView(props.row.id, props.row.testPlan.id)"
                  >
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_manage_history"
                    class="cursor-pointer q-mr-sm"
                    size="xs"
                    @click="onTestCaseReleaseHistory(props.row.id)"
                  >
                    <q-tooltip>History</q-tooltip>
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

import testcasesService from "modules/test-case/testCase.service";
import manageDropdownsService from "modules/dropdown/dropdown.service";

// SOP Change :- Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import testPlanModule from "src/modules/test-plan/utils/dropdowns.js";
import testCaseModule from "src/modules/test-case/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Project Dialogs
import {
  initTestCaseDialogs,
  onTestCaseView,
  onTestCaseAdd,
  onTestCaseEdit,
  onTestCaseReleaseHistory,
  onTestCaseStatusChangeLog
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
  onSubmitTestCaseDelete,
  updatingRow
} from "src/modules/test-case/utils/actions.js";

// Common variables
const expandedRows = ref([]);
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const dropdownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const showSortDialog = ref(false);
const { toDate } = useFilters();
const activeEdit = ref({ rowId: null, field: null });
const currentSiteId = computed(() => user?.siteId || null);

// Table variables
const rows = ref([]);
const shownProjects = new Set();
const columns = ref([
  { name: "testCaseNumber", label: "Id", field: "testCaseNumber", align: "left", sortable: true, default: true },
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true, default: true },
  { name: "projectModule.name", label: "Project Module", field: "projectModule.name", align: "left", sortable: true, default: false },
  { name: "requirement.title", label: "Requirement", field: "requirement.title", align: "left", sortable: true, default: false },
  { name: "testPlan.name", label: "Test Plan Name", field: "testPlan.name", align: "left", sortable: true, default: true },
  { name: "name", label: "Test Case Name", field: "name", align: "left", sortable: true, default: true },
  { name: "status.dropDownValue", label: "Test Case Status", field: "status.dropDownValue", align: "left", sortable: true, default: true },
  { name: "testedByEmployee.person.firstName", label: "Tested By", field: "testedByEmployee.person.firstName", align: "left", sortable: true, default: true },
  { name: "createdByUser.person.firstName", label: "Created By", field: "createdByUser.person.firstName", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "center", sortable: true, default: true },
  { name: "updatedBy.person.firstName", label: "Updated By", field: "updatedBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "left", sortable: true, default: false }
]);

const getAllTestCase = async ({ pagination: p }) => {
  try {
    loading.value = true;
    const { page, rowsPerPage, sortBy, descending } = p;

    search.value.fromDate = search.value.fromDate
      ? toDate(search.value.fromDate)
      : null;

    search.value.toDate = search.value.toDate
      ? toDate(search.value.toDate)
      : null;

    const number = search.value.testCaseNumber
      ? search.value.testCaseNumber
          .replace(/[^0-9]/g, "")
          .replace(/^0+(?!$)/, "")
      : "";

    search.value.testCaseNumber = number || "0";

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

    const resp = await testcasesService.getAllTestCase(payload);

    rows.value = resp.data.map(testCase => {
      const hasFullAccess =
        testCase?.project?.projectUserMappings?.[0]?.fullAccess ?? false;

      return {
        ...testCase,
        isNotes: testCase?.project?.projectUserMappings?.[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess
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
  testedBys: [],
  testCaseNumber: 0,
  statusIds: [],
  fromDate: "",
  toDate: "",
  projectIds: [],
  projectModuleIds: [],
  requirementIds: [],
  planIds: []
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
  storageKey: "test-Case-Index",
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

if (history.state?.planId) {
  search.value.planIds = Array.isArray(history.state.planId)
    ? history.state.planId
    : [history.state.planId];
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

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshTestCaseList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.testCaseNumber = undefined;
  search.value.projectIds = [];
  search.value.projectModuleIds = [];
  search.value.requirementIds = [];
  search.value.planIds = [];
  search.value.testedBys = [];
  search.value.statusIds = [];
  search.value.versionNumber = "";
  search.value.fromDate = null;
  search.value.toDate = null;
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
    refreshTestCaseList();
  }
});

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
  ...mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Module"),
  ...mapFilterToLabel(search.value.requirementIds, requirementsByProjectModuleIdForDropdown.list, "Requirement"),
  ...mapFilterToLabel(search.value.planIds, testPlansByProjectIdForDropdown.list, "Test Plan Name"),
  ...mapFilterToLabel(search.value.testedBys, activeEmployeesDropdown.list, "Tested By"),
  ...mapFilterToLabel(search.value.statusIds, testCaseStatusForDropdown.list, "Test Case Status"),
  ...(search.value.testCaseNumber > 0 ? { "Test Case Id": search.value.testCaseNumber } : {}),
  ...(search.value.versionNumber > 0 ? { "Release Version": search.value.versionNumber } : {}),
  ...(search.value.fromDate ? { "Created From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "Created To Date": search.value.toDate } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Requirement": return search.value.requirementIds?.length || 0;
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
  } else if (key === "Project Module") {
    search.value.projectModuleIds = [];
  } else if (key === "Requirement") {
    search.value.requirementIds = [];
  } else if (key === "Test Plan Name") {
    search.value.planIds = [];
  } else if (key === "Tested By") {
    search.value.testedBys = [];
  } else if (key === "Test Case Status") {
    search.value.statusIds = [];
  } else if (key === "Release Version") {
    search.value.versionNumber = "";
  } else if (key === "Created From Date") {
    search.value.fromDate = "";
  } else if (key === "Created To Date") {
    search.value.toDate = "";
  }

  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    activeRowId: activeRowId.value
  });
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
const { projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { requirementsByProjectModuleIdForDropdown } = requirementModule();

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshTestCaseList();
});

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  search.value.projectModuleIds = [];
  projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);
  await testPlansByProjectIdForDropdown.load(search.value.projectIds);
}, { immediate: true });

watch(
  () => search.value.projectModuleIds,
  (moduleIds) => {
    if (moduleIds == null) return;
    requirementsByProjectModuleIdForDropdown.load(moduleIds);
  },
  { immediate: true }
);

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

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  refreshTestCaseList();
  activeEmployeesDropdown.load();
  projectNameDropdown.load();
  testCaseStatusForDropdown.load("Test Case Status");
  testCaseStatusDropdownSingleSelect.load("Test Case Status");
  if (search.value.projectIds?.length > 0) testPlansByProjectIdForDropdown.load(search.value.projectIds);
  if (search.value.projectIds.length > 0) projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);
  if (search.value.projectModuleIds.length > 0) requirementsByProjectModuleIdForDropdown.load(search.value.projectModuleIds);
  getDropdownTypeByModuleName("SDLC");
  document.addEventListener("click", handleDocumentClick);
});

</script>
<style scoped>
.table-test-case .Custom-DataTable {
  min-width: max-content;
}
</style>
