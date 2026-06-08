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
              <q-breadcrumbs-el label="Project Module" />
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
              <div class="search-container position-relative">
              <searchFilterBar
                v-model="searchText"
                :loading="searchLoader"
                :applied-filters="appliedFilters"
                @toggle-filter="showFilter = !showFilter"
              />
                <!-- Dropdown Content -->
                <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <multiSelectDropdown
                      v-model="search.projectIds"
                      label="Project Name"
                      :options="projectNameDropdown.list.value"
                      :filter="projectNameDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectModuleStatusIds"
                      label="Project Module Status"
                      :options="projectModuleStatusForDropdown.list.value"
                      :filter="projectModuleStatusForDropdown.filter"
                      :isShowAll="true"
                    />
                    <multiSelectDropdown
                      v-model="search.customerIds"
                      label="Customer Name"
                      :options="customerNameDropdown.list.value"
                      :filter="customerNameDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.companyContactIds"
                      label="Company Contact"
                      :options="companyContactNameDropdown.list.value"
                      :filter="companyContactNameDropdown.filter"
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
              <div class="q-ml-sm">
                <q-btn icon="o_add"
                  outline
                  label="Add"
                  no-caps
                  class="text-primary
                  btnRounded"
                  @click="onProjectModuleAdd(null, null, refreshProjectModulesList)"
                >
                  <q-tooltip>Add Project Module</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-separator />
      <q-table
        ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getProjectModules"
      >
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''" :set="(preProjectName = null, resetTracking())">
            <q-td style="width: 5%;" class="hidden">#{{ props.row.projectModuleNumber }}</q-td>
            <q-td style="width: 15%; white-space: normal;">
              <div class="row no-wrap items-center justify-between">
                <span style="flex: 1; word-break: break-word; white-space: normal;">
                  <span v-if="preProjectName !== props.row.project.name" :set="preProjectName = props.row.project.name" class="hoverable-cell" @click="onProjectView(props.row.project.id)">
                    {{ props.row.project.name }}
                  </span>
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
            <q-td style="width: 5%;">
              <q-select
                v-model="props.row.projectModuleStatus.id" outlined stack-label hide-bottom-space :dense="true"
                :options="projectModuleStatusForDropdownSingleSelect.list.value" class="project-module-status-list" option-value="value" option-label="text" emit-value map-options :bg-color="getStatusColor(props.row.projectModuleStatus.dropDownValue)" :disable="isClose" @update:model-value="onSubmit(props.row.id, props.row.projectModuleStatus.id)"
              />
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">{{ props.row.createdBy.person.firstName +" "+ props.row.createdBy.person.lastName }}</q-td>
            <q-td style="width: 5%;" class="text-center">{{ props.row.createdOnUtc }}</q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <a style="position: relative;" class="q-icon notranslate cursor-pointer q-mr-md" @click="onNoteAdd(props.row.id, 'Projects Module', props.row.id, props.row.name, props.row.name, refreshProjectModulesList)">
                <q-tooltip anchor="bottom middle" self="top middle">
                  Note
                </q-tooltip>
                <q-icon name="o_assignment" />
                <q-badge
                  v-if="props.row.projectModuleNotesCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.projectModuleNotesCount"
                />
              </a>
              <q-btn dense flat round icon="o_more_vert" size="sm" color="primary">
                <q-tooltip>More Options</q-tooltip>
                <q-menu auto-close>
                  <q-list style="min-width: 180px">
                    <q-item v-ripple clickable @click="onProjectModuleView(props.row.id)">
                      <q-item-section avatar><q-icon name="o_visibility" size="xs" /></q-item-section>
                      <q-item-section>View</q-item-section>
                    </q-item>
                    <q-item v-ripple clickable @click="onProjectFilesAdd(props.row.id, props.row.name, props.row.project.name)">
                      <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                      <q-item-section>Files</q-item-section>
                    </q-item>

                    <q-item v-ripple clickable @click="onProjectModuleEdit(props.row.id, refreshProjectModulesList)">
                      <q-item-section avatar><q-icon name="o_edit" size="xs" /></q-item-section>
                      <q-item-section>Edit</q-item-section>
                    </q-item>

                    <q-item v-ripple clickable @click="onProjectModuleCopy(props.row.id, props.row.name, refreshProjectModulesList)">
                      <q-item-section avatar><q-icon name="o_content_copy" size="xs" /></q-item-section>
                      <q-item-section>Copy to Project</q-item-section>
                    </q-item>

                    <q-item v-if="role === 'admin'" v-ripple clickable @click="onProjectModuleMoveAsProject(props.row.id, props.row.name, props.row.project.id, refreshProjectModulesList)">
                      <q-item-section avatar><q-icon name="o_arrow_forward" size="xs" /></q-item-section>
                      <q-item-section>Move Module As Project</q-item-section>
                    </q-item>
                    <q-separator />

                    <q-item v-ripple clickable @click="onSubmitProjectModuleDelete(props.row.id, props.row.name, props.row.project.name, refreshProjectModulesList)">
                      <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                      <q-item-section class="text-negative">Delete</q-item-section>
                    </q-item>
                  </q-list>
                </q-menu>
              </q-btn>
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
import { zwConfirm, notifySuccess, getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";
import projectModulesService from "modules/project-modules/projectModules.service";
// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";

// Shared Project Module Dialogs
import {
  initProjectModuleDialogs,
  onProjectModuleAdd,
  onProjectModuleEdit,
  onProjectModuleView,
  onProjectModuleCopy,
  onProjectModuleMoveAsProject,
  onProjectFilesAdd
} from "src/modules/project-modules/utils/dialogs.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView,
} from "src/modules/project/utils/dialogs.js";

