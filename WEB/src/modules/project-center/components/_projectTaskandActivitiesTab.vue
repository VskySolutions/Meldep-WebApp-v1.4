<template>
  <fieldset>
    <legend>Project Activities</legend>
    <div class="q-gutter-sm flex justify-end q-mb-md">
      <div class="row items-center">
        <div class="col-auto">
          <div class="input-group">
            <q-input outlined label="Target Month" stack-label v-model="search.targetMonthStr" fill-input dense class="q-mx-sm q-mt-xs">
              <template #append>
                <q-icon name="o_calendar_month" class="cursor-pointer">
                  <q-popup-proxy ref="qDateProxy" v-model="isPopupVisible" transition-show="scale" transition-hide="scale">
                    <q-date ref="date3ref" v-model="search.targetMonthStr" default-view="Years" emit-immediately minimal mask="MMMM-YYYY" class="myDate" @update:model-value="onUpdateMv2" />
                  </q-popup-proxy>
                </q-icon>
              </template>
            </q-input>
          </div>
        </div>
        <!-- Search Filter -->
        <div class="col-auto">
          <q-input v-model="filterActivity" outlined class="bg-white search-box q-mr-sm" debounce="300" placeholder="Search" dense clearable>
            <template #prepend>
              <q-icon name="o_search" />
            </template>
          </q-input>
        </div>
        <!-- Action Buttons -->
        <div class="col-auto AdvanceBTN">
          <q-btn
            color="primary" outline label="Search" type="button" no-caps class="q-mx-md btnRounded q-px-lg q-pa-sm"
            @click="onSearch"
          />
          <q-btn
            color="grey-4" outline label="Clear" type="button" class="text-grey-9 btnRounded q-px-lg q-pa-sm" no-caps
            @click="onClear"
          />
        </div>
      </div>
    </div>
    <q-table
      ref="tableRef3" v-model:pagination="paginationActivity" :class="rowsActivity.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :rows-per-page-options="[20, 50, 100, 200, 500]"
      :filter="filterActivity" :loading="loadingActivity" :rows="filteredActivity" :columns="columnsActivity" row-key="id" separator="cell" no-data-label="No data available" binary-state-sort @request="getProjectActivities"
    >
      <template #header="propsActivity">
        <q-tr :props="propsActivity" class="bg-primary text-white">
          <q-th v-for="col in propsActivity.cols" :key="col.name" :props="propsActivity">{{ col.label }}</q-th>
        </q-tr>
      </template>
      <template #body="propsActivity">
        <q-tr :props="propsActivity" :class="activeRowIdActivity == propsActivity.row.id ? 'highlight' : ''" :set="(preProjectName = null, preProjectModuleName = null)">
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;"><span v-if="preProjectModuleName !== propsActivity.row.projectModule.name" :set="preProjectModuleName = propsActivity.row.projectModule.name">{{ propsActivity.row.projectModule.name }}</span></q-td>
          <q-td style="width: 20%; white-space: normal;" class="hoverable-cell">
            <div class="row no-wrap items-center justify-between">
              <span style="flex: 1; word-break: break-word; white-space: normal;">
                <span @click="onViewTask(propsActivity.row.task.id)">{{ propsActivity.row.task.name }}</span>
              </span>
            </div>
          </q-td>
          <q-td style="width: 5%;">
            <q-select
              v-model="propsActivity.row.task.status.id" outlined stack-label hide-bottom-space :dense="true"
              :options="projectTaskStatusList" class="task-status-list" option-value="value" option-label="text" emit-value map-options :bg-color="getActivityStatusColor(propsActivity.row.task.status.dropDownValue)" :disable="isClose" @update:model-value="onSubmitActivityTaskStatus(propsActivity.row.id, propsActivity.row.task.id, propsActivity.row.task.status.id)"
            />
          </q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 5%;">{{ propsActivity.row.name }}</q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">{{ propsActivity.row.assignedTo.person.fullName ? propsActivity.row.assignedTo.person.fullName : " " }}</q-td>
          <q-td style="width: 5%;">{{ toDate(propsActivity.row.startDate) }}</q-td>
          <q-td style="width: 5%;">{{ toDate(propsActivity.row.endDate) }}</q-td>
          <q-td style="width: 5%;">
            <q-select
              v-model="propsActivity.row.activityStatus.id" outlined stack-label hide-bottom-space :dense="true"
              :options="activityStatusList" class="task-activity-status-list" option-value="value" option-label="text" emit-value map-options :bg-color="getActivityStatusColor(propsActivity.row.activityStatus.dropDownValue)" :disable="isClose" @update:model-value="onChangeActivityStatus(propsActivity.row.id, propsActivity.row.activityStatus.id)"
            />
          </q-td>
          <q-td class="text-right" style="width: 5%;">{{ propsActivity.row.estimateHours }}</q-td>
          <q-td style="width: 5%;">{{ toMonthYear(propsActivity.row.targetMonth) }}</q-td>
        </q-tr>
        <q-tr v-if="propsActivity.pageIndex === rowsActivity.length - 1">
          <q-td colspan="8" class="text-right font-bold"><b>Total Hours:</b></q-td>
          <q-td class="text-right"><b>{{ totalEstimateActivityHours() }}</b></q-td>
          <q-td />
        </q-tr><q-separator />
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import { clearLocalStorage, getLocalStorage, notifySuccess } from "assets/utils";
import { ref, onMounted, computed } from "vue";
import { useQuasar } from "quasar";
import useFilters from "composables/useFilters";
import projectTaskService from "modules/project-tasks/projectTasks.service";
import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";
import projectService from "modules/project/projects.service";
import commonService from "services/common.service";
import viewProjectTask from "modules/project-tasks/components/view.vue";

