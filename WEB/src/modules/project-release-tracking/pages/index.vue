<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Project Management" />
              <q-breadcrumbs-el label="Project Release Tracking" />
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
          <div class="col-12 col-md-4">
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
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.deploymentOwnerIds"
                        label="Deployment Owner"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
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
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add Release" no-caps class="text-primary btnRounded" @click="onReleaseTrackingAdd(refreshReleaseTrackingList)" />
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="showManageDropdownOptions = !showManageDropdownOptions"
                >
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
        @request="getAllReleaseTracking"
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
          <q-tr
            :props="props"
            :class="[
              highlightedId == props.row.id ? 'highlight' : '',
              props.row.statusText.toLowerCase() !== 'draft' ? 'bg-cyan-1' : ''
            ]"
            :set="(preProjectName = null, resetTracking())"
          >
            <q-td style="width: 5%;" class="text-right">{{ props.row.versionNumber }}</q-td>
            <q-td style="width: 15%; white-space: normal;" :class="(props.row.isEditable || props.row.isView) ? 'hoverable-cell' : ''">
              <div class="row no-wrap items-center justify-between">
                <span style="flex: 1; word-break: break-word; white-space: normal;">
                  <span
                    v-if="preProjectName !== props.row.project.name"
                    :set="preProjectName = props.row.project.name"
                    @click="(props.row.isEditable || props.row.isView) && onProjectView(props.row.project.id)"
                  >{{ props.row.project.name }}
                  </span>
                </span>
                <div
                  v-if="shouldShowIcons(props.row.project.name, index)"
                  class="row items-center q-gutter-sm q-ml-sm"
                  style="flex-shrink: 0;"
                >
                  <q-icon
                    v-if="props.row.isEditable || props.row.isView"
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
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">
              {{ props.row.infraInstance.instanceType.dropDownValue + " (" + props.row.infraInstance.url + ")" }}
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 30%;">
              {{ props.row.name }}
            </q-td>
            <q-td
              style="width: 10%;"
              class="common-q-td"
              :class="{ 'hoverable-cell' : props.row.isEditable }"
              @click="activeEdit = { rowId: props.row.id, field: 'status' }"
            >
              <quickEditSingleSelect
                field="status"
                :row-id="props.row.id"
                :value="props.row.statusId"
                :display-value="props.row.statusText"
                :editable="props.row.isEditable"
                :options="filteredStatusList"
                :active-edit="activeEdit"
                :show-history="true"
                @popup-show="handlePopupShow"
                @cancel="activeEdit = { rowId: null, field: null }"
                @submit="({ rowId, value }) => onSubmitReleaseTrackingStatus(rowId, value, refreshReleaseTrackingList)"
                @history="() => onSiteModifiedLog(props.row.id, props.row.name, 'Release Tracking Status')"
              />
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">
              {{ props.row.deploymentOwner.person.fullName }}
            </q-td>
            <q-td class="text-center" style="width: 5%;">
              {{ toDate(props.row.plannedReleaseDate) }}
            </q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon v-if="props.row.isEditable || props.row.isView" name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onReleaseTrackingView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.isEditable" name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onReleaseTrackingEdit(props.row.id, refreshReleaseTrackingList)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.isEditable" name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onSubmitReleaseTrackingDelete(props.row.id, props.row.name, refreshReleaseTrackingList)">
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
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";

import releaseTrackingService from "modules/project-release-tracking/projectReleaseTracking.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";

// Shared DataTable Features
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";

// SOP Change :- Shared Dropdowns
import releaseTrackingModule from "src/modules/project-release-tracking/utils/dropdowns.js";

// SOP Change :- Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
// SOP Change :- Shared Project Dialogs
import {
  initReleaseTrackingDialogs,
  onReleaseTrackingView,
  onReleaseTrackingAdd,
  onReleaseTrackingEdit
} from "src/modules/project-release-tracking/utils/dialogs.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// SOP Change :- Shared Site Dialogs
import {
  initSiteDialogs,
  onSiteModifiedLog
} from "src/modules/sites/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initReleaseTrackingActions,
  onSubmitReleaseTrackingStatus,
  onSubmitReleaseTrackingDelete
} from "src/modules/project-release-tracking/utils/actions.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const showManageDropdownOptions = ref(false);
const { toDate } = useFilters();
const manageDropDownTypes = ref([]);
const activeEdit = ref({ rowId: null, field: null });
const hideDraft = ref(true);