// Shared Notes Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initProjectModuleActions,
  onSubmitProjectModuleDelete
} from "src/modules/project-modules/utils/actions.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

// local storage values
const localStorageKey = "Project Modules";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds : [];
const projectModuleStatusIds = filterLocalStorage ? filterLocalStorage.projectModuleStatusIds : [];
const customerIds = filterLocalStorage ? filterLocalStorage.customerIds : [];
const companyContactIds = filterLocalStorage ? filterLocalStorage.companyContactIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

const highlightProjectId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightProjectId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

function setActiveRowIdInLocalStorage (id) {
  const storedData = getLocalStorage(localStorageKey) || {};
  setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
}

// Search variables
const search = ref({
  searchText,
  projectIds,
  projectModuleStatusIds,
  customerIds,
  companyContactIds
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "projectModuleNumber", label: "Project Module Number", field: "projectModuleNumber", align: "left", sortable: true, headerStyle: "width: 100px; display: none;" },
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "name", label: "Project Module Name", field: "name", align: "left", sortable: true },
  { name: "projectModuleStatus.dropDownValue", label: "Project Module Status", field: "projectModuleStatus.dropDownValue", align: "left", sortable: false },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "center", sortable: true }
]);

// open advance search
// const hasActiveFilters = (search) => {
//   return (
//     (search.projectIds && search.projectIds.length > 0) ||
//     (search.projectModuleStatusIds && search.projectModuleStatusIds.length > 0) ||
//     (search.customerIds && search.customerIds.length > 0) ||
//     (search.companyContactIds && search.companyContactIds.length > 0)
//   );
// };

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
const getProjectModules = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  projectModulesService.getProjectModules(payload).then((resp) => {
    rows.value = resp.data;
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

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getProjectModules(propps);
};

// Clear search
const onClear = () => {
  search.value.projectIds = [];
  search.value.projectModuleStatusIds = [];
  search.value.customerIds = [];
  search.value.companyContactIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable :- Initialization Of Dialogs, Actions
// ----------------------------------------------------------------------------------------------------------------
initProjectModuleDialogs(activeRowId);
initCommonDialogs(activeRowId);
initProjectDialogs(activeRowId);
initProjectModuleActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshProjectModulesList = () => {
  getProjectModules({ pagination: pagination.value });
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectNameDropdown } = projectModule();
const { projectModuleStatusForDropdown, projectModuleStatusForDropdownSingleSelect } = projectModuleOfProjectModule();
const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();

// Added colors for task status dropdown list
function getStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Open":
      return "purple-4";
    case "Close":
      return "grey-4";
    case "Completed":
      return "green-4";
    case "In Progress":
      return "yellow-4";
    case "New":
      return "blue-4";
    default:
      return "#ffffff";
    }
  }
}

const onSubmit = async (id, projectModuleStatusId) => {
  const selected = projectModuleStatusForDropdown.list.value.find(item => item.value === projectModuleStatusId);
  try {
    if (selected?.text.toLowerCase() === "close") {
      const resp = await projectModulesService.checkModuleCanBeDeleted(id);
      const canDelete = resp?.canDelete;
      if (canDelete) {
        setTimeout(function () {
          projectModulesService.updateProjectModuleStatus(id, projectModuleStatusId).then(resp => {
            notifySuccess({ message: "Project module status is saved successfully." });
            getProjectModules({ pagination: pagination.value });
          });
        });
      } else {
      // Warning confirmation
        zwConfirm({
          title: "Active Tasks or Activities Found",
          message: "This module has active tasks or activities. You cannot close it.",
          okLabel: "OK",
          cancel: false
        }, () => {
          getProjectModules({ pagination: pagination.value });
        });
      }
    } else {
      setTimeout(function () {
        projectModulesService.updateProjectModuleStatus(id, projectModuleStatusId).then(resp => {
          notifySuccess({ message: "Project module status is saved successfully." });
          getProjectModules({ pagination: pagination.value });
        });
      });
    }
  } catch (error) {
    console.error("Error checking module:", error);
  } finally {
    activeRowId.value = null;
  }
};

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
  ...mapFilterToLabel(search.value.projectModuleStatusIds, projectModuleStatusForDropdown.list, "Project Module Status"),
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.companyContactIds, companyContactNameDropdown.list, "Company Contact")
}));

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Project Module Status") {
    search.value.projectModuleStatusIds = [];
  } else if (key === "Customer") {
    search.value.customerIds = [];
  } else if (key === "Company Contact") {
    search.value.companyContactIds = [];
  }
  delete appliedFilters.value[key];
  getProjectModules({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module Status": return search.value.projectModuleStatusIds?.length || 0;
  case "Customer": return search.value.customerIds?.length || 0;
  case "Company Contact": return search.value.companyContactIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getProjectModules({ pagination: pagination.value });
});

watch(() => search.value.customerIds, (newValue, oldValue) => {
  if (newValue) {
    companyContactNameDropdown.load(newValue);
  }
}, { immediate: true });

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  projectNameDropdown.load();
  projectModuleStatusForDropdownSingleSelect.load("WO Status");
  projectModuleStatusForDropdown.load("WO Status");
  customerNameDropdown.load();
  companyContactNameDropdown.load();
  if (!activeRowId.value && highlightProjectId) {
    activeRowId.value = highlightProjectId;
  }

  document.addEventListener("click", handleDocumentClick);
});
</script>
<style scoped>
.q-item__section--avatar{
min-width: 10px !important;
}
</style>
