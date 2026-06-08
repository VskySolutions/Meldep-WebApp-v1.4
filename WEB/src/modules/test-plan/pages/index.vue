<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <!-- <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" /> -->
              <q-breadcrumbs-el label="SDLC" />
              <q-breadcrumbs-el label="Test Plans" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
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
              <div class="row items-center q-mr-xs">
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
                      <div class="row items-center q-mb-sm hidden">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Test Plan Id</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.testPlanNumber" fill-input :dense="true" class="q-mx-sm" />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Test Plan Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.name" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.planMakerIds"
                        label="Plan Maker"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.planReviewerIds"
                        label="Plan Reviewer"
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
              </div>
              <q-menu v-model="showManageDropdownOptions" anchor="bottom right" self="top right" no-parent-event style="width: 320px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Manage Dropdown Options</div>
                  <q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in dropdownTypes"
                      :key="opt.id"
                      clickable
                      :active="selectedField === opt.id"
                      active-class="bg-primary text-white"
                      @click="$router.push({ path: '/manage-dropdowns', state: { id: opt.id, groupName: opt.groupName, moduleName: opt.moduleName } })"
                    >
                      <q-item-section>{{ opt.type }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Create Test Plan" no-caps class="text-primary btnRounded" @click="onTestPlanAdd(refreshTestPlanList)" />
                <q-btn v-if="role === 'admin'" icon="o_playlist_add" outline no-caps class="text-primary btnRounded q-ml-sm" @click="showManageDropdownOptions = !showManageDropdownOptions">
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn>
              </div>
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
        @request="getAllTestPlan"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''" :set="(preProjectName = null, resetTracking())">
            <q-td style="width: 3%;" class="hidden">#{{ props.row.testPlanNumber }}</q-td>
            <!-- <q-td>{{ props.row.project.name }}</q-td> -->
            <!-- <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;"><span v-if="preProjectName !== props.row.project.name" :set="preProjectName = props.row.project.name">{{ props.row.project.name }}</span></q-td> -->
            <q-td style="width: 15%; white-space: normal;" class="hoverable-cell">
              <div class="row no-wrap items-center justify-between">
                <span style="flex: 1; word-break: break-word; white-space: normal;">
                  <span v-if="preProjectName !== props.row.project.name" :set="preProjectName = props.row.project.name" @click="onProjectView(props.row.project.id)">{{ props.row.project.name }}</span>
                </span>
                <div v-if="shouldShowIcons(props.row.project.name, index)" class="row items-center q-gutter-sm q-ml-sm" style="flex-shrink: 0;">
                  <q-icon
                    name="o_radio_button_checked" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id);
                            $router.push({ path: '/project-center', state: { projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Project Center</q-tooltip>
                  </q-icon>
                  <q-icon
                    v-if="props.row.isEditable"
                    name="o_developer_board" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id);
                            $router.push({ path: '/project-planning/workboard', state: {projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Work Board</q-tooltip>
                  </q-icon>
                </div>
              </div>
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ props.row.name }}</q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 12%;">{{ props.row.planMaker.person.fullName }}</q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 12%;">{{ props.row.planReviewer.person.fullName }}</q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon 
                name="o_visibility" 
                class="cursor-pointer q-mr-sm" 
                size="xs" 
                @click="onTestPlanView(props.row.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon 
                v-if="props.row.isEditable"
                name="o_edit" 
                class="cursor-pointer q-mr-sm" 
                size="xs" 
                @click="onTestPlanEdit(props.row.id, refreshTestPlanList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon 
                name="o_checklist" 
                class="cursor-pointer q-mr-sm" 
                size="xs" 
                @click="$router.push('/test-case?planId='+props.row.id+'&&projectId='+props.row.projectId)"
              >
                <q-tooltip>Test Case</q-tooltip>
              </q-icon>
              <q-icon 
                v-if="props.row.isEditable" 
                name="o_delete_outline" 
                class="cursor-pointer" 
                color="negative" 
                size="xs" 
                @click="onSubmitTestPlanDelete(props.row.id, props.row.name, refreshTestPlanList)"
              >
                <q-tooltip>Delete</q-tooltip>
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
import { ref, onMounted, computed, watch, onBeforeUnmount } from "vue";
import { useAuthStore } from "stores/auth";
import { getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";
import testplansService from "modules/test-plan/testPlan.service";
import manageDropdownsService from "modules/dropdown/dropdown.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Customer Dialogs
import {
  initTestPlanDialogs,
  onTestPlanView,
  onTestPlanAdd,
  onTestPlanEdit
} from "src/modules/test-plan/utils/dialogs.js";

// Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// Shared Test Plan Actions
import {
  initTestPlanActions,
  onSubmitTestPlanDelete
} from "src/modules/test-plan/utils/actions.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const dropdownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

// local storage values
const localStorageKey = "Test Plan";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

const highlightTestPlanId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightTestPlanId);
const highlightedId = computed(() => { return activeRowId.value; });

function setActiveRowIdInLocalStorage (id) {
  const storedData = getLocalStorage(localStorageKey) || {};
  setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
  activeRowId.value = id;
}

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  // { name: "testPlanNumber", label: "Test Plan Id", field: "testPlanNumber", align: "left", sortable: true },
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "planMaker.person.firstName", label: "Plan Maker", field: "planMaker.person.firstName", align: "left", sortable: true },
  { name: "planReviewer.person.firstName", label: "Plan Reviewer", field: "planReviewer.person.firstName", align: "left", sortable: true }
]);

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    const storedData = getLocalStorage(localStorageKey) || {};
    setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
  }
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initTestPlanDialogs(activeRowId);
initProjectDialogs(activeRowId);
initTestPlanActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshTestPlanList = () => {
  getAllTestPlan({ pagination: pagination.value });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel
// ------------------------------------------------------------------------------------

const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

// Search variables
const search = ref({
  searchText: getFilterValue("searchText", ""),
  projectIds: getFilterValue("projectIds", []),
  planMakerIds: getFilterValue("planMakerIds", []),
  planReviewerIds: getFilterValue("planReviewerIds", []),
  name: getFilterValue("name", ""),
  testPlanNumber: getFilterValue("testPlanNumber", 0)
});

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshTestPlanList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.testPlanNumber = undefined;
  search.value.projectIds = [];
  search.value.name = "";
  search.value.planMakerIds = [];
  search.value.planReviewerIds = [];
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

function getDropdownTypeByModuleName (moduleName) {
  manageDropdownsService.getDropdownTypeByModuleName(moduleName).then((resp) => {
    dropdownTypes.value = resp;
  });
}

const shownProjects = new Set();

function resetTracking () {
  shownProjects.clear(); // Clear the set before rendering rows
}

function shouldShowIcons (projectName) {
  if (shownProjects.has(projectName)) {
    return false;
  } else {
    shownProjects.add(projectName);
    return true;
  }
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get Test Plan List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const getAllTestPlan = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.testPlanNumber = search.value.testPlanNumber ? search.value.testPlanNumber : 0;
  // advanceSearchEnable.value = hasActiveFilters(search.value);
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  testplansService.getAllTestPlan(payload).then((resp) => {
    // rows.value = resp.data;
    rows.value = resp.data.map(testPlan => {
      const hasFullAccess = testPlan?.project?.projectUserMappings[0]?.fullAccess ?? false;
      // console.log(hasFullAccess);
      return {
        ...testPlan,
        isNotes: testPlan?.project?.projectUserMappings[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess
      };
    });
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
  ...(search.value.testPlanNumber ? { "Test Plan Id": search.value.testPlanNumber } : {}),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.planMakerIds, activeEmployeesDropdown.list, "Plan Maker"),
  ...mapFilterToLabel(search.value.planReviewerIds, activeEmployeesDropdown.list, "Plan Reviewer"),
  ...(search.value.name ? { "Test Plan Name": search.value.name } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Plan Maker": return search.value.planMakerIds?.length || 0;
  case "Plan Reviewer": return search.value.planReviewerIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Plan Maker") {
    search.value.planMakerIds = [];
  } else if (key === "Plan Reviewer") {
    search.value.planReviewerIds = [];
  } else if (key === "Test Case Status") {
    search.value.statusIds = [];
  } else if (key === "Test Plan Id") {
    search.value.testPlanNumber = "";
  } else if (key === "Test Plan Name") {
    search.value.name = "";
  }
  delete appliedFilters.value[key];
  refreshTestPlanList();
}

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshTestPlanList();
});

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown
} = projectModule();

const { activeEmployeesDropdown } = employeeModule();

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  getDropdownTypeByModuleName("SDLC");
  activeEmployeesDropdown.load();
  projectNameDropdown.load();
  if (!activeRowId.value && highlightTestPlanId) {
    activeRowId.value = highlightTestPlanId;
  }

  document.addEventListener("click", handleDocumentClick);
});
</script>
