<template>
  <q-card flat bordered>
    <q-separator />
    <q-card-section class="row q-col-gutter-sm">
      <div class="col-auto" style="width: 42%;">
        <formSingleSelectDropdown
          v-model="groupBy"
          placeholder="Group By"
          :isClearable="false"
          :options="groupOptions"
        />
      </div>
        <div class="row justify-end q-ml-xs">
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
        </div>
    </q-card-section>

    <q-separator />

    <q-table
      v-model:pagination="pagination"
      flat
      :loading="loading"
      :rows="filteredRows"
      :columns="columns"
      row-key="timesheetDate"
      separator="cell"
      binary-state-sort
      no-data-label="No data available"
      :rows-per-page-options="[20, 50, 100, 200]"
      style="height: calc(100vh - 220px)"
    >
      <template #loading>
        <q-inner-loading showing color="primary">
          <q-spinner size="40px" />
        </q-inner-loading>
      </template>

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
          class="cursor-pointer"
          :class="{ 'bg-blue-1': selectedTimesheet === props.row.id }"
          @click="selectedTimesheet = props.row.id; selectGroup(props.row);"
        >
          <q-td class="q-pa-none">
            <div
              class="column q-gutter-xs"
              style="white-space: normal; word-break: break-word;"
            >
              <div class="row items-center justify-between">
                <div class="row items-center">
                  <q-icon
                    :name="groupIcon"
                    size="18px"
                    class="q-mr-sm"
                  />

                  <div class="text-caption text-weight-bold text-black">
                    {{ props.row.name }}
                  </div>
                </div>

                <q-badge
                  rounded
                  color="primary"
                >
                  {{ props.row.count }}
                  <q-tooltip>Entries</q-tooltip>
                </q-badge>
              </div>

              <div class="text-caption text-grey-7">
                <strong>Hours:</strong> {{ props.row.hours }}
              </div>
            </div>
          </q-td>
        </q-tr>

        <q-separator />
      </template>
      <template #bottom-row>
        <q-tr class="bg-grey-2 text-weight-bold">
          <q-td class="text-left">Total Hrs: {{ totalHours }}</q-td>
        </q-tr>
        <q-separator />
      </template>

    </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch, onMounted } from "vue";
import { useAuthStore } from "stores/auth";
import { format } from "date-fns"; // Standard TimeZone Conversion

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";


const emit = defineEmits([
  "select",
  "search-change"
]);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  },
  projectId: {
    type: String,
    required: true
  },
  activeTab: String
});

const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);
const loading = ref(false);
const searchLoader = ref(false);
const showFilter = ref(false);
const selectedTimesheet = ref(null);
const rows = ref([]);

const columns = [
  {
    name: "timesheet",
    label: "Timesheets",
    field: "name",
    align: "left",
    sortable: true
  }
];

const groupBy = ref("date");

const groupOptions = [
  { text: "Group by Date", value: "date" },
  { text: "Group by Employee", value: "employee" },
  { text: "Group by Task", value: "task" }
];

const createdByList = ref(["Created By Me", "View All"]);
const weekFilterList = ref(["Last Week", "This Week", "This Month"]);

const groupIcon = computed(() => {
  switch (groupBy.value) {
    case "employee":
      return "o_person";
    case "task":
      return "o_task";
    default:
      return "o_calendar_month";
  }
});

// Function to calculate dates
const calculateLastWeekDates = () => {
  const weekFromDate = new Date();
  const weekToDate = new Date();
  const currentDay = weekFromDate.getDay();
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

  if (dates.fromDate) {
    search.value.fromDate = format(new Date(dates.fromDate), "MM/dd/yyyy");
  } else {
    search.value.fromDate = null;
  }

  if (dates.toDate) {
    search.value.toDate = format(new Date(dates.toDate), "MM/dd/yyyy");
  } else {
    search.value.toDate = null;
  }
};

const selectGroup = row => {
  const selected = {
    ...row,
    groupBy: groupBy.value
  };

  emit("select", selected);
};

const getGroupedTimesheetsByRequirementId = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;

  try {
    loading.value = true;
    const payload = {
        requirementId: props.requirementId,
        groupBy: groupBy.value,
        searchText: search.value.searchText,
        createdBy: search.value.createdBy,
        employeeId: search.value.employeeId,
        projectTaskId: search.value.projectTaskId,
        activityDate: search.value.activityDate,
        fromDate: search.value.fromDate,
        toDate: search.value.toDate,
        weekFilter: search.value.weekFilter,
        page,
        pageSize: rowsPerPage,
        sortBy,
        descending,
        sorts: sorts.value
    };

    const resp = await requirementCenterService.getGroupedTimesheetsByRequirementId(payload);
    rows.value = resp;

    if (rows.value.length) {
      selectedTimesheet.value = rows.value[0].id;

      // automatically select first row
      selectGroup(rows.value[0]);
    }

    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: rows.value.total
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
  getGroupedTimesheetsByRequirementId({ pagination: pagination.value });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Timesheet-List",
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
  () => [props.requirementId, groupBy.value],
  async () => {
    await getGroupedTimesheetsByRequirementId({
      pagination: pagination.value
    });
  },
  { immediate: true }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshTimesheetList();
  }
);

watch(
  search,
  value => {
    emit("search-change", { ...value });
  },
  {
    deep: true,
    immediate: true
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

watch(
  () => props.activeTab,
  () => {
    showFilter.value = false;
  }
);

onMounted(async () => {
  activeEmployeesDropdownSingleSelect.load(siteId.value);
  projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.load(false, props.projectId);
});

</script>
