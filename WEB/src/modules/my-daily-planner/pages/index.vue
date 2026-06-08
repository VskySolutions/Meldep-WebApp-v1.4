<template>
  <q-page padding>
    <q-card>
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <!-- <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" /> -->
              <q-breadcrumbs-el label="My Work" />
              <q-breadcrumbs-el label="Daily Planner List" />
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Created By</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.createdBy"
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            use-input
                            :dense="true"
                            :options="createdByList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <singleSelectDropdown
                        v-model="search.employeeId"
                        label="Employee Name"
                        :options="activeEmployeesDropdownSingleSelect.list.value"
                        :filter="activeEmployeesDropdownSingleSelect.filter"
                        :disable="search.createdBy === 'Created By Me'"
                      />
                      <singleSelectDropdown
                        v-model="search.projectId"
                        label="Project Name"
                        :options="projectNameDropdownSingleSelect.list.value"
                        :filter="projectNameDropdownSingleSelect.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Activity Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.activityDate" fill-input dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.activityDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">From Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.fromDate" fill-input dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.fromDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
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
                            <q-input v-model="search.toDate" fill-input dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.toDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
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
                <q-btn icon="o_add"
                  outline
                  label="Add Daily Plan"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onDailyPlannerAdd(refreshDailyPlannerList)"
                />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-separator />
      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        flat
        :loading="loading"
        :columns="columns"
        :rows="rows"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        :filter="filter"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getDailyPlanners"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td colspan="6" style="background: #dbf2ff;" class="text-center">
              {{ props.row.dailyPlannerDate }}
            </q-td>
            <q-td auto-width class="text-center actions" style="background: #dbf2ff;">
              <q-icon 
                name="o_forward" 
                class="cursor-pointer q-mr-sm" 
                :class="props.row.isForwordedToTimesheet ? 'hidden' : (storedUser.username === props.row.user.userName ? '' : 'hidden')" 
                @click="onDailyPlannerEdit(props.row.id, 'isForwarded', refreshDailyPlannerList)"
              >
                <q-tooltip>Forward To Timesheet</q-tooltip>
              </q-icon>
              <q-icon 
                name="o_edit" 
                class="cursor-pointer q-mr-sm" 
                :class="storedUser.username === props.row.user.userName ? '' : 'hidden'" 
                @click="onDailyPlannerEdit(props.row.id, 'isEdit', refreshDailyPlannerList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon 
                name="o_delete_outline" 
                class="cursor-pointer" 
                :class="storedUser.username === props.row.user.userName ? '' : 'hidden'" color="negative" 
                @click="onSubmitDailyPlannerDelete(props.row.id, props.row.dailyPlannerDate, refreshDailyPlannerList)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-tr v-for="(line) in props.row.dailyPlannerLines" :key="line.id" :class="highlightedId == line.id ? 'highlight' : ''" :set="(preProjectName = null, preProjectTask = null, preProjectDate = null, preProjectTaskDate = null,resetTracking())">
            <q-td style="width: 20%; white-space: normal;" class="hoverable-cell">
              <div class="row no-wrap items-center justify-between">
                <span v-if="preProjectName !== line.project.name || preProjectDate !== props.row.dailyPlannerDate" :set="(preProjectName = line.project.name, preProjectDate = props.row.dailyPlannerDate)" style="flex: 1; word-break: break-word; white-space: normal;" @click="onProjectView(line.project.id)">{{ line.project.name }}</span>
                <div v-if="shouldShowIcons(line.project.name, 'project', props.row.dailyPlannerDate)" class="row items-center q-gutter-sm q-ml-sm" style="flex-shrink: 0;">
                  <q-icon
                    name="o_radio_button_checked" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(line.id);
                            $router.push({ path: '/project-center', state: { projectId: line.project.id } })"
                  >
                    <q-tooltip>Project Center</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_developer_board" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(line.id);
                            $router.push({ path: '/project-planning/workboard', state: {projectId: line.project.id } })"
                  >
                    <q-tooltip>Work Board</q-tooltip>
                  </q-icon>
                </div>
              </div>
            </q-td>
            <q-td class="text-left" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">
              {{ line.projectModule.name }}
            </q-td>
            <q-td style="width: 20%; white-space: normal;" class="hoverable-cell">
              <div class="row no-wrap items-center justify-between">
                <span
                  v-if="preProjectTask !== line.projectTask.name || preProjectTaskDate !== props.row.dailyPlannerDate"
                  :set="(preProjectTask = line.projectTask.name, preProjectTaskDate = props.row.dailyPlannerDate)"
                  style="flex: 1; word-break: break-word; white-space: normal;"
                  @click="onProjectTaskView(line.projectTask.id, refreshDailyPlannerList)"
                >
                  {{ line.projectTask.name }}
                </span>
              </div>
            </q-td>
            <q-td
              class="text-left"
              style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;"
            >
              {{ line.projectActivity.name }}
              <q-icon
                v-if="line.activityNameDescription"
                name="o_info"
                size="15px"
                class="q-ml-sm"
              >
                <q-tooltip v-if="line.activityNameDescription" class="text-wrap break-words" max-width="300px">
                  <div class="RichTextEditor" v-html="line.activityNameDescription" />
                </q-tooltip>
              </q-icon>
            </q-td>
            <q-td>
              <div class="RichTextEditor" style="display: block; width: 450px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;" v-html="line.description" />
            </q-td>
            <q-td class="text-left" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">
              {{ props.row.user.person.fullName }}
            </q-td>
            <q-td class="text-right" style="width: 5%;">
              {{ line.hours }}
            </q-td>
          </q-tr>
          <q-tr>
            <q-td colspan="6" class="text-right">Total:</q-td>
            <q-td class="text-right">
              {{ calculateLineTotal(props.row.dailyPlannerLines) }}
            </q-td>
          </q-tr>
          <q-tr v-if="props.pageIndex === rows.length - 1">
            <q-td colspan="6" class="text-right">Total Hours:</q-td>
            <q-td class="text-right">
              {{ calculateGrandTotal(rows) }}
            </q-td>
          </q-tr>
          <q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>

