<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <!-- Header -->
    <q-card-section class="row items-center justify-end q-pb-sm">
      <div class="row items-center">
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
                  <label class="Cutomlabel q-mt-sm fs-13">Task Number</label>
                </div>
                <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                  <q-input v-model="search.projectTaskNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                </div>
              </div>
              <multiSelectDropdown
                v-model="search.projectTaskIds"
                label="Task Names"
                :options="projectTasksByProjectIdAndModuleIdForDropdown.list.value"
                :filter="projectTasksByProjectIdAndModuleIdForDropdown.filter"
              />
              <multiSelectDropdown
                v-model="search.activityOwners"
                label="Activity Owners"
                :options="activeEmployeesDropdown.list.value"
                :filter="activeEmployeesDropdown.filter"
              />
              <multiSelectDropdown
                v-model="search.statusIds"
                label="Task Status"
                :options="projectTaskStatusListWithDisables"
                :filter="getProjectTaskStatusFilter"
                :isShowAll="true"
              />
              <multiSelectDropdown
                v-model="search.priorityIds"
                label="Task Priority"
                :options="projectTaskPrioritiesForDropdown.list.value"
                :filter="projectTaskPrioritiesForDropdown.filter"
                :isShowAll="true"
              />
              <multiSelectDropdown
                v-model="search.taskTagsIds"
                label="Task Tags"
                :options="projectTaskTagsDropdown.list.value"
                :filter="projectTaskTagsDropdown.filter"
                :show-bg-color="true"
              />
              <!-- Search and Clear Buttons -->
              <div class="row justify-end q-gutter-sm q-mb-sm">
                <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
                <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
              </div>
            </q-card>
          </q-menu>
        </div>
      </div>

      <div class="row items-center q-gutter-sm">
        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary q-ml-md"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/project-tasks',
          state: {
            projectId: projectId,
            projectModuleId: projectModuleId,
            requirementId: props.requirementId
          }})"
        >
          <q-tooltip>Open Task List</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>
    <q-separator />

    <!-- Table -->
    <q-table
      flat
      :rows="filteredTasks"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      row-key="id"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      class="req-dashboard-table"
      separator="cell"
      no-data-label="No data available"
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
        <q-tr :props="props">
          <q-td style="width:15%;">
            #{{ props.row.projectTaskNumber }}
          </q-td>

          <q-td style="width:45%;">
            <span
              class="hoverable-cell"
              @click="onProjectTaskView(props.row.id)"
            >
              {{ props.row.name }}
            </span>
          </q-td>

          <q-td style="width:20%;">
            {{ props.row.owner }}
          </q-td>

          <q-td style="width:20%;">
            <q-chip
              dense
              class="fs-13"
              :style="{
                backgroundColor: props.row.statusBgColor,
                color: props.row.statusTextColor
              }"
            >
              {{ props.row.statusName }}
            </q-chip>
          </q-td>

          <q-td style="width:20%;">
            {{ props.row.dueDate }}
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>
<script setup>
import { computed, ref, watch, onMounted } from "vue";
import { notifyError } from "assets/utils";
import { useAuthStore } from "stores/auth";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";
import commonService from "services/common.service";

// Shared Dropdowns
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

const emit = defineEmits(['summary']);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  },
  projectId: {
    type: String,
    required: true
  }
});

// common variables
const loading = ref(false);
const rows = ref([]);
const authStore = useAuthStore();
const user = authStore.user;
const searchLoader = ref(false);
const showFilter = ref(false);
const siteId = computed(() => authStore.user?.siteId);

const columns = [
  { name: 'projectTaskNumber', label: 'TASK NO.', field: 'projectTaskNumber', align: 'left', sortable: true },
  { name: 'name', label: 'NAME', field: 'name', align: 'left', sortable: true },
  { name: 'owner', label: 'OWNER', field: 'owner', align: 'left', sortable: true },
  { name: 'statusName', label: 'STATUS', field: 'statusName', align: 'left', sortable: true },
  { name: 'dueDate', label: 'DUE DATE', field: 'dueDate', align: 'left', sortable: true }
]

