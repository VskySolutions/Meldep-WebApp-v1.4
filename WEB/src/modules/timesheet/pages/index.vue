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
              <!-- <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" /> -->
              <q-breadcrumbs-el label="My Work" />
              <q-breadcrumbs-el label="Timesheets" />
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
                        label="Projects"
                        :options="projectNameDropdownSingleSelect.list.value"
                        :filter="projectNameDropdownSingleSelect.filter"
                      />
                      <singleSelectDropdown
                        v-model="search.projectModuleId"
                        label="Project Modules"
                        :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                        :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                      />
                      <singleSelectDropdown
                        v-model="search.projectTaskId"
                        label="Project Tasks"
                        :options="projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.list.value"
                        :filter="projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.filter"
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
                          <label class="Cutomlabel q-mt-sm fs-13">Week Filter</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.weekFilter"
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            clearable
                            hide-bottom-space
                            use-input
                            :dense="true"
                            :options="weekFilterList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @update:model-value="updateDates"
                          />
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
              <div class="row items-center q-gutter-sm q-ml-xs">
                <q-btn icon="o_add" outline label="Add Timesheet" no-caps class="text-primary btnRounded" @click="onTimesheetAdd(refreshTimesheetList)" />
                <q-btn
                  icon="o_add"
                  outline
                  label="Send to New Timesheet"
                  no-caps
                  class="text-primary btnRounded"
                  :disabled="selectedTimesheetLineIds.length === 0"
                  @click="onTimesheetEdit(null, true, selectedTimesheetLineIds,  refreshTimesheetList)"
                />
                <q-btn-dropdown
                  class="q-ml-sm"
                  outline=""
                  color="primary"
                  label="Export Timesheet"
                >
                  <div class="row no-wrap q-pa-md">
                    <div class="column">
                      <div class="text-h6 q-mb-md">Export Timesheet</div>
                      <div v-for="col in columns" :key="col.name">
                        <q-toggle v-model="col.checkedStatus" :label="col.label" />
                      </div>
                    </div>
                    <div class="column items-center">
                      <q-btn class="" color="deep-orange" label="Get File" size="sm" @click="downloadExcel()" />
                    </div>
                  </div>
                </q-btn-dropdown>
                <!-- </div> -->
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
        :columns="columns"
        :rows="rows"
        row-key="id"
        separator="cell"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getTimesheets"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th></q-th>
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td colspan="7" style="background: #dbf2ff;" class="text-center">{{ toDate(props.row.timesheetDate) }}</q-td>
            <q-td auto-width class="text-center actions" style="background: #dbf2ff;">
              <q-icon
                v-if="props.row.isActionVisible && storedUser.username === props.row.user.userName"
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onTimesheetEdit(props.row.id, false, null, refreshTimesheetList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
                v-if="props.row.isActionVisible && storedUser.username === props.row.user.userName"
                name="o_delete_outline"
                class="cursor-pointer"
                :class="storedUser.username === props.row.user.userName ? '' : 'hidden'"
                color="negative"
                @click="onSubmitTimesheetDelete(props.row.id, props.row.timesheetDate, refreshTimesheetList)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-tr v-for="(line) in props.row.timesheetLines" :key="line.id" :class="highlightedId == line.id ? 'highlight' : ''" :set="(preProjectName = null, preProjectTask = null, preProjectDate = null, preProjectTaskDate = null, resetTracking())">
             <q-td class="text-center">
                <q-checkbox
                  v-if="line.projectActivity?.activityStatus?.dropDownValue === 'Open' &&
                        line.projectActivity?.active &&
                        storedUser.username === props.row.user.userName"
                  :model-value="selectedTimesheetLineIds.includes(line.id)"
                  @update:model-value="onSelectCheckbox(line, $event)"
                  size="sm"
                />
              </q-td>
             <q-td class="hoverable-cell common-q-td">
              <div class="row no-wrap items-center justify-between">
                <span
                  v-if="preProjectName !== line.project.name || preProjectDate !== props.row.timesheetDate" :set="(preProjectName = line.project.name, preProjectDate = props.row.timesheetDate)"
                  style="flex: 1; word-break: break-word; white-space: normal;" @click="onProjectView(line.project.id)"
                >{{ line.project.name }}</span>

                <div v-if="shouldShowIcons(line.project.name, 'project', props.row.timesheetDate)" class="row items-center q-gutter-sm q-ml-sm hidden" style="flex-shrink: 0;">
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
            <q-td class="text-left common-q-td" >{{ line.projectModule.name }}</q-td>
            <q-td class="hoverable-cell common-q-td">
              <div class="row no-wrap items-center justify-between">
                <span
                  v-if="preProjectTask !== line.task.name || preProjectTaskDate !== props.row.timesheetDate"
                  :set="(preProjectTask = line.task.name, preProjectTaskDate = props.row.timesheetDate)"
                  style="flex: 1; word-break: break-word; white-space: normal;"
                  @click="onProjectTaskView(line.task.id)"
                >
                  {{ line.task.name }}
                </span>
              </div>
            </q-td>
            <q-td
              class="text-left common-q-td"
            >
              {{ line.projectActivity.name }}
              <q-icon
                v-if="line.activityNameDescription"
                name="o_info"
                size="15px"
                class="q-ml-sm"
              >
                <q-tooltip v-if="line.activityNameDescription" class="text-wrap break-words" max-width="300px">
                  <div v-html="line.activityNameDescription" />
                </q-tooltip>
              </q-icon>
            </q-td>
            <q-td class="RichTextEditor common-q-td">
              <div v-html="line.description" />
            </q-td>
            <q-td class="text-left common-q-td">
              {{ props.row.user.person.fullName }}
            </q-td>
            <q-td class="text-right">
              {{ line.hours }}
            </q-td>
          </q-tr>
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td colspan="7" class="text-right">Total:</q-td>
            <q-td class="text-right">
              {{ calculateLineTotal(props.row.timesheetLines) }}
            </q-td>
          </q-tr>
          <q-tr v-if="props.pageIndex === rows.length - 1">
            <q-td colspan="7" class="text-right">Total Hours:</q-td>
            <q-td class="text-right">
              {{ calculateGrandTotal(rows) }}
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { format } from "date-fns"; // Standard TimeZone Conversion

import timesheetService from "modules/timesheet/timesheet.service";
import useFilters from "composables/useFilters";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";

// Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Timesheet Dialogs
import {
  initTimesheetDialogs,
  onTimesheetAdd,
  onTimesheetEdit
} from "src/modules/timesheet/utils/dialogs.js";

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

// Shared timesheet Actions
import {
  initTimesheetActions,
  onSubmitTimesheetDelete
} from "src/modules/timesheet/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const $q = useQuasar();
const storedUser = getLocalStorage("user");
const authStore = useAuthStore();
const user = authStore.user;
const { toDate } = useFilters();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const shownProjects = new Set();
const shownTasks = new Set();

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Timesheet";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Timesheet
// ----------------------------------------------------------------------------------------------------------------

// const getTimesheets = (props) => {
//   const { page, rowsPerPage, sortBy, descending } = props.pagination;
//   loading.value = true;
//   search.value.activityDate = isValidDate(search.value.activityDate) ? search.value.activityDate : null;
//   search.value.toDate = isValidDate(search.value.toDate) ? search.value.toDate : null;
//   search.value.fromDate = isValidDate(search.value.fromDate) ? search.value.fromDate : null;
//   search.value.projectId = search.value.projectId === "" ? null : search.value.projectId;
//   search.value.projectModuleId = search.value.projectModuleId === "" ? null : search.value.projectModuleId;
//   const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
//   setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
//   timesheetService.getTimesheets(payload).then((resp) => {
//      rows.value = resp.data.map(row => ({
//     ...row,
//     timesheetLines: row.timesheetLines?.map(line => ({
//       ...line,
//       checkboxStatus: selectedTimesheetLines.value.some(
//         item => item.id === line.id
//       )
//     }))
//   }));
//     pagination.value.page = page;
//     pagination.value.rowsPerPage = rowsPerPage;
//     pagination.value.sortBy = sortBy;
//     pagination.value.descending = descending;
//     pagination.value.rowsNumber = resp.total;
//   }).finally(() => {
//     loading.value = false;
//     searchLoader.value = false;
//   });
// };

const getTimesheets = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.activityDate = isValidDate(search.value.activityDate) ? search.value.activityDate : null;
  search.value.toDate = isValidDate(search.value.toDate) ? search.value.toDate : null;
  search.value.fromDate = isValidDate(search.value.fromDate) ? search.value.fromDate : null;
  search.value.projectId = search.value.projectId === "" ? null : search.value.projectId;
  search.value.projectModuleId = search.value.projectModuleId === "" ? null : search.value.projectModuleId;

  const payload = {
    thisWeek: true,
    lastNumberOfWeeks: 1,
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    ...search.value
  };
  setLocalStorage(localStorageKey, {
    ...search.value,
    pagination: props.pagination,
    activeRowId: activeRowId.value
  });

  timesheetService.getTimesheets(payload)
    .then((resp) => {
      rows.value = resp.data.map(row => ({
        ...row,
        timesheetLines: row.timesheetLines?.map(line => ({
          ...line,
          checkboxStatus: selectedTimesheetLineIds.value.some(
            item => item.id === line.id
          )
        })) || []
      }));
      pagination.value.page = page;
      pagination.value.rowsPerPage = rowsPerPage;
      pagination.value.sortBy = sortBy;
      pagination.value.descending = descending;
      pagination.value.rowsNumber = resp.total;
    })
    .finally(() => {
      loading.value = false;
      searchLoader.value = false;
    });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const highlightTimesheetId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightTimesheetId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

function setActiveRowIdInLocalStorage (id) {
  const storedData = getLocalStorage(localStorageKey) || {};
  setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
  activeRowId.value = id;
}

// Function to calculate dates
const calculateLastWeekDates = () => {
  const weekFromDate = new Date();
  const weekToDate = new Date();
  const currentDay = weekFromDate.getDay();
  console.log(weekFromDate);
  // Calculate last week's Monday and Saturday
  const lastMondayOffset = (currentDay === 0 ? -6 : 1) - currentDay;
  const lastSaturdayOffset = 6 - currentDay - (currentDay === 0 ? 7 : 0);

  weekFromDate.setDate(weekFromDate.getDate() + lastMondayOffset - 7); // Last Monday
  weekToDate.setDate(weekToDate.getDate() + lastSaturdayOffset - 7); // Last Saturday

  return { fromDate: weekFromDate, toDate: weekToDate };
};

const calculateThisWeekDates = () => {
  const today = new Date();
  const currentDay = today.getDay();
  const mondayOffset = (currentDay === 0 ? -6 : 1) - currentDay;

  const fromDate = new Date(today);
  const toDate = new Date(today);

  fromDate.setDate(today.getDate() + mondayOffset); // Monday of this week
  toDate.setDate(fromDate.getDate() + 5); // Saturday of this week

  return { fromDate, toDate };
};

const calculateThisMonthDates = () => {
  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth(), 1); // 1st of this month
  const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0); // Last day of this month

  return { fromDate: firstDay, toDate: lastDay };
};