const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;

const $q = useQuasar();
const isPopupVisible = ref(false);
const currentView = ref("Years");
const date3ref = ref(null);
const tableRef3 = ref();
const loading = ref(true);
const { toDate, toMonthYear } = useFilters();

const loadingActivity = ref(false);
const rowsActivity = ref([]);
const activeRowIdActivity = ref(null);
const filterActivity = ref("");
const paginationActivity = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columnsActivity = ref([
  { name: "projectModule.name", label: "Module", field: "projectModule.name", align: "left", sortable: false },
  { name: "task.name", label: "Task", field: "task.name", align: "left", sortable: true },
  { name: "task.status.dropDownValue", label: "Task Status", field: "task.status.dropDownValue", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "assignedTo.person.fullName", label: "Activity Owner", field: "assignedTo.person.fullName", align: "left", sortable: false },
  { name: "startDate", label: "Start Date", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "End Date", field: "endDate", align: "left", sortable: true },
  { name: "activityStatus.dropDownValue", label: "Status", field: "activityStatus.dropDownValue", align: "left", sortable: true },
  { name: "estimateHours", label: "Est. Hrs", field: "estimateHours", align: "left", sortable: true },
  { name: "targetMonth", label: "Target Month", field: "targetMonth", align: "left", sortable: false }
]);

const localStorageKey = "DashboardProjectTasks";
const filterLocalStorage = getLocalStorage(localStorageKey);
const targetMonthStr = ref(filterLocalStorage ? filterLocalStorage.targetMonthStr : getCurrentMonthYear());

// Search variables
const search = ref({
  targetMonthStr
});

// Clear search
const onClear = () => {
  search.value.targetMonthStr = getCurrentMonthYear();
  clearLocalStorage(localStorageKey);
  onSearch();
};

const onSearch = () => {
  const proppsTaskActivity = { pagination: paginationActivity.value };
  getProjectActivities(proppsTaskActivity);
};

// Function to get the current month and year in 'YYYY-MM' format
function getCurrentMonthYear () {
  const today = new Date();
  const year = today.getFullYear();
  const monthNames = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
  ];
  const month = monthNames[today.getMonth()]; // Get the month name
  return `${month}-${year}`; // Format as 'Month-YYYY'
}

