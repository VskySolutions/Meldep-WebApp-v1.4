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
              <q-breadcrumbs-el label="Project Management" />
              <q-breadcrumbs-el label="Client's Timesheets" />
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Start Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.fromDate" fill-input dense mask="##/##/####">
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
                          <label class="Cutomlabel q-mt-sm fs-13">End Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.toDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.toDate" mask="MM/DD/YYYY" :options="disableBeforeStartDate" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <singleSelectDropdown
                        v-model="search.projectId"
                        label="Project Name"
                        :options="projectNameDropdownSingleSelect.list.value"
                        :filter="projectNameDropdownSingleSelect.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.projectModuleIds"
                        label="Project Module"
                        :options="projectModulesByProjectIdForDropdown.list.value"
                        :filter="projectModulesByProjectIdForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.projectTaskIds"
                        label="Project Task"
                        :options="projectTasksByProjectIdAndModuleIdForDropdown.list.value"
                        :filter="projectTasksByProjectIdAndModuleIdForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.customerIds"
                        label="Customer Name"
                        :options="customerNameDropdown.list.value"
                        :filter="customerNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.companyContactIds"
                        label="Company Contact"
                        :options="companyContactNameDropdown.list.value"
                        :filter="companyContactNameDropdown.filter"
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
                <div>
                  <q-btn icon="o_view_list" class="text-white btnRounded bg-primary q-ml-xs" @click="$router.push({ path: '/billing-processing' })"><q-tooltip>Line</q-tooltip></q-btn>
                </div>
                <div class="q-ml-sm">
                  <div class="q-mr-md AdvanceBTN justify-end">
                    <q-btn-dropdown
                      class=""
                      outline=""
                      color="primary"
                      label="Export Timesheet"
                    >
                      <div class="row no-wrap q-pa-md">
                        <div class="column">
                          <div class="text-h6 q-mb-md">Export Timesheet</div>
                          <div v-for="col in columns" :key="col.name">
                            <div v-if="col.label !== 'Billable Hours(Percent)'">
                              <q-toggle v-model="col.checkedStatus" :label="col.label" />
                            </div>
                          </div>
                        </div>

                        <div class="column items-center">
                          <q-btn class="" color="deep-orange" label="Get File" size="sm" @click="downloadExcel()" />
                        </div>
                      </div>
                    </q-btn-dropdown>
                  </div>
                </div>
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
        @request="getGroupedBillableTimesheets"
      >
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
          </q-tr>
          <q-tr class="bg-grey-3">
            <q-th colspan="4" class="text-right" style="font-size: 12px !important;"><b>Total Hours:</b></q-th>
            <q-th class="text-right" style="font-size: 12px !important;"><b>{{ totalEstimateHours() }}</b></q-th>
            <q-th class="text-right" style="font-size: 12px !important;"><b>{{ totalBillableHours() }}</b></q-th>
            <q-th></q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''" :set="(preProjectName = null, preProjectModuleName = null, preProjectTaskName = null)">
            <q-td class="text-left common-q-td"><span v-if="preProjectName !== props.row.project.name" :set="preProjectName = props.row.project.name">{{ props.row.project.name }}</span></q-td>
            <q-td class="text-left common-q-td"><span v-if="preProjectModuleName !== props.row.projectModule.name" :set="preProjectModuleName = props.row.projectModule.name">{{ props.row.projectModule.name }}</span></q-td>
            <q-td class="text-left common-q-td"><span v-if="preProjectTaskName !== props.row.task.name" :set="preProjectTaskName = props.row.task.name">{{ props.row.task.name }}</span></q-td>
            <q-td class="text-left common-q-td">{{ props.row.timesheet.user.person.fullName }}</q-td>
            <q-td class="text-right" style="width: 5%;">{{ props.row.hours }}</q-td>
            <q-td class="text-right" style="width: 5%;">{{ getCalculatedBillableHours(props.row) }}</q-td>
            <q-td class="text-right" style="width: 2%;">
              <div class="row items-center no-wrap justify-end">
                <q-input
                  v-model="props.row.billablePercent"
                  class="billableHrsInput col"
                  type="number"
                  min="1"
                  max="100"
                  dense
                  :rules="[validateHours]"
                  @blur="onChangeBillableHrs(props.row.id, props.row.billablePercent)"
                  :style="{ marginBottom: validateHours(props.row.billablePercent) !== true ? '20px' : '0' }"
                >
                  <template #error>
                    <div>
                      {{ validateHours(props.row.billablePercent) !== true ? validateHours(props.row.billablePercent) : '' }}
                    </div>
                  </template>
                </q-input>
                <div class="row items-center justify-center" style="width: 20%;">
                  <q-icon
                    v-if="props.row.billableCreatedOnUtc != null && props.row.billableCreatedBy?.person?.fullName?.trim()"
                    name="o_info"
                    size="16px"
                    color="primary"
                    class="q-ml-xs cursor-pointer"
                  >
                    <q-tooltip>
                      <div class="text-left">
                        <div>
                          <strong>Updated By :</strong>
                          {{ props.row.billableCreatedBy?.person?.fullName }}
                        </div>
                        <div>
                          <strong>Updated On :</strong>
                          {{ props.row.billableCreatedOnUtc }}
                        </div>
                      </div>
                    </q-tooltip>
                  </q-icon>
                </div>
              </div>
            </q-td>
          </q-tr>
          <q-tr v-if="props.pageIndex === rows.length - 1" class="bg-grey-3">
            <q-td colspan="4" class="text-right"><b>Total Hours:</b></q-td>
            <q-td class="text-right"><b>{{ totalEstimateHours() }}</b></q-td>
            <q-td class="text-right"><b>{{ totalBillableHours() }}</b></q-td>
            <q-td class="text-right"></q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import timesheetService from "modules/timesheet/timesheet.service";
