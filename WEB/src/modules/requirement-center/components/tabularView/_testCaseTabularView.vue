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
                <label class="Cutomlabel q-mt-sm fs-13">Test Case Id</label>
              </div>
              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <q-input v-model="search.testCaseNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
              </div>
            </div>
            <multiSelectDropdown
              v-model="search.planIds"
              label="Test Plan Name"
              :options="testPlansByProjectIdForDropdown.list.value"
              :filter="testPlansByProjectIdForDropdown.filter"
            />
            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">Release Version</label>
              </div>
              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <q-input v-model="search.versionNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
              </div>
            </div>
            <multiSelectDropdown
              v-model="search.testedBys"
              label="Tested By"
              :options="activeEmployeesDropdown.list.value"
              :filter="activeEmployeesDropdown.filter"
            />
            <multiSelectDropdown
              v-model="search.statusIds"
              label="Test Case Status"
              :options="testCaseStatusForDropdown.list.value"
              :filter="testCaseStatusForDropdown.filter"
              :isShowAll="true"
            />
            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">Created From Date</label>
              </div>
              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <div class="input-group q-ml-sm w-100 h-auto">
                  <q-input
                    v-model="search.fromDate" fill-input dense mask="##/##/####"
                  >
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date
                            v-model="search.fromDate" mask="MM/DD/YYYY"
                            @update:model-value="() => $refs.qDateProxy.hide()"
                          />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
            </div>
            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">Created To Date</label>
              </div>
              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <div class="input-group q-ml-sm w-100 h-auto">
                  <q-input v-model="search.toDate" fill-input dense mask="##/##/####">
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date
                            v-model="search.toDate"
                            mask="MM/DD/YYYY"
                            @update:model-value="() => $refs.qDateProxy.hide()"
                          />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
            </div>
            <!-- Search and Clear Buttons -->
            <div class="row justify-end q-gutter-sm q-mb-sm">
              <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
              <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
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
          class="text-primary q-ml-md"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/test-case',
            state: {
              projectId: projectId,
              projectModuleId: projectModuleId,
              requirementId: props.requirementId
            }
          })"
        >
          <q-tooltip>Open Test Case List</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>

    <q-separator />
    <q-table
      flat
      :rows="filteredTestCases"
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
          <!-- Number -->
          <q-td style="width:15%;">
            #{{ props.row.testCaseNumber }}
          </q-td>

          <!-- Name -->
          <q-td style="width:45%;">
            <span
              class="hoverable-cell"
              @click="onTestCaseView(props.row.id, props.row.planId)"
            >
              {{ props.row.name }}
            </span>
          </q-td>

          <!-- Owner -->
          <q-td style="width:20%;">
            {{ props.row.owner }}
          </q-td>

          <!-- Status -->
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
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch, onMounted } from 'vue';
import { notifyError } from "assets/utils";
import { useAuthStore } from "stores/auth";
import { format } from 'date-fns';
import useFilters from "composables/useFilters";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// SOP Change :- Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import testPlanModule from "src/modules/test-plan/utils/dropdowns.js";
import testCaseModule from "src/modules/test-case/utils/dropdowns.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Project Dialogs
import {
  initTestCaseDialogs,
  onTestCaseView
} from "src/modules/test-case/utils/dialogs.js";

const emit = defineEmits(['summary']);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  },
  projectId: {
    type: String,
    required: true
  },
});

const loading = ref(false);
const searchLoader = ref(false);
const showFilter = ref(false);
const { toDate } = useFilters();
const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);

const rows = ref([]);

const columns = [
  { name: 'testCaseNumber', label: 'NUMBER', field: 'testCaseNumber', align: 'left', sortable: true },
  { name: 'name', label: 'NAME', field: 'name', align: 'left', sortable: true },
  { name: 'owner', label: 'TESTED BY', field: 'owner', align: 'left', sortable: true },
  { name: 'statusName', label: 'STATUS', field: 'statusName', align: 'left', sortable: true }
]

