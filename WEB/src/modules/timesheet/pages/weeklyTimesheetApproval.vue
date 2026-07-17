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
              <q-breadcrumbs-el label=" Weekly Timesheet Approval List" />
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
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <singleSelectDropdown
                        v-model="search.employeeId"
                        label="Employee Name"
                        :options="activeEmployeesDropdownSingleSelect.list.value"
                        :filter="activeEmployeesDropdownSingleSelect.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.timesheetStatusIds"
                        label="Timesheet Status"
                        :options="timesheetStatusForDropdown.list.value"
                        :filter="timesheetStatusForDropdown.filter"
                        :isShowAll="true"
                      />
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
            </div>
          </div>
        </div>
      </q-card-section>
      <q-table
          ref="tableRef"
          v-model:pagination="pagination"
          :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
          :rows-per-page-options="[20, 50, 100, 200, 500]"
          :loading="loading"
          :rows="rows"
          :columns="columns"
          row-key="id"
          separator="cell"
          no-data-label="No data available"
          binary-state-sort
          @request="getAllWeeklyTimesheetApprovalList"
        >
          <template #loading>
            <q-inner-loading showing color="primary">
              <q-spinner-ios size="40px" class="q-mt-xl" />
            </q-inner-loading>
          </template>
          <template #header="props">
            <q-tr :props="props" class="bg-primary text-white">
              <q-th v-for="col in props.cols" :key="col.name" :props="props">
                {{ col.label }}
              </q-th>
            </q-tr>
          </template>
            <template #body="props">
          <q-tr
            :props="props"
          >
            <q-td style="width: 100%;" class="hoverable-cell">
              <div class="row items-center q-gutter-sm">
                <!-- Expand / Collapse Icon -->
                <q-icon
                  :name="isExpanded(props.row.id) ? '-' : '+'"
                  class="cursor-pointer custom-plus-minus-icon"
                  @click.stop="toggleExpand(props.row.id)"
                >
                  <q-tooltip>{{ isExpanded(props.row.id) ? 'Collapse' : 'Expand' }}</q-tooltip>
                </q-icon>
                <span>
                  {{ props.row.weekEndDate }} - {{ props.row.employee.fullName }}
                </span>
                <q-btn
                  label="Approve"
                  color="primary"
                  type="button"
                  no-caps
                  size="sm"
                  class="text-h1"
                  @click.stop="openApprovalDialog(props.row)"
                >
                  <q-tooltip>Approve Timesheet</q-tooltip>
                </q-btn>
                <q-badge
                  rounded
                  class="text-h7 q-px-sm q-py-sm"
                  :style="{
                    color: props.row.timesheetStatus.color,
                    background: props.row.timesheetStatus.bgColor
                  }"
                >
                  {{ props.row.timesheetStatus.dropDownValue }}
                </q-badge>
              </div>
            </q-td>
          </q-tr>

          <!-- Expanded Row (all other columns) -->
          <q-tr v-if="isExpanded(props.row.id)" class="expanded-row">
            <q-td colspan="100%" class="q-pa-none">
              <div class="q-table__expanded-row bg-primary">
                <q-table
                  flat
                  bordered
                  class="q-pa-sm"
                  row-key="id"
                  :rows="props.row.timesheetLines"
                  :columns="timesheetLineColumns"
                  :pagination="timesheetLinePagination[props.row.id]"
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                  @update:pagination="val => {
                    timesheetLinePagination = {
                      ...timesheetLinePagination,
                      [props.row.id]: { ...val }
                    }
                  }"
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-grey-4 text-black">
                      <q-th
                        v-for="col in props.cols"
                        :key="col.name"
                        :props="props"
                      >
                        {{ col.label }}
                      </q-th>
                    </q-tr>
                  </template>
                  <template #body="lineProps">
                    <q-tr :props="lineProps">
                      <q-td class="common-q-td" style="width: 10%;">
                        {{ lineProps.row.date }}
                      </q-td>
                      <q-td class="common-q-td" style="width: 20%;">
                        {{ lineProps.row.project }}
                      </q-td>
                      <q-td class="common-q-td" style="width: 25%;">
                        {{ lineProps.row.task }}
                      </q-td>
                      <q-td class="text-right common-q-td" style="width: 5%;">
                        {{ lineProps.row.hours }}
                      </q-td>
                      <q-td class="common-q-td text-center" style="width: 5%;">
                        <q-checkbox
                          v-model="lineProps.row.isApproved"
                          :disable="lineProps.row.isReadOnly"
                        />
                      </q-td>
                      <q-td class="common-q-td" style="width: 40%;">
                        <p v-html="lineProps.row.description" class="q-ma-none" />
                      </q-td>
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </q-td>
          </q-tr>
        </template>
      </q-table>
      <q-dialog v-model="approvalDialog">
        <q-card>
         <q-card-section class="text-h6 text-primary row items-center justify-between">
          <div class="text-h1">
            Confirmation
          </div>
          <q-btn
            icon="o_close"
            class="close"
            color="primary"
            flat
            round
            dense
            @click="approvalDialog = false"
          />
         </q-card-section>
          <q-separator />
          <q-card-section>
            <div class="q-mb-md">
              Are you sure you want to approve or decline this weekly timesheet?
            </div>
            <!-- <div class="text-subtitle2 q-mb-sm">
              Approver/Decline Note
            </div>
            <q-input
              v-model="approverNote"
              type="textarea"
              outlined
            /> -->
          </q-card-section>
          <q-separator />
          <q-card-actions align="right">
            <q-btn
              label="Decline"
              color="negative"
              flat
              @click="onSubmitTimesheetApproval(
                selectedWeek,
                'Declined',
                refreshWeeklyTimesheetApprovalList,
                () => { approvalDialog = false; }
              )"
            />
            <q-btn
              label="Approve"
              color="primary"
              flat
              :disable="hasDeclinedLines"
              @click="onSubmitTimesheetApproval(
                selectedWeek,
                'Approved',
                refreshWeeklyTimesheetApprovalList,
                () => { approvalDialog = false; }
              )"
            />
          </q-card-actions>
        </q-card>
      </q-dialog>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, watch, computed } from "vue";
