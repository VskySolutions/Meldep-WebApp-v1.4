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
              <q-breadcrumbs-el label="SDLC" />
              <q-breadcrumbs-el label="Requirement Groups" />
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
                          <label class="Cutomlabel q-mt-sm fs-13">Requirement Group Id</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.requirementGroupNumber" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
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
                          <label class="Cutomlabel q-mt-sm fs-13">Requirement Group Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.name" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
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
              </div>
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Create Requirement Group" no-caps class="text-primary btnRounded" @click="onRequirementGroupAdd(refreshRequirementGroupList)" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-table
        ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getAllRequirementGroup"
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
            <q-td style="width: 3%;" class="hidden">#{{ props.row.requirementGroupNumber }}</q-td>
            <q-td style="width: 10%; white-space: normal;" class="hoverable-cell">
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
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ truncateText(props.row.name) }}</q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onRequirementGroupView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.isEditable" name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onRequirementGroupEdit(props.row.id, refreshRequirementGroupList)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_description" class="cursor-pointer q-mr-sm hidden" size="xs" @click="$router.push('/requirement?requirementGroupId='+props.row.id+'&&projectId='+props.row.projectId)">
                <q-tooltip>Requirements</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.isEditable" name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onDelete(props.row)">
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
import { zwConfirmDelete, notifySuccess, getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";
import requirementGroupsService from "modules/requirement-group/requirementGroup.service";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Project Dialogs
import {
  initRequirementGroupDialogs,
  onRequirementGroupView,
  onRequirementGroupAdd,
  onRequirementGroupEdit
} from "src/modules/requirement-group/utils/dialogs.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

// local storage values
const localStorageKey = "Requirement Group";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds : [];
const name = filterLocalStorage ? filterLocalStorage.name : "";
const requirementGroupNumber = filterLocalStorage ? filterLocalStorage.requirementGroupNumber : 0;
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

const highlightProjectId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightProjectId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

// Search variables
const search = ref({
  searchText,
  projectIds,
  name,
  requirementGroupNumber
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const shownProjects = new Set();
const columns = ref([
  // { name: "requirementGroupNumber", label: "Requirement Group Id", field: "requirementGroupNumber", align: "left", sortable: true },
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "name", label: "Requirement Group Name", field: "name", align: "left", sortable: true }
]);

function setActiveRowIdInLocalStorage (id) {
  const storedData = getLocalStorage(localStorageKey) || {};
  setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
}

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    const storedData = getLocalStorage(localStorageKey) || {};
    setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
  }
};

// Get/Map project list to table
const getAllRequirementGroup = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.requirementGroupNumber = search.value.requirementGroupNumber ? search.value.requirementGroupNumber : 0;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  requirementGroupsService.getAllRequirementGroup(payload).then((resp) => {
    rows.value = resp.data.map(requirement => {
      const hasFullAccess = requirement.project?.projectUserMappings[0]?.fullAccess ?? false;
      return {
        ...requirement,
        isNotes: requirement.project?.projectUserMappings[0]?.notes ?? false,
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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const refreshRequirementGroupList = () => {
  getAllRequirementGroup({ pagination: pagination.value });
};

// Search records as per parameters
const onSearch = () => {
  refreshRequirementGroupList();
};

// Clear search
const onClear = () => {
  search.value.requirementGroupNumber = undefined;
  search.value.projectIds = [];
  search.value.name = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

function truncateText (text) {
  if (!text) return "NA";
  const maxLength = 60;
  return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
}

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.name}, ${item.project.name}` }, () => {
    requirementGroupsService.deleteRequirementGroup(item.id).then(resp => {
      notifySuccess({ message: "Requirement Group is deleted successfully." });
      refreshRequirementGroupList();
    });
  }, () => {
    activeRowId.value = null;
  });
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
initRequirementGroupDialogs(activeRowId);
initProjectDialogs(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown
} = projectModule();

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
  ...(search.value.requirementGroupNumber ? { "Requirement Group Id": search.value.requirementGroupNumber } : {}),
  ...(search.value.name ? { "Requirement Group Name": search.value.name } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Requirement Group Id") {
    search.value.requirementGroupNumber = "";
  } else if (key === "Requirement Group Name") {
    search.value.name = "";
  }
  delete appliedFilters.value[key];
  refreshRequirementGroupList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  refreshRequirementGroupList();
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  // getAllProjectListForDropdown();
  projectNameDropdown.load();
  if (!activeRowId.value && highlightProjectId) {
    activeRowId.value = highlightProjectId;
  }

  document.addEventListener("click", handleDocumentClick);
});
</script>
