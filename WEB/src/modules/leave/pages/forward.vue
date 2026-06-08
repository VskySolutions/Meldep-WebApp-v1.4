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
              <q-breadcrumbs-el label="Org Management" />
              <q-breadcrumbs-el label="Forward Leave List" />
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
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.employeeIds"
                        label="Employee Name"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Leave Status</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.statusIds" push class="q-mx-sm w-100 h-auto" use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="leaveStatusList" option-value="value" option-label="text" emit-value map-options @filter="filterFn2"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Applied Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.createdOnUtc" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.createdOnUtc" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Month</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.leaveMonthStr" fill-input dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" v-model="isPopupLeaveMonthVisible" transition-show="scale" transition-hide="scale">
                                    <q-date ref="date4ref" v-model="search.leaveMonthStr" default-view="Months" emit-immediately minimal mask="MMMM" class="myDate" @update:model-value="onUpdateMv3" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Year</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.years" fill-input dense mask="####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" v-model="isPopupVisible" transition-show="scale" transition-hide="scale">
                                    <q-date ref="date3ref" v-model="search.years" default-view="Years" emit-immediately minimal mask="YYYY" class="myDate" :options="onlyYears" @update:model-value="onUpdateMv2" />
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
        @request="getEmployeeLeaves"
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
            <q-td style="width: 15%;">{{ props.row.employee.person.firstName + " "+ props.row.employee.person.lastName }}</q-td>
            <q-td class="text-center">
              <div class="row justify-center items-center no-wrap">
                <!-- If same date -->
                <span v-if="props.row.fromDate === props.row.toDate">
                  {{ props.row.fromDate }}
                </span>
                <template v-else>
                  <span>{{ props.row.fromDate }}</span>
                  <div
                    v-if="props.row.noofLeaves > 1"
                    class="q-mx-sm bg-primary text-white rounded-borders flex flex-center"
                    style="width: 20px; height: 20px; font-size: 12px;"
                  >
                    <q-tooltip>
                      No. of leaves
                    </q-tooltip>
                    {{ props.row.noofLeaves }}
                  </div>
                  <span>{{ props.row.toDate }}</span>
                  <span>
                    <q-icon
                      v-if="props.row.isSandwich"
                      name="o_menu"
                      size="sm"
                      class="q-ml-xs cursor-pointer"
                      color="primary"
                      @click="onLeaveView(props.row.id, 'forward', refreshForwardLeaveList)"
                    />
                    <q-tooltip>
                      This is the sandwich leave
                    </q-tooltip>
                  </span>
                </template>
              </div>
            </q-td>
            <q-td class="text-center">
              <q-badge
                v-if="props.row.halfDayType"
                color="primary"
                class="q-pa-xs"
              >
                {{ props.row.halfDayType }}
              </q-badge>
              <q-badge
                v-else
                color="secondary"
                class="q-pa-xs"
              >
                Full Day
              </q-badge>
            </q-td>
            <q-td class="text-center">
              <q-chip v-if="props.row.leaveCategories.dropDownValue == 'Casual'" label="Casual" name="o_done" class="rounded q-px-lg" color="green-5" text-color="black" />
              <q-chip v-if="props.row.leaveCategories.dropDownValue == 'Sick'" label="Sick" name="o_done" class="rounded q-px-lg" color="red-5" text-color="black" />
            </q-td>
            <q-td class="text-center" style="width: 10%;">{{ props.row.createdOnUtc }}</q-td>
            <q-td style="width: 15%;">
              <!-- <q-chip v-if="props.row.createdOnUtc && props.row.leaveStatuses.dropDownValue == 'Applied'" :label="isRecent(props.row.createdOnUtc) ? 'Applied' : 'Waiting for Send To Approver'" name="o_done" class="rounded q-px-lg" color="light-blue-5" text-color="black" /> -->
              <q-chip v-if="props.row.createdOnUtc && props.row.leaveStatuses.dropDownValue == 'Applied'" label="Waiting to Send for Approver" name="o_done" class="rounded q-px-lg" color="light-blue-5" text-color="black" />
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Cancelled'" label="Cancelled" name="o_done" class="rounded q-px-lg" color="grey-5" text-color="black" />
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Sent to Approver'" label="Sent to Approver" name="o_done" class="rounded q-px-lg" color="yellow-5" text-color="black" />
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Approved'" label="Approved" name="o_done" class="rounded q-px-lg" color="green-5" text-color="black" />
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Decline'" label="Decline" name="o_done" class="rounded q-px-lg" color="red-5" text-color="black" />
            </q-td>
            <!-- <q-td style="width: 15%;">{{ props.row.reason }}</q-td> -->
            <q-td style="width: 15%;" class="RichTextEditor"><div v-html="stripHTML(truncate(props.row.reason, 50))" /></q-td>
            <q-td auto-width class="text-center actions" style="width: 10%;">
              <q-icon v-if="props.row.leaveStatuses.dropDownValue == 'Applied'" name="o_double_arrow" class="cursor-pointer q-mr-sm" @click="onLeaveForward(props.row.id, refreshForwardLeaveList)">
                <q-tooltip>Forward to Approver</q-tooltip>
              </q-icon>
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onLeaveView(props.row.id, 'forward', refreshForwardLeaveList)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.leaveStatuses.dropDownValue != 'Cancelled' && props.row.leaveStatuses.dropDownValue != 'Decline'" name="o_cancel" class="cursor-pointer" color="negative" @click="onCancelLeave(props.row)">
                <q-tooltip>Cancel</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>