// Function to update dates based on the selected filter
const updateDates = (weekFilter) => {
  let dates;

  switch (weekFilter) {
  case "Last Week":
    dates = calculateLastWeekDates();
    break;
  case "This Week":
    dates = calculateThisWeekDates();
    break;
  case "This Month":
    dates = calculateThisMonthDates();
    break;
  default:
    dates = { fromDate: "", toDate: "" };
  }

  search.value.fromDate = format(dates.fromDate, "MM/dd/yyyy");
  search.value.toDate = format(dates.toDate, "MM/dd/yyyy");
};

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    const storedData = getLocalStorage(localStorageKey) || {};
    setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
  }
};

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

const isValidDate = (dateStr) => {
  const regex = /^(0[1-9]|1[0-2])\/(0[1-9]|[12]\d|3[01])\/\d{4}$/;
  if (!regex.test(dateStr)) return false;

  const [mm, dd, yyyy] = dateStr.split("/").map(Number);
  const date = new Date(yyyy, mm - 1, dd);
  return date.getFullYear() === yyyy && date.getMonth() === mm - 1 && date.getDate() === dd;
};

function calculateLineTotal (timesheetLines) {
  const total = timesheetLines.reduce((sum, line) => {
    return sum + (parseFloat(line.hours) || 0);
  }, 0);
  return Number(total.toFixed(2));
}