import { setLocalStorage, clearLocalStorage, getLocalStorage, notifySuccess } from "assets/utils";

// Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js"
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";

// SOP Change :- Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

import { parse, format } from "date-fns"; // Standard TimeZone Conversion

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

const currentYear = new Date().getFullYear();
const defaultFromDate = format(new Date(currentYear, 0, 1), "MM/dd/yyyy");
const defaultToDate = format(new Date(currentYear, 11, 31), "MM/dd/yyyy");

// local storage values
const localStorageKey = "Group Billing Timesheet";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "", descending: true, rowsPerPage: 20, page: 1 });

// Get group billable timesheets list to table
const getGroupedBillableTimesheets = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  if (search.value.toDate === "") {
    search.value.toDate = null;
  }
  if (search.value.fromDate === "") {
    search.value.fromDate = null;
  }
  search.value.projectId = search.value.projectId === "" ? null : search.value.projectId;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  timesheetService.getGroupedBillingTimesheet(payload).then((resp) => {
    // rows.value = resp.data;
    rows.value = resp.data.map(row => {
      const hours = parseFloat(row.hours || 0);
      const billable = parseFloat(row.billableHours || 0);
      const percent = hours > 0 ? ((billable / hours) * 100) : 0;
      return {
        ...row,
        billablePercent: percent ? parseFloat(percent.toFixed(2)) : 0// Used only in UI
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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------
const refreshBillableTimesheetList = () => {
  getGroupedBillableTimesheets({ pagination: pagination.value });
};

function totalEstimateHours () {
  const total = rows.value.reduce((total, row) => total + (row.hours || 0), 0);
  return total.toFixed(2);
}

function totalBillableHours () {
  const total = rows.value.reduce((sum, row) => {
    const billable = parseFloat(row.billableHours || 0);
    return sum + (isNaN(billable) ? 0 : billable);
  }, 0);
  return total.toFixed(2);
}

function getCalculatedBillableHours (row) {
  const percent = parseFloat(row.billablePercent || 0);
  const hours = parseFloat(row.hours || 0);
  if (isNaN(percent) || isNaN(hours)) return "0.00";
  // return ((percent / 100) * hours).toFixed(2);
  const calculated = (percent / 100) * hours;
  row.billableHours = parseFloat(calculated.toFixed(2)); // Update billableHours used for display
  return calculated.toFixed(2);
}

function onChangeBillableHrs (id, billablePercent) {
  if (validateHours(billablePercent) !== true) return;

  const rowIndex = rows.value.findIndex(row => row.id === id);
  if (rowIndex !== -1) {
    const row = rows.value[rowIndex];
    const actualBillableHours = (parseFloat(billablePercent) / 100) * parseFloat(row.hours || 0);
    row.billableHours = parseFloat(actualBillableHours.toFixed(2)); // Save this to DB
    row.billablePercent = billablePercent; // Only for UI
    rows.value = [...rows.value];
  }

  setTimeout(() => {
    timesheetService.updateBillableHrs(id, rows.value[rowIndex].billableHours).then(() => {
      notifySuccess({ message: "Billable hours updated successfully." });
      getGroupedBillableTimesheets({ pagination: pagination.value });
    });
  });
}
function validateHours (value) {
  const num = parseFloat(value);
  if (isNaN(num)) return "Please enter a number.";
  if (num < 1 || num > 100) return "Invalid Billable hours.";
  const regex = /^\d{1,3}(\.\d{1,2})?$/; // Allows up to 2 decimal places
  if (!regex.test(value.toString())) return "Only 2 decimal places allowed.";
  return true;
}

function disableBeforeStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!search.value.fromDate) {
    return true;
  }

  const start = parse(search.value.fromDate, "MM/dd/yyyy", new Date());
  const currentDate = parse(date, "yyyy/MM/dd", new Date());

  // Disable dates before the Start Date
  return currentDate >= start;
}

const downloadExcel = () => {
  const filteredColumns = columns.value.filter(col => col.label !== "Billable Hours(Percent)");
  const payload2 = {
    timesheetDataModel: rows.value,
    columns: filteredColumns
  };
  timesheetService.exportBillingTimesheet(payload2).then(response => {
    // Create a Blob URL
    const url = window.URL.createObjectURL(new Blob([response]));
    const link = document.createElement("a");
    link.href = url;

    // Set the file name
    link.setAttribute("download", "BillingTimesheet.xlsx");

    // Trigger download
    document.body.appendChild(link);
    link.click();

    // Clean up
    link.remove();
  });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "project.name", label: "Project Name", field: "ProjectId", align: "left", sortable: true, type: "P" },
  { name: "projectModule.name", label: "Module Name", field: "projectModule.name", align: "left", sortable: true, type: "PM" },
  { name: "task.name", label: "Task", field: "task.name", align: "left", sortable: true, type: "PT" },
  { name: "createdById", label: "Created By", field: "createdById", align: "left", sortable: true, type: "U" },
  { name: "hours", label: "Actual Hours", field: "hours", align: "right", sortable: true, checkedStatus: true, type: "H" },
  { name: "actualHours", label: "Cal Hours", field: "actualHours", align: "right", sortable: true, checkedStatus: true, type: "CH" },
  { name: "billableHours", label: "Billable Hours(Percent)", field: "billableHours", align: "right", sortable: false }
]);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------
// Search records as per parameters
const onAdvanceSearch = () => { refreshBillableTimesheetList(); };

// Search variables
const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

const search = ref({
  searchText: getFilterValue("searchText", ""),
  fromDate: getFilterValue("fromDate", defaultFromDate),
  toDate: getFilterValue("toDate", defaultToDate),
  projectId: getFilterValue("projectId", null),
  projectModuleIds: getFilterValue("projectModuleIds", []),
  projectTaskIds: getFilterValue("projectTaskIds", []),
  customerIds: getFilterValue("customerIds", []),
  companyContactIds: getFilterValue("companyContactIds", [])
});

// Clear search
const onAdvanceClear = () => {
  search.value.fromDate = defaultFromDate;
  search.value.toDate = defaultToDate;
  search.value.projectId = null;
  search.value.projectModuleIds = [];
  search.value.projectTaskIds = [];
  search.value.customerIds = [];
  search.value.companyContactIds = [];
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

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

const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...(search.value.fromDate ? { "Start Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "End Date": search.value.toDate } : {}),
  ...mapSingleFilterToLabel(search.value.projectId, projectNameDropdownSingleSelect.list, "Project Name"),
  ...mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Module"),
  ...mapFilterToLabel(search.value.projectTaskIds, projectTasksByProjectIdAndModuleIdForDropdown.list, "Project Task"),
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.companyContactIds, companyContactNameDropdown.list, "Company Contact")
}));

