<template>
  <fieldset>
    <legend>Project Tasks</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input
        v-model="filterTask" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search"
        dense clearable
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      ref="tableRef1" v-model:pagination="paginationTask"
      :class="taskRows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" flat bordered
      :loading="loading" :rows="filteredTask" :columns="columnsTask" row-key="id" separator="cell" :filter="filterTask"
      binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getProjectTasks"
    >
      <template #header="propsTask">
        <q-tr :props="propsTask" class="bg-primary text-white">
          <!-- <q-th auto-width class="text-center"></q-th> -->
          <q-th v-for="col in propsTask.cols" :key="col.name" :props="propsTask">{{ col.label }}</q-th>
        </q-tr>
      </template>
      <template #body="propsTask">
        <q-tr
          :props="propsTask" :class="activeRowId == propsTask.row.id ? 'highlight' : ''"
          :set="(preProjectName = null, preProjectModuleName = null)"
        >
          <q-td style="width: 3%" class="hidden">{{ propsTask.row.projectTaskNumber }}</q-td>
          <!-- <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;"><span v-if="preProjectName !== propsTask.row.project.name" :set="preProjectName = propsTask.row.project.name">{{ propsTask.row.project.name }}</span></q-td> -->
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;"><span
            v-if="preProjectModuleName !== propsTask.row.projectModule.name"
            :set="preProjectModuleName = propsTask.row.projectModule.name"
          >{{ propsTask.row.projectModule.name
          }}</span></q-td>
          <!-- <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{
            propsTask.row.name }}</q-td> -->
          <q-td style="width: 20%; white-space: normal;" class="hoverable-cell">
            <div class="row no-wrap items-center justify-between">
              <span style="flex: 1; word-break: break-word; white-space: normal;">
                <span @click="onViewTask(propsTask.row.id)">{{ propsTask.row.name }}</span>
              </span>
            </div>
          </q-td>
          <q-td style="width: 5%;">{{ toDate(propsTask.row.startDate) }}</q-td>
          <q-td style="width: 5%;">{{ toDate(propsTask.row.endDate) }}</q-td>
          <q-td style="width: 5%;">{{ propsTask.row.priority.dropDownValue }}</q-td>
          <q-td class="text-right" style="width: 5%;">{{ propsTask.row.totalActivityHours }}</q-td>
          <q-td style="width: 5%;">
            <q-select
              v-model="propsTask.row.status.id" outlined stack-label hide-bottom-space :dense="true"
              :disable="projectTaskStatusList?.find(item => item.value === propsTask.row.status.id)?.text === 'Close'"
              :options="projectTaskStatusList" class="task-status-list" option-value="value" option-label="text"
              emit-value map-options :bg-color="getStatusColorTask(propsTask.row.status.dropDownValue)"
              @update:model-value="onSubmitTaskStatus(propsTask.row.id, propsTask.row.status.id)" @popup-show="() => handlePopupShow(propsTask.row.status.dropDownValue, propsTask.row.project.projectStatus.dropDownValue)"
            />
          </q-td>
        </q-tr>
        <q-tr v-if="propsTask.pageIndex === taskRows.length - 1">
          <q-td colspan="5" class="text-right font-bold"><b>Total Hours:</b></q-td>
          <q-td class="text-right"><b>{{ totalEstimateHours() }}</b></q-td>
          <q-td />
        </q-tr><q-separator />
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import { notifySuccess, zwConfirm } from "assets/utils";
import { ref, onMounted, computed } from "vue";
import { useQuasar } from "quasar";
import useFilters from "composables/useFilters";
import projectService from "modules/project/projects.service";
import projectTaskService from "modules/project-tasks/projectTasks.service";
import commonService from "services/common.service";
import viewProjectTask from "modules/project-tasks/components/view.vue";

const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;
const { toDate } = useFilters();
const $q = useQuasar();

const loading = ref(true);
const tableRef1 = ref();
const taskRows = ref([]);
const activeRowId = ref(null);
const filterTask = ref("");
const paginationTask = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columnsTask = ref([
  // { name: "projectTaskNumber", label: "Id", field: "projectTaskNumber", align: "left", sortable: true },
  { name: "projectModule.name", label: "Module", field: "projectModule.name", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "startDate", label: "Start Date", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "End Date", field: "endDate", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true },
  { name: "estimateTime", label: "Activity Hrs", field: "estimateTime", align: "left", sortable: true, headerStyle: "width: 90px" },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true }
]);