// Function to calculate total hours
function calculateGrandTotal (rows) {
  return rows.reduce((grandTotal, row) => {
    return grandTotal + calculateLineTotal(row.timesheetLines);
  }, 0);
}

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

const selectedTimesheetLineIds = ref([]);
const onSelectCheckbox = (line, flag) => {
  console.log("Clicked line:", line.id);
  if (flag) {
    if (!selectedTimesheetLineIds.value.includes(line.id)) {
      selectedTimesheetLineIds.value.push(line.id);
    }
  } else {
    selectedTimesheetLineIds.value = selectedTimesheetLineIds.value.filter(
      id => id !== line.id
    );
  }
};

// const onSendTimesheet = () => {
//   activeRowId.value = selectedTimesheetLines.value.map(line => line.id);

//   $q.dialog({
//     component: editTimesheet,
//     componentProps: { timesheetLineIds: selectedTimesheetLines.value, isMyTaskActivity: true }
//   }).onOk(() => {
//     selectedTimesheetLines.value = [];
//     localStorage.removeItem("selectedTimesheetLines");
//     refreshTimesheetList();s
//   }).onCancel(() => {
//   }).onDismiss(() => {
//     activeRowId.value = null;
//   });
// };

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const createdByList = ref(["Created By Me", "View All"]);
const weekFilterList = ref(["Last Week", "This Week", "This Month"]);
const columns = ref([
  { name: "project.name", label: "Project Name", field: "ProjectId", align: "left", sortable: true, checkedStatus: true, type: "P" },
  { name: "projectModule.name", label: "Module Name", field: "projectModule.name", align: "left", sortable: true, checkedStatus: true, type: "PM" },
  { name: "task.name", label: "Task", field: "task.name", align: "left", sortable: true, checkedStatus: true, type: "PT" },
  { name: "projectActivity.name", label: "Project Activity", field: "projectActivity.name", align: "left", sortable: true, checkedStatus: true, type: "PA" },
  { name: "description", label: "Activity Details", field: "description", align: "left", sortable: true, checkedStatus: true, type: "D" },
  { name: "createdById", label: "Created By", field: "createdById", align: "left", sortable: true, checkedStatus: true, type: "U" },
  { name: "hours", label: "Actual Hours", field: "hours", align: "right", sortable: true, checkedStatus: true, type: "H" }
]);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

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
  projectModuleId: getFilterValue("projectModuleId", null),
  projectTaskId: getFilterValue("projectTaskId", null),
  activityDate: getFilterValue("activityDate", ""),
  fromDate: getFilterValue("fromDate", ""),
  toDate: getFilterValue("toDate", ""),
  weekFilter: getFilterValue("weekFilter", "")
});

