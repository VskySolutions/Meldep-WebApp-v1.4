<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <q-card-section class="row items-center justify-end q-pb-sm">
      <div class="search-container position-relative">
        <searchFilterBar
          v-model="search.searchText"
          :loading="searchLoader"
          :applied-filters="appliedFilters"
          class="search-bar"
          @toggle-filter="showFilter = !showFilter"
        />
        <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
          <q-card class="q-pa-sm">
            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">Issue Id</label>
              </div>
              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <q-input v-model="search.issueNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
              </div>
            </div>
            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">Issue Name</label>
              </div>
              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <q-input
                  v-model="search.name"
                  class="q-mx-sm w-100 h-auto"
                  fill-input
                  :dense="true"
                />
              </div>
            </div>
            <multiSelectDropdown
              v-model="search.priorityIds"
              label="Issue Priority"
              :options="issuePriorityForDropdown.list.value"
              :filter="issuePriorityForDropdown.filter"
              :isShowAll="true"
            />
            <multiSelectDropdown
              v-model="search.statusIds"
              label="Status"
              :options="issueStatusForDropdown.list.value"
              :filter="issueStatusForDropdown.filter"
              :isShowAll="true"
            />
            <multiSelectDropdown
              v-model="search.issueTypeIds"
              label="Issue Type"
              :options="issueTypeForDropdown.list.value"
              :filter="issueTypeForDropdown.filter"
              :isShowAll="true"
            />
            <multiSelectDropdown
              v-model="search.employeeIds"
              label="Assign To"
              :options="activeEmployeesDropdown.list.value"
              :filter="activeEmployeesDropdown.filter"
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
      
      <div class="row items-center q-gutter-sm">
        <q-btn
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary q-ml-md"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/issue',
            state: {
              projectId: projectId,
              projectModuleId: projectModuleId,
              requirementId: props.requirementId
            }
          })"
        >
          <q-tooltip>Open Issue List</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>
    <q-separator />

    <q-table
      flat
      :rows="filteredIssues"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      row-key="issueNumber"
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
          <q-td style="width:10%;">
            #{{ props.row.issueNumber }}
          </q-td>

          <q-td style="width:40%;">
            <span
              class="hoverable-cell"
              @click="onIssueView(props.row.id)"
            >
              {{ props.row.name }}
            </span>
          </q-td>

          <q-td style="width:15%;">
            <q-chip
              dense
              class="fs-13"
              :style="{
                backgroundColor: props.row.priorityBgColor,
                color: props.row.priorityTextColor
              }"
            >
              {{ props.row.priorityName }}
            </q-chip>
          </q-td>

          <q-td style="width:15%;">
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
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch, onMounted } from 'vue';
import { useAuthStore } from "stores/auth";
import { notifyError } from 'assets/utils'

import requirementCenterService from 'src/modules/requirement-center/requirementCenter.service'

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import issueModule from "src/modules/issue/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Issue Dialogs
import {
  initIssueDialogs,
  onIssueView
} from "src/modules/issue/utils/dialogs.js";

const emit = defineEmits(['summary'])

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
})

const loading = ref(false);
const rows = ref([]);
const searchLoader = ref(false);
const showFilter = ref(false);
const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);

const columns = [
  { name: 'issueNumber', label: 'NUMBER', field: 'issueNumber', align: 'left', sortable: true },
  { name: 'name', label: 'NAME', field: 'name', align: 'left', sortable: true },
  { name: 'priorityName', label: 'PRIORITY', field: 'priorityName', align: 'left', sortable: true },
  { name: 'statusName', label: 'STATUS', field: 'statusName', align: 'left', sortable: true }
]

const closedStatuses = [
  'Closed',
  'Done',
  'UAT Passed'
];

const projectId = ref('');
const projectModuleId = ref('');
const getIssuesByRequirementId = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;

  const number = search.value.issueNumber ? search.value.issueNumber.replace(/[^0-9]/g, "").replace(/^0+(?!$)/, "") : "";
  search.value.issueNumber = number || "0";
  
  const payload = {
    searchText: search.value.searchText,
    requirementId: props.requirementId,
    issueNumber: search.value.issueNumber,
    name: search.value.name,
    priorityIds: search.value.priorityIds,
    statusIds: search.value.statusIds,
    issueTypeIds: search.value.issueTypeIds,
    employeeIds: search.value.employeeIds,
    sortBy: sortBy,
    descending: descending,
    page: page,
    pageSize: rowsPerPage
  };
  saveDataTableState({
    search: search.value,
    pagination: p,
    activeRowId: activeRowId.value,
    sorts
  });

  try {
    loading.value = true

    const resp = await requirementCenterService.getIssuesByRequirementId(payload)

    rows.value = resp.map(item => ({
      ...item,
      priorityName: item.priority?.dropDownValue ?? '-',
      priorityBgColor: item.priority?.bgColor ?? '#e0e0e0',
      priorityTextColor: item.priority?.color ?? '#000000',

      statusName: item.status?.dropDownValue ?? '-',
      statusBgColor: item.status?.bgColor ?? '#e0e0e0',
      statusTextColor: item.status?.color ?? '#000000'
    }))

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
      activeRowId: activeRowId.value
    });

    emit('summary', {
      total: rows.value.length,
      open: rows.value.filter(r => !closedStatuses.includes(r.statusName)).length
    })

  } catch (err) {
    console.error(err)
    notifyError({ message: 'Failed to load Issues' })
  } finally {
    loading.value = false
    searchLoader.value = false;
  }
}

const filteredIssues = computed(() => rows.value);

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Issue-Tabular-List",
  siteId: siteId,

  defaultSearch: {
    searchText: "",
    issueNumber: "",
    issueTypeIds: [],
    priorityIds: [],
    statusIds: [],
    employeeIds: [],
    name: null
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshIssueList = () => {
  getIssuesByRequirementId({ pagination: pagination.value });
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { activeEmployeesDropdown } = employeeModule();
const {
  issueStatusForDropdown,
  issuePriorityForDropdown,
  issueTypeForDropdown
} = issueModule();


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
  ...mapFilterToLabel(search.value.priorityIds, issuePriorityForDropdown.list, "Issue Priority"),
  ...mapFilterToLabel(search.value.statusIds, issueStatusForDropdown.list, "Status"),
  ...mapFilterToLabel(search.value.issueTypeIds, issueTypeForDropdown.list, "Issue Type"),
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Assign To"),
  ...(search.value.issueNumber > 0 ? { "Issue Id": search.value.issueNumber } : {}),
  ...(search.value.name ? { "Issue Name": search.value.name } : {})
}));
// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onAdvanceSearch = () => { refreshIssueList(); };

// Clear search
const onAdvanceClear = () => {
  search.value.issueNumber = undefined;
  search.value.name = "";
  search.value.priorityIds = [];
  search.value.statusIds = [];
  search.value.issueTypeIds = [];
  search.value.employeeIds = [];
  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initIssueDialogs(activeRowId);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
  async () => {
    await getIssuesByRequirementId({
      pagination: pagination.value
    });
  },
  { immediate: true }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshIssueList();
  }
);

// ----------------------------------------------------------------------------------------------------------------
// On page rendering
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  issueStatusForDropdown.load("Issue Status");
  issuePriorityForDropdown.load("Issue Priority");
  issueTypeForDropdown.load("Issue Type");
  activeEmployeesDropdown.load(siteId.value);
});
</script>