const siteId = computed(() => authStore.user?.siteId);
const highlightedId = computed(() => activeRowId.value);

const defaultSearch = {
  searchText: "",
  projectIds: [],
  deploymentOwnerIds: []
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
  saveDataTableState
} = useSiteTableState({
  storageKey: "project-Release-Tracking-Index",
  siteId,

  defaultSearch,
  defaultPagination
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const shownProjects = new Set();
const columns = ref([
  { name: "versionNumber", label: "Version Number", field: "versionNumber", align: "left", sortable: true },
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "infraInstance.instanceType.dropDownValue", label: "Infra Instance", field: "infraInstance.instanceType.dropDownValue", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "statusId", label: "Status", field: "statusId", align: "left", sortable: true },
  { name: "deploymentOwner.person.firstName", label: "Deployment Owner", field: "deploymentOwner.person.firstName", align: "left", sortable: true },
  { name: "plannedReleaseDate", label: "Planned Release Date", field: "plannedReleaseDate", align: "center", sortable: true }
]);

const filteredStatusList = computed(() => {
  const list = releaseTrackingStatusDropdownSingleSelect.list.value || [];

  return list.map(item => {
    if (item.text === "Draft" && hideDraft.value) {
      return {
        ...item,
        disable: true
      };
    }
    return item;
  });
});

function handlePopupShow () {
  releaseTrackingStatusDropdownSingleSelect.load("Release Tracking Status");
}

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    saveDataTableState({
      activeRowId: null
    });
  }
};

// Get/Map project list to table
const getAllReleaseTracking = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  releaseTrackingService.getAllReleaseTracking(payload).then((resp) => {
    rows.value = resp.projectReleaseTrackingsList.map(release => {
      const userMapping = release.project?.projectUserMappings?.[0];

      return {
        ...release,
        isEditable: role === "admin" || (userMapping?.fullAccess ?? false),
        isView: role === "admin" || (userMapping?.viewOnly ?? false)
      };
    });
    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });
    saveDataTableState({
      search: { ...search.value },
      pagination: { ...pagination.value },
      activeRowId: activeRowId.value
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshReleaseTrackingList = () => {
  getAllReleaseTracking({ pagination: pagination.value });
};

// Search records as per parameters
const onSearch = () => {
  refreshReleaseTrackingList();
};

// Clear search
const onClear = () => {
  search.value.projectIds = [];
  search.value.deploymentOwnerIds = [];
  saveDataTableState({
    search: {
      ...defaultSearch
    }
  });
  onSearch();
};

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

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initReleaseTrackingDialogs(activeRowId);
initProjectDialogs(activeRowId);
initSiteDialogs(activeRowId);
initReleaseTrackingActions(activeRowId);

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
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.deploymentOwnerIds, activeEmployeesDropdown.list, "Deployment Owner")
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Deployment Owner": return search.value.deploymentOwnerIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Deployment Owner") {
    search.value.deploymentOwnerIds = [];
  }
  refreshReleaseTrackingList();
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown
} = projectModule();

const {
  releaseTrackingStatusDropdownSingleSelect
} = releaseTrackingModule();

const { activeEmployeesDropdown } = employeeModule();
const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  if (search.value.searchText) searchLoader.value = true;
  refreshReleaseTrackingList();
});

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  search.value.projectModuleIds = [];
}, { immediate: true });

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// On page rendering
onMounted(async () => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdown.load();
  projectNameDropdown.load();
  releaseTrackingStatusDropdownSingleSelect.load("Release Tracking Status");

  // Admin:- Manage all Release-Tracking Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("Release-Tracking");

  document.addEventListener("click", handleDocumentClick);
});

</script>