const refreshTimesheetList = () => {
  getTimesheets({ pagination: pagination.value });
};

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshTimesheetList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.createdBy = "Created By Me";
  search.value.weekFilter = "";
  search.value.projectId = null;
  search.value.projectModuleId = null;
  search.value.projectTaskId = null;
  search.value.projectActivityId = null;
  search.value.employeeId = null;
  search.value.activityDate = null;
  search.value.fromDate = null;
  search.value.toDate = null;
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

const downloadExcel = () => {
  const payload2 = {
    timesheetDataModel: rows.value,
    columns: columns.value
  };
  timesheetService.exportTimesheet(payload2).then(response => {
    // Create a Blob URL
    const url = window.URL.createObjectURL(new Blob([response]));
    const link = document.createElement("a");
    link.href = url;

    // Set the file name
    link.setAttribute("download", "Timesheet.xlsx");

    // Trigger download
    document.body.appendChild(link);
    link.click();

    // Clean up
    link.remove();
  });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initTimesheetDialogs(activeRowId);
initProjectDialogs(activeRowId);
initProjectTaskDialogs(activeRowId);
initTimesheetActions(activeRowId);

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
  ...mapFilterToLabel(search.value.createdBy, createdByList, "Created By"),
  ...mapFilterToLabel(search.value.employeeId, activeEmployeesDropdownSingleSelect.list, "Employee Name"),
  ...mapFilterToLabel(search.value.projectId, projectNameDropdownSingleSelect.list, "Projects"),
  ...mapFilterToLabel(search.value.projectModuleId, projectModulesByProjectIdForDropdownSingleSelect.list, "Project Modules"),
  ...mapFilterToLabel(search.value.projectTaskId, projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.list, "Project Tasks"),
  ...mapFilterToLabel(search.value.weekFilter, weekFilterList, "Week Filter"),
  ...(search.value.activityDate ? { "Activity Date": search.value.activityDate } : {}),
  ...(search.value.fromDate ? { "From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "To Date": search.value.toDate } : {})
}));