import { useAuthStore } from "stores/auth";

import timesheetService from "modules/timesheet/timesheet.service";
import useFilters from "composables/useFilters";

// Shared Dropdowns
import useSiteTableState from "composables/dataTable/useSiteTableState.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import timesheetModule from "src/modules/timesheet/utils/dropdowns.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// Shared timesheet Actions
import {
  initTimesheetActions,
  onSubmitTimesheetApproval
} from "src/modules/timesheet/utils/actions.js";

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Common variables
// --------------------------------------------------------------------------------------------------------------------------------------------------
const approvalDialog = ref(false);
const approverNote = ref("");
const selectedWeek = ref(null);
const siteId = computed(() => authStore.user?.siteId);
const authStore = useAuthStore();
const showFilter = ref(false);
const searchLoader = ref(false);
const user = authStore.user;

const defaultSearch = {
  searchText: "",
  employeeId: null,
  timesheetStatusIds: [],
  fromDate: "",
  stoDate: "",
};

const defaultPagination = {
  sortBy: "date",
  descending: false,
  rowsPerPage: 20,
  page: 1
};

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState,
  getTableState
} = useSiteTableState({
  storageKey: "weekly-Timesheet-Approval-Index",
  siteId,
  defaultSorts: {}
});

const tableState = getTableState();
const expandedRows = ref(
  Array.isArray(tableState?.expandedRows)
    ? [...tableState.expandedRows]
    : []
);

const timesheetLinePagination = ref(
  typeof tableState?.rowPagination === "object" &&
  !Array.isArray(tableState?.rowPagination)
    ? { ...tableState.rowPagination }
    : {}
);

const openApprovalDialog = (row) => {
  selectedWeek.value = row;
  approverNote.value = "";
  approvalDialog.value = true;
};

