<template>
  <q-page padding>
    <q-card class="breadcrumSection project6 flex justify-between items-center">
      <!-- Breadcrumb Section -->
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template v-slot:separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Project Management" />
          <q-breadcrumbs-el label="Project Tasks" clickable to="/project-tasks" />
          <q-breadcrumbs-el label="Tasks Grid" />
        </q-breadcrumbs>
      </q-card-section>
      <q-card-section class="flex items-center no-padding">
        <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-mr-md no-space-between" @click="$router.back()" />
      </q-card-section>
    </q-card>
    <q-card class="project6">
      <div class="">
        <q-card-section>
          <div class="row q-col-gutter-sm q-mb-sm">
            <div class="row items-center q-gutter-md no-wrap">
              <div class="col-auto">
                <q-btn class="bg-grey-3 text-grey-10 btnRounded" label="Search" flat type="button" no-caps @click="advanceSearch">
                  <q-icon :name="advanceSearchEnable ? 'o_filter_alt_off' : 'o_filter_alt'" />
                </q-btn>
              </div>
              <div class="col row items-center wrap">
                <span class="q-mr-xs text-grey-10" style="font-weight: 600;">Applied Filters:-</span>
                <q-chip v-for="(value, key) in appliedFilters" :key="key" class="q-mb-xs bg-grey-3 text-grey-10">
                  {{ key }} -> {{ value }}
                </q-chip>
              </div>
            </div>
          </div>
          <div class="row">
            <div :class="advanceSearchEnable ? 'col-lg-2' : 'col-12'">
              <!-- <q-card class="q-pa-sm"> -->
              <div class="">
                <div v-if="advanceSearchEnable">
                  <div class="row q-col-gutter-sm items-center">
                  <div class="col-12 col-sm-6 col-md-4 col-lg-12">
                    <label class="Cutomlabel">Project Name</label>
                    <multiSelectDropdown
                      v-model="search.projectIds"
                      :options="projectNameDropdown.list.value"
                      :filter="projectNameDropdown.filter"
                    />
                  </div>
                  <div class="col-12 col-sm-6 col-md-4 col-lg-12">
                    <label class="Cutomlabel">Sort By Filter</label>
                    <singleSelectDropdown
                      v-model="search.sortByFilterId"
                      :options="sortByFilterDropdownSingleSelect.list.value"
                      :filter="sortByFilterDropdownSingleSelect.filter"
                    />
                  </div>
                  <div class="col-12 col-sm-6 col-md-4 col-lg-12 q-mt-sm">
                    <label class="Cutomlabel">Task Name</label>
                    <div>
                      <q-input v-model="search.name" fill-input :dense="true" class="q-mx-sm"/>
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-4 col-lg-12 q-mt-sm">
                    <label class="Cutomlabel">Activity Owner</label>
                    <multiSelectDropdown
                      v-model="search.activityOwners"
                      :options="activeEmployeesDropdown.list.value"
                      :filter="activeEmployeesDropdown.filter"
                    />
                  </div>
                  <div class="col-12 col-sm-6 col-md-4 col-lg-12">
                    <label class="Cutomlabel">Customer</label>
                    <multiSelectDropdown
                      v-model="search.customerIds"
                      :options="customerNameDropdown.list.value"
                      :filter="customerNameDropdown.filter"
                    />
                  </div>
                  <div class="col-12 col-sm-6 col-md-4 col-lg-12">
                    <label class="Cutomlabel">Company Contact</label>
                    <multiSelectDropdown
                      v-model="search.companyContactIds"
                      :options="companyContactNameDropdown.list.value"
                      :filter="companyContactNameDropdown.filter"
                    />
                  </div>
                  <div class="col-12 col-sm-6 col-md-4 col-lg-12 q-mt-sm flex justify-center justify-lg-center">
                    <q-btn color="primary" outline label="Search" type="button" no-caps class="btnRounded q-mb-sm" @click="onSearch" />
                    <q-btn color="grey-4" outline label="Clear" type="button" class="text-grey-9 text-h4 btnRounded clear q-mx-sm q-px-md q-mb-sm" no-caps @click="onClear" />
                  </div>
                </div>
                </div>
              </div>
            </div>
            <div :class="advanceSearchEnable ? 'col-lg-10 col-xl-10 col-md-12 col-sm-12 col-12' : 'col-12'">
              <q-card class="q-pa-md">
                <q-table ref="tableRef" flat bordered grid :rows="rows" :columns="columns" row-key="id" separator="cell" :loading="loading" v-model:pagination="pagination" @request="getProjectTasks" hide-header :rows-per-page-options="[20, 50, 100, 200, 500]">
                  <template v-slot:item="props">
                    <div class="q-mb-md full-width">
                      <q-badge class="q-mb-md q-pa-sm text-weight-bold" color="primary" text-color="white">
                        {{ props.row.groupValue }}
                      </q-badge>
                      <div class="row q-col-gutter-md">
                        <div v-for="task in props.row.tasks" :key="task.id" class="col-12 col-sm-6 col-md-4 col-lg-3 col-xl-3">
                          <q-card class="my-card q-pa-xs full-height" style="box-shadow: 5px 4px 8px rgba(0, 0, 0, 0.2); background-color: #e4f6ff75;">
                            <q-card-section class="q-pt-xs q-pb-xs">
                              <!-- <div class="row">
                                <div class="col-12 text-h5">
                                  <span class="col-12 text-h5 text-black">{{ task.name }}</span>
                                </div>
                              </div> -->
                              <div class="row items-center justify-between">
                                <div class="text-black fs-16" style="font-weight: 600;">{{ task.name }}</div>
                                <q-icon name="o_visibility" class="cursor-pointer q-mr-sm bordered-icon" @click="onProjectTaskView(task.id)" size="sm" style="border: 1px solid #938e8e; border-radius: 5px; padding: 2px;">
                                  <q-tooltip>View</q-tooltip>
                                </q-icon>
                              </div>
                              <div class="row q-mt-md">
                                <span class="text-bold text-primary">Project:</span>
                                <span class="text-black q-ml-sm">{{ task.project.name }}</span>
                              </div>
                              <div class="row q-mt-md">
                                <span class="text-bold text-primary">Module:</span>
                                <span class="text-black q-ml-sm">{{ task.projectModule.name }}</span>
                              </div>
                              <div class="row q-mt-md">
                                <div class="col-12 col-sm-6 col-md-6 q-mb-xs">
                                  <span class="text-bold text-primary">Start Date:</span>
                                  <q-badge v-if="!task.startDate" color="red" square>No Data</q-badge>
                                  <span class="text-black q-ml-xs">{{ task.startDate }}</span>
                                </div>
                                <div class="col-12 col-sm-6 col-md-6 q-mb-xs">
                                  <span class="text-bold text-primary">End Date:</span>
                                  <q-badge v-if="!task.endDate" color="red" square>No Data</q-badge>
                                  <span class="text-black q-ml-xs">{{ task.endDate }}</span>
                                </div>
                              </div>
                              <div class="row q-mt-md">
                                <div class="col-12 col-sm-6 col-md-6 q-mb-xs">
                                  <span class="text-bold text-primary">Priority:</span>
                                  <q-badge color="green" square class="q-ml-xs fs-14" text-color="black"><span class="text-black">{{ task.priority.dropDownValue }}</span></q-badge>
                                  <!-- <span class="text-black q-ml-xs">{{ task.priority.dropDownValue }}</span> -->
                                </div>
                                <div class="col-12 col-sm-6 col-md-6 q-mb-xs">
                                  <span class="text-bold text-primary">Status:</span>
                                  <q-badge color="amber-4" square class="q-ml-xs fs-14" text-color="black"><span class="text-black">{{ task.status.dropDownValue }}</span></q-badge>
                                  <!-- <span class="text-black q-ml-xs">{{ task.status.dropDownValue }}</span> -->
                                </div>
                              </div>
                            </q-card-section>
                            <!-- <q-separator /> -->
                            <!-- <q-card-section class="q-pt-xs q-pb-xs">
                              <div class="text-right actions">
                              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm"  flat dense rounded outline @click="onView(task.id)" size="sm">
                                <q-tooltip>View</q-tooltip>
                              </q-icon>
                              </div>
                            </q-card-section> -->
                          </q-card>
                        </div>
                      </div>
                    </div>
                  </template>
                </q-table>
              </q-card>
            </div>
          </div>
        </q-card-section>
      </div>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import projectTaskService from "modules/project-tasks/projectTasks.service";
