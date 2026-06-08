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
              <q-breadcrumbs-el label="Approve Leaves" />
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
                      <multiSelectDropdown
                        v-model="search.leaveCategoryId"
                        label="Leave Type"
                        :options="leaveCategoryDropdown.list.value"
                        :filter="leaveCategoryDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Leave Status"
                        :options="leaveStatusDropdown.list.value"
                        :filter="leaveStatusDropdown.filter"
                        :isShowAll="true"
                      />
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
              <q-menu v-model="showMultiSelectOptions" anchor="bottom right" self="top right" persistent no-parent-event style="width: 300px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Click one of the options below</div><q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in selectedFieldOptions"
                      :key="opt.value"
                      clickable
                      :active="selectedField === opt.value"
                      active-class="bg-primary text-white"
                      @click="onLeaveApproveOrDecline(opt.value)"
                    >
                      <q-item-section avatar>
                        <q-icon
                          :name="opt.icon"
                          size="xs"
                          class="cursor-pointer"
                          :color="selectedField === opt.value ? 'white' : 'grey-7'"
                        />
                      </q-item-section>
                      <q-item-section>{{ opt.label }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <q-btn
                icon="o_checklist"
                outline
                no-caps
                class="text-primary btnRounded q-ml-sm"
                :disabled="multiSelectLeaveIds.length === 0" @click.stop="showMultiSelectOptions = !showMultiSelectOptions"
              >
                <q-badge
                  v-if="multiSelectLeaveIds?.length > 0"
                  :label="multiSelectLeaveIds.length"
                  class="primary"
                  floating
                />
                <q-tooltip>Approve/Decline leave status</q-tooltip>
              </q-btn>
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
            <q-th auto-width class="text-center" />
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td class="text-center" style="width: 3%;">
              <q-checkbox
                v-if="props.row.leaveStatuses.dropDownValue === 'Sent to Approver'"
                v-model="props.row.checkboxStatus"
                @update:model-value="onSelectCheckbox(props.row.id, $event)"
              />
            </q-td>
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
                  ><q-tooltip>
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
                      @click="onLeaveView(props.row.id, 'approve', refreshApproveLeaveList)"
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
            <q-td style="width: 14%;">
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Applied'" label="Applied" name="o_done" class="rounded q-px-lg" color="light-blue-5" text-color="black" />
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Cancelled'" label="Cancelled" name="o_done" class="rounded q-px-lg" color="grey-5" text-color="black" />
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Sent to Approver'" label="Waiting for Approval" name="o_done" class="rounded q-px-lg" color="yellow-5" text-color="black" />
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Approved'" label="Approved" name="o_done" class="rounded q-px-lg" color="green-5" text-color="black" />
              <q-chip v-if="props.row.leaveStatuses.dropDownValue == 'Decline'" label="Decline" name="o_done" class="rounded q-px-lg" color="red-5" text-color="black" />
            </q-td>
            <q-td class="text-center" style="width: 8%;">{{ props.row.createdOnUtc }}</q-td>
            <q-td style="width: 22%;"><div v-html="stripHTML(truncate(props.row.reason, 50))" /></q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon v-if="props.row.leaveStatuses.dropDownValue == 'Sent to Approver'" name="o_check_box" class="cursor-pointer q-mr-sm" @click="onLeaveApprove(props.row.id, refreshApproveLeaveList)">
                <q-tooltip>Leave Approve/Decline</q-tooltip>
              </q-icon>
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onLeaveView(props.row.id, 'approve', refreshApproveLeaveList)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.leaveStatuses.dropDownValue == 'Approved'" name="o_cancel" class="cursor-pointer" color="negative" @click="onCancelLeave(props.row)">
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
import { setLocalStorage, getLocalStorage, clearLocalStorage, notifySuccess, notifyError, zwConfirm } from "assets/utils";

import useFilters from "composables/useFilters";

import leaveService from "../leave.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import leaveModule from "src/modules/leave/utils/dropdowns.js";

// Shared Leave Dialogs
import {
  initLeaveDialogs,
  onLeaveApprove,
  onLeaveView
} from "src/modules/leave/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const { truncate, stripHTML, toDate } = useFilters();
const date3ref = ref(null);
const isPopupVisible = ref(false);
const showFilter = ref(false);
const searchLoader = ref(false);
const multiSelectLeaveIds = ref([]);
const showMultiSelectOptions = ref(false);

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  leaveStatusFlag: "",
  flag: ""
});

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Approve Leave";
const filterLocalStorage = getLocalStorage(localStorageKey);
const createdOnUtc = filterLocalStorage ? filterLocalStorage.createdOnUtc : "";
const leaveMonthStr = filterLocalStorage ? filterLocalStorage.leaveMonthStr : "";
const years = filterLocalStorage ? filterLocalStorage.years : getCurrentMonthYear();
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

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
  { name: "leaveStatuses.dropDownValue", label: "Leave Status", field: "leaveStatuses.dropDownValue", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Applied On", field: "createdOnUtc", align: "center", sortable: true },
  { name: "reason", label: "Reason", field: "reason", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Employee Approve Leaves
// ----------------------------------------------------------------------------------------------------------------

const getEmployeeLeaves = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
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
  const storedLeaveIds = localStorage.getItem("selectedRequirementIds") || [];
  leaveService.getEmployeeLeavesForApprove(payload).then((resp) => {
    rows.value = resp.data.map(leave => ({
      ...leave,
      checkboxStatus: storedLeaveIds.includes(leave.id)
    }));
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

const refreshApproveLeaveList = () => {
  getEmployeeLeaves({ pagination: pagination.value });
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
  leaveMonthStr,
  years
});

const onAdvanceSearch = () => {
  refreshApproveLeaveList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.employeeIds = [];
  search.value.statusIds = [];
  search.value.createdOnUtc = "";
  search.value.leaveMonthStr = "";
  search.value.years = getCurrentMonthYear();
  search.value.leaveCategoryId = [];
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { activeEmployeesDropdown } = employeeModule();
const {
  leaveStatusDropdown,
  leaveCategoryDropdown
} = leaveModule();

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initLeaveDialogs(activeRowId);

const onSelectCheckbox = (leaveId, flag) => {
  if (flag === true) {
    if (!multiSelectLeaveIds.value.includes(leaveId)) {
      multiSelectLeaveIds.value.push(leaveId); // add leave ID
    }
  } else {
    multiSelectLeaveIds.value = multiSelectLeaveIds.value.filter(id => id !== leaveId); // remove leave ID
  }

  // Optional: save to localStorage if needed
  localStorage.setItem("selectedLeaveIds", JSON.stringify(multiSelectLeaveIds.value));
};

const selectedFieldOptions = [
  { label: "Approve", value: "approve", icon: "o_check_circle" },
  { label: "Decline", value: "decline", icon: "o_cancel" }
];

const onLeaveApproveOrDecline = async (action) => {
  try {
    model.value.flag = "AV";
    model.value.leaveStatusFlag = action === "approve" ? "AV" : "DC"; // set status based on action

    zwConfirm(
      { data: `${model.value}` },
      async () => {
        loading.value = true;
        // Call the service (same for approve or decline)
        await leaveService.saveEmployeeLeaveDetails(multiSelectLeaveIds.value, model.value);
        notifySuccess({
          message: action === "approve" ? "Leave Approved" : "Leave Declined"
        });
        loading.value = false;
        showMultiSelectOptions.value = false;
        multiSelectLeaveIds.value = [];
        refreshApproveLeaveList();
      },
      () => {
        loading.value = false; // user canceled confirm
      }
    );
  } catch (error) {
    console.error("Error in leave action:", error);
    notifyError({ message: "An error occurred while saving." });
  }
};

const onCancelLeave = (item) => {
  activeRowId.value = item.id;
  zwConfirm({ data: `${item.employee.person.firstName}` }, () => {
    leaveService.cancelEmployeeLeave(item.id).then(resp => {
      notifySuccess({ message: "Leave Cancelled successfully." });
      refreshApproveLeaveList();
    });
  }, () => {
    activeRowId.value = null;
  });
};

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
  ...mapFilterToLabel(search.value.statusIds, leaveStatusDropdown.list, "Leave Status"),
  ...mapFilterToLabel(search.value.leaveCategoryId, leaveCategoryDropdown.list, "Leave Type"),
  ...(search.value.createdOnUtc ? { "Applied Date": search.value.createdOnUtc } : {}),
  ...(search.value.leaveMonthStr ? { Month: search.value.leaveMonthStr } : {}),
  ...(search.value.years ? { Year: search.value.years } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Employee Name": return search.value.employeeIds?.length || 0;
  case "Leave Status": return search.value.statusIds?.length || 0;
  case "Leave Type": return search.value.leaveCategoryId?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Employee Name") {
    search.value.employeeIds = [];
  } else if (key === "Leave Status") {
    search.value.statusIds = [];
  } else if (key === "Leave Type") {
    search.value.leaveCategoryId = [];
  } else if (key === "Month") {
    search.value.leaveMonthStr = "";
  }
  delete appliedFilters.value[key];
  refreshApproveLeaveList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshApproveLeaveList();
});

watch(multiSelectLeaveIds, () => {
  if (multiSelectLeaveIds.value.length === 0) {
    showMultiSelectOptions.value = false;
  }
}, { deep: true });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdown.load();
  leaveStatusDropdown.load("Leave Status");
  leaveCategoryDropdown.load("Leave Category");
  getCurrentMonthYear();
  refreshApproveLeaveList();
});
</script>