const projectId = ref('');
const projectModuleId = ref('');
const getTestCasesByRequirementId = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  
  search.value.fromDate = search.value.fromDate
    ? toDate(search.value.fromDate)
    : null;

  search.value.toDate = search.value.toDate
    ? toDate(search.value.toDate)
    : null;

  const number = search.value.testCaseNumber
    ? search.value.testCaseNumber
        .replace(/[^0-9]/g, "")
        .replace(/^0+(?!$)/, "")
    : "";

  search.value.testCaseNumber = number || "0";

  const payload = {
    searchText: search.value.searchText,
    requirementId: props.requirementId,
    testCaseNumber: search.value.testCaseNumber,
    planIds: search.value.planIds,
    versionNumber: search.value.versionNumber,
    testedBys: search.value.testedBys,
    statusIds: search.value.statusIds,
    fromDate: search.value.fromDate,
    toDate: search.value.toDate,
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
    loading.value = true;

    const resp = await requirementCenterService.getTestCasesByRequirementId(payload);
    rows.value = resp.map(item => ({
      ...item,
      owner: item.testedByEmployee?.person?.fullName ?? '-',
      statusName: item.status?.dropDownValue ?? '-',
      statusTextColor: item.status?.color ?? '-',
      statusBgColor: item.status?.bgColor ?? '-'
    }));

    if (rows.value.length) {
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
      passed: rows.value.filter(r => r.statusName === 'Resolved').length
    });
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Test Cases" });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

const filteredTestCases = computed(() => rows.value);
const defaultSearch = {
  searchText: "",
  testedBys: [],
  testCaseNumber: 0,
  versionNumber: "",
  statusIds: [],
  fromDate: "",
  toDate: "",
  planIds: []
};

const defaultPagination = {
  sortBy: "createdOnUtc",
  descending: true,
  rowsPerPage: 20,
  page: 1
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Test-Case-Tabular-List",
  siteId: siteId.value,

  defaultSearch,
  defaultPagination
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshTestCaseList = () => {
  getTestCasesByRequirementId({ pagination: pagination.value });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const { activeEmployeesDropdown } = employeeModule();
const { testPlansByProjectIdForDropdown } = testPlanModule();
const {
  testCaseStatusForDropdown
} = testCaseModule();

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initTestCaseDialogs(activeRowId);

// Clear search
const onClear = () => {
  search.value.testCaseNumber = undefined;
  search.value.planIds = [];
  search.value.testedBys = [];
  search.value.statusIds = [];
  search.value.versionNumber = "";
  search.value.fromDate = null;
  search.value.toDate = null;
  saveDataTableState({
    search: {
      ...defaultSearch
    }
  });
  onSearch();
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
  ...mapFilterToLabel(search.value.planIds, testPlansByProjectIdForDropdown.list, "Test Plan Name"),
  ...mapFilterToLabel(search.value.testedBys, activeEmployeesDropdown.list, "Tested By"),
  ...mapFilterToLabel(search.value.statusIds, testCaseStatusForDropdown.list, "Test Case Status"),
  ...(search.value.testCaseNumber > 0 ? { "Test Case Id": search.value.testCaseNumber } : {}),
  ...(search.value.versionNumber
  ? { "Release Version": search.value.versionNumber }
  : {}),
  ...(search.value.fromDate ? { "Created From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "Created To Date": search.value.toDate } : {})
}));

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onSearch = () => { refreshTestCaseList(); };

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
    async () => {
      await getTestCasesByRequirementId({
        pagination: pagination.value
      });
    },
  {
    immediate: true
  }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshTestCaseList();
  }
);

onMounted(async () => {
  if (props.projectId) testPlansByProjectIdForDropdown.load(props.projectId);
  activeEmployeesDropdown.load();
  testCaseStatusForDropdown.load("Test Case Status");
});
</script>