const onUpdateMv2 = (val) => {
  if (currentView.value === "Years") {
    // Switch to Months view after selecting a year
    currentView.value = "Months";
    if (date3ref.value) {
      date3ref.value.setView("Months");
    }
  } else {
    // Update the target month string and close the popup
    search.value.targetMonthStr = val; // Update the reactive property
    currentView.value = "Years"; // Reset for the next use
    isPopupVisible.value = false; // Close the popup
  }
};

// Get all activity status list
const activityStatusList = ref([]);
function getDropDownActivityStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    activityStatusList.value = responseData;
  });
}
// Get all project task status List
const projectTaskStatusList = ref([]);
function getDropDownTaskStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    projectTaskStatusList.value = responseData;
  });
}
const getProjectActivities = (propsActivity) => {
  const { page, rowsPerPage, sortBy, descending } = propsActivity.pagination;
  loading.value = true;
  const payloadActivity = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId,
    // targetMonthStr: search.value.targetMonthStr
    ...search.value
  };
  projectService.getAllProjectActivitiesForDashboard(payloadActivity).then((resp) => {
    rowsActivity.value = resp.data;
    rowsActivity.value = resp.data.map(project => ({
      ...project
    }));
    paginationActivity.value.page = page;
    paginationActivity.value.rowsPerPage = rowsPerPage;
    paginationActivity.value.sortBy = sortBy;
    paginationActivity.value.descending = descending;
    paginationActivity.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// View popup
const onViewTask = (id) => {
  activeRowIdActivity.value = id;
  $q.dialog({
    component: viewProjectTask,
    componentProps: { id }
  }).onOk(() => {
    getProjectActivities();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowIdActivity.value = null;
  });
};

function onSubmitActivityTaskStatus (id, taskId, statusId) {
  setTimeout(function () {
    projectTaskService.updateTaskStatus(id, taskId, statusId).then(resp => {
      notifySuccess({ message: "Status is saved successfully." });
      getProjectActivities({ pagination: paginationActivity.value });
    });
  });
}

function onChangeActivityStatus (id, activityStatusId) {
  const payload = {
    activityIds: [id],
    activityStatusId
  };
  setTimeout(function () {
    projectActivitiesService.updateTaskActivityStatus(payload).then(resp => {
      notifySuccess({ message: "Activity status is saved successfully." });
      getProjectActivities({ pagination: paginationActivity.value });
    });
  });
}

// Added colors for task status dropdown list
function getActivityStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Open":
      return "purple-4";
    case "Close":
      return "grey-4";
    case "Completed":
      return "green-4";
    case "In Development":
      return "yellow-4";
    case "In QA":
      return "cyan-4";
    case "In UAT":
      return "deep-orange-4";
    case "New":
      return "blue-4";
    case "On Hold":
      return "brown-4";
    case "Test Site":
      return "blue-grey-4";
    case "UAT passed":
      return "green-9";
    default:
      return "#ffffff";
    }
  }
}

function totalEstimateActivityHours () {
  const total = rowsActivity.value.reduce((total, row) => total + (row.estimateHours || 0), 0);
  return total.toFixed(2);
}

// for static filter
const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data; // If no filter, return all data
  const lowerCaseTerm = searchTerm.toLowerCase();
  return data.filter(row =>
    columns.some(column => {
      const value = column.field.split(".").reduce((obj, key) => obj?.[key], row); // Handle nested fields
      return String(value || "").toLowerCase().includes(lowerCaseTerm);
    })
  );
};

const activityColumns = columnsActivity.value;
const filteredActivity = computed(() => filterRows(rowsActivity.value, filterActivity.value, activityColumns));

onMounted(() => {
  getDropDownActivityStatus("Activity Status");
  getDropDownTaskStatus("Task Status");
  const propsActivity = { pagination: paginationActivity.value };
  getProjectActivities(propsActivity);
});
</script>