<script setup>
// Import libraries
import { ref, onMounted, computed, watch } from "vue";
import { useAuthStore } from "stores/auth";
import { setLocalStorage, clearLocalStorage, getLocalStorage, notifySuccess, zwConfirm } from "assets/utils";

import leaveService from "modules/leave/leave.service";
import commonService from "services/common.service";
import useFilters from "composables/useFilters";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Leave Dialogs
import {
  initLeaveDialogs,
  onLeaveForward,
  onLeaveView
} from "src/modules/leave/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const { truncate, stripHTML, toDate } = useFilters();
const loading = ref(true);
const date3ref = ref(null);
const isPopupVisible = ref(false);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const user = authStore.user;

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Employee Leave";
const filterLocalStorage = getLocalStorage(localStorageKey);
const createdOnUtc = filterLocalStorage ? filterLocalStorage.createdOnUtc : "";
const leaveMonthStr = filterLocalStorage ? filterLocalStorage.leaveMonthStr : "";
const years = filterLocalStorage ? filterLocalStorage.years : getCurrentMonthYear();
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshForwardLeaveList = () => {
  getEmployeeLeaves({ pagination: pagination.value });
};

const onlyYears = (date) => {
  // Allows only years to be selectable
  return true; // No restriction, allowing all years
};

const onUpdateMv2 = (val) => {
  // Update the selected year and close the popup
  search.value.years = val; // Update the reactive property with the selected year
  isPopupVisible.value = false; // Close the popup
};