// On page rendering
onMounted(() => {
  getTaskStatuses("Task Status");
  const propsTask = { pagination: paginationTask.value };
  getProjectTasks(propsTask);
});

const getProjectTasks = (propsTask) => {
  const { page, rowsPerPage, sortBy, descending } = propsTask.pagination;
  loading.value = true;
  const payloadTask = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllProjectTasksForDashboard(payloadTask).then((resp) => {
    taskRows.value = resp.data.map(task => ({
      ...task,
      activity: task.projectActivities ? task.projectActivities.map(activity => ({
        ...activity
      })) : [],
      totalActivityHours: totalActivityHours(task.projectActivities)
    }));
    paginationTask.value.page = page;
    paginationTask.value.rowsPerPage = rowsPerPage;
    paginationTask.value.sortBy = sortBy;
    paginationTask.value.descending = descending;
    paginationTask.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// View popup
const onViewTask = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewProjectTask,
    componentProps: { id }
  }).onOk(() => {
    getProjectTasks();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

function totalEstimateHours () {
  // Iterate over all rows and sum up the `estimateHours` of their activities
  const total = taskRows.value.reduce((sum, row) => {
    if (row.activity && Array.isArray(row.activity)) {
      return sum + row.activity.reduce((activitySum, activity) => {
        return activitySum + (activity.estimateHours || 0);
      }, 0); // Sum activity hours
    }
    return sum;
  }, 0);

  return parseFloat(total.toFixed(2)); // Round to 2 decimal places
}

function totalActivityHours (activities) {
  if (!activities || activities.length === 0) {
    return 0;
  }
  const total = activities.reduce((total, activity) => {
    return total + (activity.estimateHours || 0);
  }, 0);

  const roundedTotal = parseFloat(total.toFixed(2));
  return roundedTotal;
}

const onSubmitTaskStatus = async (id, statusId) => {
  const selected = projectTaskStatusList.value.find(item => item.value === statusId);
  try {
    if (selected?.text.toLowerCase() === "close") {
      const resp = await projectTaskService.checkTaskCanBeDeleted(id);
      const canDelete = resp?.canDelete;
      if (canDelete) {
        const payload = {
          taskIds: [id],
          statusId
        };
        setTimeout(function () {
          projectTaskService.updateProjectTaskStatus(payload).then(resp => {
            notifySuccess({ message: "Task status is saved successfully." });
            getProjectTasks({ pagination: paginationTask.value });
          });
        });
      } else {
      // Warning confirmation
        zwConfirm({
          title: "Active Activities Found",
          message: "This task has active activities. You cannot close it.",
          okLabel: "OK",
          cancel: false
        }, () => {
          getProjectTasks({ pagination: paginationTask.value });
        });
      }
    } else {
      const payload = {
        taskIds: [id],
        statusId
      };
      setTimeout(function () {
        projectTaskService.updateProjectTaskStatus(payload).then(resp => {
          notifySuccess({ message: "Task status is saved successfully." });
          getProjectTasks({ pagination: paginationTask.value });
        });
      });
    }
  } catch (error) {
    console.error("Error checking module:", error);
  }
};
// Get all project task status List
// const projectTaskStatusList = ref([]);
// function getDropDownTaskStatus (typeName) {
//   commonService.getDropDown(typeName).then((resp) => {
//     const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
//     projectTaskStatusList.value = responseData;
//   });
// }
function handlePopupShow (taskStatus, projectStatusLabel) {
  getTaskStatuses("Task Status", taskStatus, projectStatusLabel);
}
// Get all project task status List
const projectTaskStatusList = ref([]);
function getTaskStatuses (typeName, taskStatusLabel = null, projectStatusLabel = null) {
  commonService.getDropDown(typeName).then((resp) => {
    const lockedStatuses = ["Cancelled", "Completed", "On Hold"];
    // const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    const responseData = resp.map((item) => {
      const label = item.dropdownValue;
      let shouldDisable = false;

      if (lockedStatuses.includes(projectStatusLabel) && taskStatusLabel === "New") {
        shouldDisable = label === "Open";
      }
      if (projectStatusLabel === "New") { shouldDisable = label === "Open"; }

      return {
        text: item.dropdownValue,
        value: item.id,
        disable: shouldDisable
      };
    }).sort((a, b) => a.text.localeCompare(b.text));

    projectTaskStatusList.value = responseData;
  });
}
// Added colors for task status dropdown list
function getStatusColorTask (statusText) {
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

const taskColumns = columnsTask.value;
const filteredTask = computed(() => filterRows(taskRows.value, filterTask.value, taskColumns));
</script>