import { useAuthStore } from "stores/auth";

// Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

const rows = ref([]);
const activeRowId = ref(null);
const loading = ref(true);
const advanceSearchEnable = ref(false);
const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };
const authStore = useAuthStore();
const user = authStore.user;
const siteId = computed(() => authStore.user?.siteId);

const {
  search,
  pagination,
  saveDataTableState
} = useSiteTableState({
  storageKey: "project-Tasks-GridView",
  siteId,

  defaultSearch: {
    statusIds: [],
    projectIds: [],
    sortByFilterId: "Filter By Status",
    activityOwners: user?.employeeId ? [user.employeeId] : [],
    customerIds: [],
    companyContactIds: [],
    name: ""
  },

  defaultPagination: {
    sortBy: "updatedOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// Get/Map project task list
const getProjectTasks = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.sortByFilterId = sortByFilterDropdownSingleSelect.list.value.find(
            a => a.value === search.value.sortByFilterId
          )?.text || "Filter By Status";
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  saveDataTableState({
    search: search.value,
    pagination: pagination.value
  });
  projectTaskService.getAllProjectTaskDetailsList(payload).then((resp) => {
    rows.value = resp.data;
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
      pagination: pagination.value
    });
  }).finally(() => {
    loading.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectNameDropdown } = projectModule();
const { sortByFilterDropdownSingleSelect } = projectTaskModule();
const { activeEmployeesDropdown } = employeeModule();
const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();

// ----------------------------------------------------------------------------------------------------------------
// DataTable :- Initialization Of Dialogs, Actions
// ----------------------------------------------------------------------------------------------------------------
initProjectTaskDialogs(activeRowId);

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getProjectTasks(propps);
};

// Clear search
const onClear = () => {
  search.value.statusIds = [];
  search.value.projectIds = [];
  search.value.sortByFilterId = "Filter By Status";
  search.value.customerIds = [];
  search.value.companyContactIds = [];
  search.value.name = "";
  saveDataTableState({
    search: search.value
  });
  onSearch();
};

// appled filters
const appliedFilters = computed(() => ({
  ...(search.value.projectIds?.length
    ? {
      "Project Name": search.value.projectIds
        .map(projectId => {
          const project = projectNameDropdown.list.value.find(p => p.value === projectId);
          return project ? project.text : projectId;
        })
        .join(", ")
    }
    : {}),
   ...(search.value.sortByFilterId
    ? {
        "Sort By":
          sortByFilterDropdownSingleSelect.list.value.find(
            a => a.value === search.value.sortByFilterId
          )?.text || "Filter By Status"
      }
    : {}),
  ...(search.value.name ? { "Task Name": search.value.name } : {}),
  ...(search.value.activityOwners?.length
    ? {
      "Activity Owner": search.value.activityOwners
        .map(ownerId => {
          const owner = activeEmployeesDropdown.list.value.find(a => a.value === ownerId);
          return owner ? owner.text : ownerId; // Use name if found, else fallback to ID
        })
        .join(", ")
    }
    : {}),
  // ...(search.value.targetMonthStr ? { "Target Month": search.value.targetMonthStr } : {}),
  ...(search.value.customerIds?.length
    ? {
      Customer: search.value.customerIds
        .map(customerId => {
          const customer = customerNameDropdown.list.value.find(p => p.value === customerId);
          return customer ? customer.text : customerId;
        })
        .join(", ")
    }
    : {}),
  ...(search.value.companyContactIds?.length
    ? {
      "Company Contact": search.value.companyContactIds
        .map(contactId => {
          const contact = companyContactNameDropdown.list.value.find(c => c.value === contactId);
          return contact ? contact.text : contactId;
        })
        .join(", ")
    }
    : {})
}));

watch(() => search.value.customerIds, (newValue, oldValue) => {
  if (search.value?.customerIds?.length === 0 || newValue === oldValue) return;

  companyContactNameDropdown.load(newValue);
}, { immediate: true });

// On page rendering
onMounted(async () => {
  projectNameDropdown.load();
  sortByFilterDropdownSingleSelect.load("Sort By Filter");

  // set default in type
  const filterStatus = await sortByFilterDropdownSingleSelect.getValueByLabel("filter by status");
  if (filterStatus) {
    search.value.sortByFilterId = filterStatus;
  }
  getProjectTasks({ pagination: pagination.value });

  activeEmployeesDropdown.load(user.siteId);
  customerNameDropdown.load();
  companyContactNameDropdown.load();
});
</script>