function getCurrentMonthYear () {
  const today = new Date();
  const year = today.getFullYear();
  return `${year}`; // Format as 'Month-YYYY'
}

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "employee.person.firstName", label: "Employee Name", field: "employee.person.firstName", align: "left", sortable: true },
  { name: "fromDate", label: "Leave Date", field: "fromDate", align: "center", sortable: true },
  { name: "halfDayType", label: "Half Day/Full Day", field: "halfDayType", align: "center", sortable: true },
  { name: "LeaveCategories.dropDownValue", label: "Leave Type", field: "LeaveCategories.dropDownValue", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Applied On", field: "createdOnUtc", align: "center", sortable: true },
  { name: "leaveStatuses.dropDownValue", label: "Leave Status", field: "leaveStatuses.dropDownValue", align: "left", sortable: true },
  { name: "reason", label: "Reason", field: "reason", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Employee forward Leaves
// ----------------------------------------------------------------------------------------------------------------

const getEmployeeLeaves = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.flag = "FD";
  search.value.createdOnUtc = search.value.createdOnUtc === "" ? null : toDate(search.value.createdOnUtc);
  if (search.value.createdOnUtc === "") {
    search.value.createdOnUtc = null;
  }
  // Validate month
  search.value.leaveMonthStr = (m => Array.from({ length: 12 }, (_, i) => new Date(0, i).toLocaleString("default", { month: "long" }))
    .includes(m) ? m : "")(search.value.leaveMonthStr);
  search.value.years = search.value.years === "" ? getCurrentMonthYear() : search.value.years;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  leaveService.getEmployeeLeaves(payload).then((resp) => {
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

const date4ref = ref(null);
const currentView = ref("Months");
const isPopupLeaveMonthVisible = ref(false);
const onUpdateMv3 = (val) => {
  search.value.leaveMonthStr = val;
  currentView.value = "Months";
  isPopupLeaveMonthVisible.value = false;
};

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
  employeeIds: getFilterValue("employeeIds", []),
  statusIds: getFilterValue("statusIds", []),
  leaveCategoryId: getFilterValue("leaveCategoryId", []),
  createdOnUtc,
  leaveMonthStr: leaveMonthStr,
  years
});

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshForwardLeaveList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.employeeIds = [];
  search.value.statusIds = [];
  search.value.createdOnUtc = "";
  search.value.leaveMonthStr = "";
  search.value.years = getCurrentMonthYear();
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

const onCancelLeave = (item) => {
  activeRowId.value = item.id;
  zwConfirm({ data: `${item.employee.person.firstName}` }, () => {
    leaveService.cancelEmployeeLeave(item.id).then(resp => {
      notifySuccess({ message: "Leave Cancelled successfully." });
      refreshForwardLeaveList();
    });
  }, () => {
    activeRowId.value = null;
  });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------
// Need to update

// Get leave status List
const leaveStatusList = ref([]);
const options2 = ref([]);
function getDropDownStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue === "Applied" ? "Waiting to Send for Approver" : item.dropdownValue, value: item.id }));
    // const responseData = resp.map((item) => ({ text: item.dropdownValue === "Sent to Approver" ? "Sent to Approver" : item.dropdownValue === "Applied" ? "Waiting for Approval" : item.dropdownValue, value: item.id }));
    leaveStatusList.value = responseData;
    options2.value = responseData;
  });
}

// Search task status List for dropdown
function filterFn2 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      leaveStatusList.value = options2.value;
    } else {
      leaveStatusList.value = options2.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { activeEmployeesDropdown } = employeeModule();

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initLeaveDialogs(activeRowId);

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
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Employee Name"),
  ...mapFilterToLabel(search.value.statusIds, leaveStatusList, "Leave Status"),
  ...(search.value.createdOnUtc ? { "Applied Date": search.value.createdOnUtc } : {}),
  ...(search.value.leaveMonthStr ? { Month: search.value.leaveMonthStr } : {}),
  ...(search.value.years ? { Year: search.value.years } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Employee Name": return search.value.employeeIds?.length || 0;
  case "Leave Status": return search.value.statusIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Employee Name") {
    search.value.employeeIds = [];
  } else if (key === "Leave Status") {
    search.value.statusIds = [];
  } else if (key === "Applied Date") {
    search.value.primaryEmailAddress = "";
  } else if (key === "Month") {
    search.value.leaveMonthStr = "";
  }
  delete appliedFilters.value[key];
  refreshForwardLeaveList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshForwardLeaveList();
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdown.load(user.siteId);
  getDropDownStatus("Leave Status");
  refreshForwardLeaveList();
});

</script>