const hasDeclinedLines = computed(() => {
  if (!selectedWeek.value?.timesheetLines) {
    return false;
  }
  return selectedWeek.value.timesheetLines.some(line => !line.isApproved);
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------
const loading = ref(true);
const rows = ref([]);
const columns  = ref([
  { name: "weekEndDate", label: "Week End Date - Employee Name", field: "timesheetDate", align: "left", sortable: true }
]);

const timesheetLineColumns = ref([
  { name: "date", label: "Date", field: row => row.date, align: "left", sortable: true },
  { name: "project", label: "Project", field: row => row.project, align: "left", sortable: true },
  { name: "task", label: "Task", field: row => row.task, align: "left", sortable: true },
  { name: "hours", label: "Hours", field: row => row.hours, align: "right", sortable: true },
  { name: "isApproved", label: "Approval/Decline", field: row => row.isApproved, align: "center", sortable: true },
  { name: "description", label: "Description", field: row => row.description, align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Weekly Timesheet Approval List
// ----------------------------------------------------------------------------------------------------------------
const getAllWeeklyTimesheetApprovalList = ({ pagination: tablePagination }) => {
  const { page, rowsPerPage, sortBy, descending } = tablePagination;
  loading.value = true;
  search.value.weekEndDate = search.value.weekEndDate ? toDate(search.value.weekEndDate) : null;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    ...search.value
  };
  timesheetService.getWeeklyTimesheetApprovalList(payload)
    .then((resp) => {
        rows.value = (resp.data || []).map(row => ({
          ...row,
          timesheetLines: (row.timesheetLines || []).map(line => ({
            ...line,
            isApproved: true,
            isReadOnly: row.timesheetStatus.dropDownValue === 'Resubmitted' && line.isApproved
          }))
        }));

      pagination.value = {
        ...pagination.value,
        page,
        rowsPerPage,
        sortBy,
        descending,
        rowsNumber: resp.total
      };
      saveDataTableState({
        search: search.value,

        pagination: {
          ...pagination.value,
          page,
          rowsPerPage,
          sortBy,
          descending,
          rowsNumber: resp.total
        },

        activeRowId: activeRowId.value,
        // sorts: sortsObj
      });
    })
    .finally(() => {
      loading.value = false;
      searchLoader.value = false;
    });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel
// ------------------------------------------------------------------------------------

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshWeeklyTimesheetApprovalList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.employeeId = null;
  search.value.timesheetStatusIds = [];
  search.value.fromDate = null;
  search.value.toDate = null;
  saveDataTableState({
    search: {
      ...defaultSearch
    },

    pagination: {
      ...defaultPagination
    },

    activeRowId: null,
    sorts: {}
  });
  onAdvanceSearch();
};

const refreshWeeklyTimesheetApprovalList = () => {
  getAllWeeklyTimesheetApprovalList({ pagination: pagination.value });
};

const toggleExpand = (id) => {
  const index = expandedRows.value.indexOf(id);

  if (index > -1) {
    expandedRows.value.splice(index, 1);
  } else {
    expandedRows.value.push(id);
  }

};

const isExpanded = (id) => expandedRows.value.includes(id);

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initTimesheetActions(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { activeEmployeesDropdownSingleSelect } = employeeModule();
const { timesheetStatusForDropdown } = timesheetModule();

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

const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapSingleFilterToLabel(search.value.employeeId, activeEmployeesDropdownSingleSelect.list, "Employee Name"),
  ...mapFilterToLabel(search.value.timesheetStatusIds, timesheetStatusForDropdown.list, "Timesheet Status"),
  ...(search.value.fromDate ? { "From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "To Date": search.value.toDate } : {})
}));

function onClearFilters (key) {
  if (key === "Employee Name") {
    search.value.employeeId = null;
  } else if (key === "Timesheet Status") {
    search.value.timesheetStatusIds = [];
  } else if (key === "From Date") {
    search.value.fromDate = "";
  } else if (key === "To Date") {
    search.value.toDate = "";
  }
  delete appliedFilters.value[key];
  refreshWeeklyTimesheetApprovalList();
}

function getFilterCount (key) {
  switch (key) {
  case "Timesheet Status": return search.value.timesheetStatusIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshWeeklyTimesheetApprovalList();
});

watch(
  timesheetLinePagination,
  (val) => {
    saveDataTableState({
      rowPagination: JSON.parse(JSON.stringify(val))
    });
  },
  { deep: true }
);

// ----------------------------------------------------------------------------------------------------------------
// Expanded Rows Save
// ----------------------------------------------------------------------------------------------------------------
watch(
  expandedRows,
  (val) => {
    saveDataTableState({
      expandedRows: [...val]
    });
  },
  { deep: true }
);

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  timesheetStatusForDropdown.load("Timesheet Status");
  activeEmployeesDropdownSingleSelect.load(user.siteId);
  refreshWeeklyTimesheetApprovalList();
});

</script>
<style>
.my-sticky-header-table thead tr {
  z-index: 5;
}

.q-table__expanded-row .q-table thead tr {
  z-index: 1;
}
</style>