const projectId = ref('');
const projectModuleId = ref('');
const getTasksByRequirementId = async ({ pagination: p }) => {
  loading.value = true;
  const { page, rowsPerPage, sortBy, descending } = p;
  
  // sanitize task number
  const taskNumber = (search.value.projectTaskNumber || "").replace(/[^0-9]/g, "").replace(/^0+(?!$)/, "");
  search.value.projectTaskNumber = taskNumber || "0";

  const payload = {
    requirementId: props.requirementId,
    searchText: search.value.searchText,
    projectTaskNumber: search.value.projectTaskNumber,
    projectTaskIds: search.value.projectTaskIds,
    activityOwners: search.value.activityOwners,
    statusIds: search.value.statusIds,
    priorityIds: search.value.priorityIds,
    tagIds: search.value.taskTagsIds,
    sortBy: sortBy,
    // sorts: sorts.value,
    descending: descending,
    page: page,
    pageSize: rowsPerPage
  };

  try {
    loading.value = true;
    const resp = await requirementCenterService.getTasksByRequirementId(payload);

    rows.value = resp.map(item => ({
      ...item,
      owner: item.assignedTo?.person?.fullName ?? '-',
      statusName: item.status?.dropDownValue ?? '-',
      dueDate: item.endDate ?? '-',
      statusTextColor: item.status?.color ?? '-',
      statusBgColor: item.status?.bgColor ?? '-'
    }));

    if (resp.length) {
      projectId.value = resp[0].project.id;
      projectModuleId.value = resp[0].projectModuleId;
    }
    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });
    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: activeRowId.value,
      sorts
    });

    emit('summary', {
      total: rows.value.length,
      open: rows.value.filter(r => r.statusName === 'Open').length
    });
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Tasks" });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshProjectTaskList = () => {
  getTasksByRequirementId({ pagination: pagination.value });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Tasks-List",
  siteId,
  defaultSearch: {
    searchText: "",
    projectTaskNumber: 0,
    projectTaskIds: [],
    activityOwners: user?.employeeId
      ? [user.employeeId]
      : [],
    statusIds: [],
    priorityIds: [],
    taskTagsIds: []
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const filteredTasks = computed(() => {
  if (!search.value) return rows.value;

  return rows.value.filter(task =>
    JSON.stringify(task)
      .toLowerCase()
      .includes((search.value.quickSearch ?? '').toLowerCase())
  );
});

// Get all project task status List
const projectTaskStatusFilters = ref([]);
const projectTaskStatusListWithDisables = ref([]);

const getProjectTaskStatusForDropdown = (typeName, taskStatusLabel = null, projectStatusLabel = null) => {
  commonService.getDropDown(typeName).then((resp) => {
    const lockedStatuses = ["Cancelled", "Completed", "On Hold"];
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
    });

    projectTaskStatusListWithDisables.value = responseData.map(item => ({ ...item, disable: false }));
    projectTaskStatusFilters.value = projectTaskStatusListWithDisables.value;
  });
};

const getProjectTaskStatusFilter = (val, update, abort) => {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectTaskStatusListWithDisables.value = projectTaskStatusFilters.value;
    } else {
      projectTaskStatusListWithDisables.value = projectTaskStatusFilters.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
};

// Clear search
const onClear = () => {
  search.value.projectTaskNumber = "";
  search.value.projectTaskIds = [];
  search.value.statusIds = [];
  search.value.priorityIds = [];
  search.value.taskTagsIds = [];

  saveDataTableState({
    search: search.value
  });
  onSearch();
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

const appliedFilters = computed(() => ({
  ...(search.value.projectTaskNumber > 0 ? { "Task Number": search.value.projectTaskNumber } : {}),
  ...mapFilterToLabel(search.value.projectTaskIds, projectTasksByProjectIdAndModuleIdForDropdown.list, "Project Task"),
  ...mapFilterToLabel(search.value.activityOwners, activeEmployeesDropdown.list, "Activity Owner"),
  ...mapFilterToLabel(search.value.statusIds, projectTaskStatusListWithDisables, "Task Status"),
  ...mapFilterToLabel(search.value.priorityIds, projectTaskPrioritiesForDropdown.list, "Task Priority"),
  ...mapFilterToLabel(search.value.taskTagsIds, projectTaskTagsDropdown.list, "Task Tags")
}));

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onSearch = () => { refreshProjectTaskList(); };

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectTasksByProjectIdAndModuleIdForDropdown, projectTaskPrioritiesForDropdown, projectTaskTagsDropdown } = projectTaskModule();
const { activeEmployeesDropdown } = employeeModule();

// ----------------------------------------------------------------------------------------------------------------
// DataTable :- Initialization Of Dialogs, Actions
// ----------------------------------------------------------------------------------------------------------------

initProjectTaskDialogs(activeRowId);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
  async () => {
    await getTasksByRequirementId({
      pagination: pagination.value
    });
  },
  { immediate: true }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshProjectTaskList();
  }
);

onMounted(async () => {
  activeEmployeesDropdown.load(user.siteId);
  projectTasksByProjectIdAndModuleIdForDropdown.load(false, props.projectId);
  getProjectTaskStatusForDropdown("Task Status");
  projectTaskPrioritiesForDropdown.load("Task Priorities");
  projectTaskTagsDropdown.load();
});
</script>