function onClearFilters (key) {
  if (key === "Start Date") {
    search.value.fromDate = "";
  } else if (key === "End Date") {
    search.value.toDate = "";
  } else if (key === "Project Name") {
    search.value.projectId = "";
  } else if (key === "Project Module") {
    search.value.projectModuleIds = [];
  } else if (key === "Project Task") {
    search.value.projectTaskIds = [];
  } else if (key === "Customer") {
    search.value.customerIds = [];
  } else if (key === "Company Contact") {
    search.value.companyContactIds = [];
  }
  delete appliedFilters.value[key];
  refreshBillableTimesheetList();
}

function getFilterCount (key) {
  switch (key) {
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Project Task": return search.value.projectTaskIds?.length || 0;
  case "Customer": return search.value.customerIds?.length || 0;
  case "Company Contact": return search.value.companyContactIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect } = projectModule();
const { projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { projectTasksByProjectIdAndModuleIdForDropdown } = projectTaskModule();
const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();

// ----------------------------------------------------------------------------------------------------------------
// Watch on change
// ----------------------------------------------------------------------------------------------------------------
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshBillableTimesheetList();
});

watch(() => search.value.projectId, async (newValue, oldValue) => {
  if (!newValue || newValue === oldValue) return;

  search.value.projectModuleIds = null;
  await projectModulesByProjectIdForDropdown.load(false, false, search.value.projectId);
}, { immediate: true });

watch(
  () => search.value.projectModuleIds,
  (newValue, oldValue) => {
    if (newValue === oldValue) return;
    search.value.projectTaskIds = [];
    const { projectId } = search.value;
    if (!projectId || !newValue?.length) return;
    projectTasksByProjectIdAndModuleIdForDropdown.load(false, projectId, newValue);
  }
);

watch(() => search.value.customerIds, (newValue, oldValue) => {
  if (search.value?.customerIds?.length === 0 || newValue === oldValue) return;

  companyContactNameDropdown.load(newValue);
}, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------
onMounted(() => {
  tableRef.value.requestServerInteraction();
  projectNameDropdownSingleSelect.load();
  if (search.value.projectId) projectModulesByProjectIdForDropdown.load(false, false, search.value.projectId);
  if (search.value.projectId && search.value.projectModuleIds?.length > 0) projectTasksByProjectIdAndModuleIdForDropdown.load(false, search.value.projectId, search.value.projectModuleIds);
  customerNameDropdown.load();
  companyContactNameDropdown.load();
});

</script>

<style>
 .billableHrsInput {
    width: 80% !important;
    padding: 0 !important;
  }

  .billableHrsInput input {
    padding: 0 5px !important;
     text-align: right;
  }
</style>
