<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <!-- Header -->
    <q-card-section class="row items-center justify-end q-pb-sm">
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

      <div class="row items-center q-gutter-sm">
        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/timesheet',
            state: {
              projectId: projectId
            }
          })"
        >
          <q-tooltip>Open Timesheet List</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>

    <q-separator />

    <!-- Table -->
    <q-table
      flat
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      row-key="id"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      class="req-dashboard-table"
      separator="cell"
      no-data-label="No data available"
      @request="getAllTimesheetByRequirementId"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th
            v-for="col in props.cols"
            :key="col.name"
            :props="props"
          >
            {{ col.label }}
          </q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr
          :props="props"
          :set="(prevDate = null, prevEmployee = null)"
        >
          <!-- Date -->
          <q-td>
            <span
              v-if="prevDate !== props.row.timesheetDate"
              :set="(prevDate = props.row.timesheetDate, prevEmployee = null)"
            >
              {{ props.row.timesheet?.timesheetDate }}
            </span>
          </q-td>

          <!-- Employee -->
          <q-td>
            <span
              v-if="prevEmployee !== props.row.employeeName"
              :set="prevEmployee = props.row.employeeName"
            >
              {{ props.row.timesheet?.employee?.person?.fullName }}
            </span>
          </q-td>

          <!-- Task -->
          <q-td>
            {{ props.row.task?.name }}
          </q-td>

          <!-- Hours -->
          <q-td align="right">
            {{ props.row.hours }}
          </q-td>
        </q-tr>
      </template>
      <template #bottom-row>
        <q-tr class="bg-grey-2 text-weight-bold">
          <q-td colspan="3" class="text-right">Total Hours:</q-td>
          <q-td class="text-right">{{ totalHours }}</q-td>
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch, onMounted } from "vue";
import { useAuthStore } from "stores/auth";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

const emit = defineEmits(['summary']);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);
const searchLoader = ref(false);
const showFilter = ref(false);
const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);
const rows = ref([]);

const columns = [
  { name: 'timesheet.timesheetDate', label: 'DATE', field: 'timesheet.timesheetDate', align: 'left', sortable: true },
  { name: 'timesheet.employee.person.fullName', label: 'EMPLOYEE', field: 'timesheet.employee.person.fullName', align: 'left', sortable: true },
  { name: 'task.name', label: 'TASK', field: 'task.name', align: 'left', sortable: true },
  { name: 'hours', label: 'HOURS', field: 'hours', align: 'right', sortable: true }
]

const projectId = ref('');
const getAllTimesheetByRequirementId = async ({ pagination: p = pagination.value }) => {
  const { page, rowsPerPage, sortBy, descending } = p;

  try {
    loading.value = true;
    const payload = {
      requirementId: props.requirementId,
      searchText: search.value.searchText,
      createdBy: search.value.createdBy,
      employeeId: search.value.employeeId,
      projectTaskId: search.value.projectTaskId,
      activityDate: search.value.activityDate,
      fromDate: search.value.fromDate,
      toDate: search.value.toDate,
      weekFilter: search.value.weekFilter,
      page: page,
      pageSize: rowsPerPage,
      sortBy,
      descending,
      sorts: sorts.value
    };

    const resp = await requirementCenterService.getAllTimesheetByRequirementId(payload);
    rows.value = resp.data || [];

    // console.log("timesheet", rows.value);

    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total || 0
    });
    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: activeRowId.value,
      sorts
    });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

function calculateTotalHours(rows) {
  let totalMinutes = 0;

  rows.forEach(row => {
    if (row.hours == null) return;

    let hour = "0";
    let minute = "0";

    if (typeof row.hours === "string") {
      [hour, minute] = row.hours.split(":");
    } else {
      // Convert HH.MM number to HH:mm string
      const value = Number(row.hours);
      const h = Math.floor(value);
      const m = Math.round((value - h) * 100);

      hour = h.toString();
      minute = m.toString().padStart(2, "0");
    }

    totalMinutes += parseInt(hour, 10) * 60 + parseInt(minute, 10);
  });

  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  return `${hours.toString().padStart(2, "0")}:${minutes
    .toString()
    .padStart(2, "0")}`;
}

const totalHours = computed(() => calculateTotalHours(rows.value));

const filteredRows = computed(() => {
    if (!search.value.searchText)
        return rows.value;

    return rows.value.filter(x =>
        x.name.toLowerCase().includes(search.value.searchText.toLowerCase())
    );
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshTimesheetList = () => {
  getAllTimesheetByRequirementId({ pagination: pagination.value });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Timesheet-Tabular-List",
  siteId: siteId,

  defaultSearch: {
    searchText: "",
    createdBy: "",
    employeeId: "",
    projectId: "",
    projectModuleId: "",
    projectTaskId: "",
    activityDate: null,
    fromDate: null,
    toDate: null,
    weekFilter: ""
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshTimesheetList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.createdBy = "Created By Me";
  search.value.weekFilter = "";
  search.value.projectTaskId = null;
  search.value.employeeId = null;
  search.value.activityDate = null;
  search.value.fromDate = null;
  search.value.toDate = null;
  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { activeEmployeesDropdownSingleSelect } = employeeModule();
const { projectTasksByProjectIdAndModuleIdForDropdownSingleSelect } = projectTaskModule();

const createdByList = ref(["Created By Me", "View All"]);
const weekFilterList = ref(["Last Week", "This Week", "This Month"]);

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
  ...mapFilterToLabel(search.value.projectTaskId, projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.list, "Project Tasks"),
  ...mapFilterToLabel(search.value.weekFilter, weekFilterList, "Week Filter"),
  ...(search.value.activityDate ? { "Activity Date": search.value.activityDate } : {}),
  ...(search.value.fromDate ? { "From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "To Date": search.value.toDate } : {})
}));

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
    async () => {
      pagination.value.page = 1;
      await getAllTimesheetByRequirementId({pagination: pagination.value});
    },
  {
    immediate: true
  }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshTimesheetList();
  }
);

watch(
  () => search.value.createdBy,
  (value) => {
    if (value === "Created By Me" || value) {
      search.value.employeeId = null;
    }
  }
);

onMounted(async () => {
  activeEmployeesDropdownSingleSelect.load(siteId.value);
  projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.load(false, props.projectId);
});
</script>