function onClearFilters (key) {
  if (key === "Created By") {
    search.value.createdBy = "";
  } else if (key === "Employee Name") {
    search.value.employeeId = null;
  } else if (key === "Projects") {
    search.value.projectId = null;
  } else if (key === "Project Modules") {
    search.value.projectModuleId = null;
  } else if (key === "Project Tasks") {
    search.value.projectTaskId = null;
  } else if (key === "Week Filter") {
    search.value.weekFilter = "";
  } else if (key === "Activity Date") {
    search.value.activityDate = "";
  } else if (key === "From Date") {
    search.value.fromDate = "";
  } else if (key === "To Date") {
    search.value.toDate = "";
  }
  delete appliedFilters.value[key];
  refreshTimesheetList();
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
const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();
const { projectTasksByProjectIdAndModuleIdForDropdownSingleSelect } = projectTaskModule();

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshTimesheetList();
});

watch(() => search.value.createdBy, (newValue) => {
  if (newValue === "Created By Me") {
    search.value.employeeId = ""; // Clear the employee selection
  }
});

watch(() => search.value.projectId, async (newValue, oldValue) => {
  if (!newValue || newValue === oldValue) return;

  search.value.projectModuleId = null;
  await projectModulesByProjectIdForDropdownSingleSelect.load(false, false, search.value.projectId);
}, { immediate: true });

watch(() => search.value.projectModuleId, (newValue, oldValue) => {
  if (!newValue || newValue === oldValue) return;

  search.value.projectTaskId = null;
  projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.load(false, search.value.projectId, search.value.projectModuleId);
}, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdownSingleSelect.load(user.siteId);
  projectNameDropdownSingleSelect.load();

  if (search.value.projectId) projectModulesByProjectIdForDropdownSingleSelect.load(false, false, search.value.projectId);
  if (search.value.projectModuleId) projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.load(false, search.value.projectId, search.value.projectModuleId);

  if (!activeRowId.value && highlightTimesheetId) {
    activeRowId.value = highlightTimesheetId;
  }

  document.addEventListener("click", handleDocumentClick);
  refreshTimesheetList();
});

</script>