import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { useAuthStore } from "stores/auth";

import dailyplannerService from "modules/my-daily-planner/myDailyPlanner.service";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// Shared DailyPlanner Dialogs
import {
  initDailyPlannerDialogs,
  onDailyPlannerAdd,
  onDailyPlannerEdit
} from "src/modules/my-daily-planner/utils/dialogs.js";

// Shared Projects Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

// Shared my-daily-planner Actions
import {
  initDailyPlannerActions,
  onSubmitDailyPlannerDelete
} from "src/modules/my-daily-planner/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const storedUser = getLocalStorage("user");
const authStore = useAuthStore();
const user = authStore.user;
const loading = ref(true);
const rows = ref([]);
const filter = ref("");
const showFilter = ref(false);
const searchLoader = ref(false);
const createdByList = ref(["Created By Me", "View All"]);
const shownProjects = new Set();
const shownTasks = new Set();

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------
const localStorageKey = "DailyPlanner";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "", descending: true, rowsPerPage: 20, page: 1 });

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All DailyPlanner
// ----------------------------------------------------------------------------------------------------------------

const getDailyPlanners = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.activityDate = isValidDate(search.value.activityDate) ? search.value.activityDate : null;
  search.value.toDate = isValidDate(search.value.toDate) ? search.value.toDate : null;
  search.value.fromDate = isValidDate(search.value.fromDate) ? search.value.fromDate : null;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  dailyplannerService.getDailyPlanners(payload).then((resp) => {
    rows.value = resp.data;
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
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------
const highlightProjectId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightProjectId);
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

const isValidDate = (dateStr) => {
  const regex = /^(0[1-9]|1[0-2])\/(0[1-9]|[12]\d|3[01])\/\d{4}$/;
  if (!regex.test(dateStr)) return false;

  const [mm, dd, yyyy] = dateStr.split("/").map(Number);
  const date = new Date(yyyy, mm - 1, dd);
  return date.getFullYear() === yyyy && date.getMonth() === mm - 1 && date.getDate() === dd;
};

function resetTracking () {
  shownProjects.clear(); // Clear the set before rendering rows
  shownTasks.clear(); // Clear the set before rendering rows
}

function shouldShowIcons (name, type, date) {
  const uniqueKey = `${name}-${date}`; // Combine name and date to create a unique key

  if (type === "project") {
    if (shownProjects.has(uniqueKey)) {
      return false; // Do not show icons if the project for this date is already shown
    } else {
      shownProjects.add(uniqueKey); // Add the unique key to the set
      return true; // Show icons for the first occurrence
    }
  }

  if (type === "task") {
    if (shownTasks.has(uniqueKey)) {
      return false; // Do not show icons if the task for this date is already shown
    } else {
      shownTasks.add(uniqueKey); // Add the unique key to the set
      return true; // Show icons for the first occurrence
    }
  }
}

function calculateLineTotal (dailyPlannerLines = []) {
  const total = dailyPlannerLines.reduce((sum, line) => {
    return sum + (parseFloat(line.hours) || 0);
  }, 0);
  return Number(total.toFixed(2));
}

// Function to calculate total hours
function calculateGrandTotal (rows) {
  return rows.reduce((grandTotal, row) => {
    const lineTotal = calculateLineTotal(row.dailyPlannerLines);
    return grandTotal + lineTotal;
  }, 0);
}

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const columns = ref([
  { name: "project.name", label: "Project Name", field: "ProjectId", align: "left", sortable: true },
  { name: "projectModule.name", label: "Module Name", field: "projectModule.name", align: "left", sortable: true },
  { name: "projectTask.name", label: "Task", field: "projectTask.name", align: "left", sortable: true },
  { name: "projectActivity.name", label: "Project Activity", field: "projectActivity.name", align: "left", sortable: true },
  { name: "description", label: "Activity Details", field: "description", align: "left", sortable: true },
  { name: "createdById", label: "Employee Name", field: "createdById", align: "left", sortable: true },
  { name: "hours", label: "Actual Hours", field: "hours", align: "right", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

const refreshDailyPlannerList = () => {
  getDailyPlanners({ pagination: pagination.value });
};

// Search variables
const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

const search = ref({
  searchText: getFilterValue("searchText", ""),
  createdBy: getFilterValue("createdBy", "Created By Me"),
  employeeId: getFilterValue("employeeId", null),
  projectId: getFilterValue("projectId", null),
  activityDate: getFilterValue("activityDate", ""),
  fromDate: getFilterValue("fromDate", ""),
  toDate: getFilterValue("toDate", "")
});

// Clear search
const onAdvanceClear = () => {
  search.value.createdBy = "Created By Me";
  search.value.projectId = null;
  search.value.employeeId = null;
  search.value.activityDate = null;
  search.value.fromDate = null;
  search.value.toDate = null;
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

const onAdvanceSearch = () => {
  refreshDailyPlannerList();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initDailyPlannerDialogs(activeRowId);
initProjectDialogs(activeRowId);
initProjectTaskDialogs(activeRowId);
initDailyPlannerActions(activeRowId);

// ----------------------------
// Applied Filter Labels.
// ----------------------------

const mapFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.createdBy, createdByList, "Created By"),
  ...mapFilterToLabel(search.value.employeeId, activeEmployeesDropdownSingleSelect.list, "Employee Name"),
  ...mapFilterToLabel(search.value.projectId, projectNameDropdownSingleSelect.list, "Project Name"),
  ...(search.value.activityDate ? { "Activity Date": search.value.activityDate } : {}),
  ...(search.value.fromDate ? { "From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "To Date": search.value.toDate } : {})
}));

function onClearFilters (key) {
  if (key === "Created By") {
    search.value.createdBy = "";
  } else if (key === "Employee Name") {
    search.value.employeeId = null;
  } else if (key === "Project Name") {
    search.value.projectId = null;
  } else if (key === "Activity Date") {
    search.value.activityDate = "";
  } else if (key === "From Date") {
    search.value.fromDate = "";
  } else if (key === "To Date") {
    search.value.toDate = "";
  }
  delete appliedFilters.value[key];
  refreshDailyPlannerList();
}

function getFilterCount (key) {
  switch (key) {
  default: return null; // For single-value filters like Year, Status
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { activeEmployeesDropdownSingleSelect } = employeeModule();
const { projectNameDropdownSingleSelect } = projectModule();

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshDailyPlannerList();
});

watch(() => search.value.createdBy, (newValue) => {
  if (newValue === "Created By Me") {
    search.value.employeeId = null; // Clear the employee selection
  }
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdownSingleSelect.load(user.siteId);
  projectNameDropdownSingleSelect.load();

  if (!activeRowId.value && highlightProjectId) {
    activeRowId.value = highlightProjectId;
  }

  document.addEventListener("click", handleDocumentClick);
  refreshDailyPlannerList();
});

</script>
