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
              <q-breadcrumbs-el label="SOP Assignments" />
              <q-breadcrumbs-el label="List" />
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.name" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.templateIds"
                        label="Template"
                        :options="sopTemplatesDropdown.list.value"
                        :filter="sopTemplatesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.assignedToEmployeeIds"
                        label="Assigned To"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.approverEmployeeIds"
                        label="Approver"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Status"
                        :options="sopAssignmentStatusesDropdown.list.value"
                        :filter="sopAssignmentStatusesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.priorityIds"
                        label="Priority"
                        :options="sopAssignmentPrioritiesDropdown.list.value"
                        :filter="sopAssignmentPrioritiesDropdown.filter"
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
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
              <div class="q-ml-xs">
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_add"
                  outline
                  label="Assign Template"
                  no-caps class="text-primary
                  btnRounded"
                  @click="onSOPTemplateAssign(refreshSOPAssignmentList)"
                />
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
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
        @request="getAllSOPAssignmentList"
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
            :class="highlightedId == props.row.id ? 'highlight' : ''"
          >
            <q-td style="width: 20%;">
              {{ props.row.template.name }}
            </q-td>
            <q-td style="width: 20%;">
              {{ props.row.name }}
            </q-td>
            <q-td style="width: 8%;">
              <div class="row items-center q-gutter-sm">
                <div class="initials-box">
                  {{ getInitials(props.row.assignedToEmployee.person.fullName) }}
                  <q-tooltip>{{ props.row.assignedToEmployee.person.fullName }}</q-tooltip>
                </div>
                <div class="text-caption text-grey-7">
                  ({{ props.row.approvedCount }}/{{ props.row.totalCount }})
                  <q-tooltip>
                    {{ props.row.approvedCount }} Approved out of {{ props.row.totalCount }}
                  </q-tooltip>
                </div>
              </div>
            </q-td>
            <q-td style="width: 8%;">
              <div class="row items-center q-gutter-sm">
                <div class="initials-box">
                  {{ getInitials(props.row.approverEmployee.person.fullName) }}
                  <q-tooltip>{{ props.row.approverEmployee.person.fullName }}</q-tooltip>
                </div>
                <div class="text-caption text-grey-7">
                  ({{ props.row.approvedCount }}/{{ props.row.totalCount }})
                  <q-tooltip>
                    {{ props.row.approvedCount }} Approved out of {{ props.row.totalCount }}
                  </q-tooltip>
                </div>
              </div>
            </q-td>
            <q-td style="width: 10%;">
              {{ props.row.status.dropDownValue }}
            </q-td>
            <q-td style="width: 9%;">
              {{ props.row.priority.dropDownValue }}
            </q-td>
            <q-td class="text-center" style="width: 10%;">
              {{ toDate(props.row.assignedDate) }}
            </q-td>
            <q-td class="text-center" style="width: 10%;">
              {{ toDate(props.row.dueDate) }}
            </q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon
                name="o_visibility"
                size="xs"
                class="cursor-pointer q-mr-sm"
                @click="handleViewClick(props.row)"
              >
                <q-tooltip>
                  {{ getTooltip(props.row) }}
                </q-tooltip>
              </q-icon>
              <q-icon
                v-if="props.row.assignedToEmployeeId === loginUserEmployeeId"
                name="o_assignment_turned_in"
                class="cursor-pointer q-mr-sm"
                size="xs"
                :class="props.row.status.dropDownValue === 'Submitted' || props.row.status.dropDownValue === 'Approved' ? 'text-grey-5 cursor-not-allowed' : 'cursor-pointer'"
                @click="handleChecklistClick(props.row)"
              >
                <q-tooltip>
                  {{
                    props.row.status.dropDownValue === 'Submitted'
                      ? 'Already Submitted'
                      : props.row.status.dropDownValue === 'Approved' ? 'Checklist Approved' : 'Checklist Execution'
                  }}
                </q-tooltip>
              </q-icon>
              <q-icon v-if="role === 'admin'" name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onSOPTemplateAssignmentEdit(props.row.id, refreshSOPAssignmentList)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
                v-if="role === 'admin'"
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                size="xs"
                @click="onSubmitSOPAssignmentDelete(props.row.id, props.row.name, refreshSOPAssignmentList)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";
import { getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

import sopAssignmentService from "modules/sop-assignment/sopAssignment.service";

// SOP Change :- Shared Project Dialogs
import {
  initSOPTemplateAssignmentDialogs,
  onSOPTemplateAssign,
  onSOPTemplateAssignmentEdit
} from "src/modules/sop-assignment/utils/dialogs.js";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";

// SOP Change :- Shared Dropdowns
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import sopAssignmentModule from "src/modules/sop-assignment/utils/dropdowns.js";
import sOPTemplateModule from "src/modules/sop-template/utils/dropdowns.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Project Actions
import {
  initSOPAssignmentActions,
  onSubmitSOPAssignmentDelete
} from "src/modules/sop-assignment/utils/actions.js";
// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

const router = useRouter();

const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const loginUserEmployeeId = user.employeeId;

const showManageDropdownOptions = ref(false);
const { toDate } = useFilters();
const manageDropDownTypes = ref([]);

// local storage values
const localStorageKey = "SOP Assignment";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "template.name", label: "Template Name", field: "template.name", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "assignedToEmployee.person.firstName", label: "Assigned To", field: "assignedToEmployee.person.firstName", align: "left", sortable: true },
  { name: "approverEmployee.person.firstName", label: "Approver", field: "approverEmployee.person.firstName", align: "left", sortable: true },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true },
  { name: "assignedDate", label: "Assigned Date", field: "assignedDate", align: "center", sortable: true },
  { name: "dueDate", label: "Due Date", field: "dueDate", align: "center", sortable: true }
]);

const highlightProjectId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightProjectId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

function setActiveRowIdInLocalStorage (id) {
  const storedData = getLocalStorage(localStorageKey) || {};
  setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
  activeRowId.value = id;
}
// ==========================================================================================
// button click functionality
// ==========================================================================================
const handleChecklistClick = (row) => {
  if (row.status.dropDownValue === "Submitted" || row.status.dropDownValue === "Approved") {
    return;
  }

  setActiveRowIdInLocalStorage(row.id);

  router.push({
    path: "/sop-assignment/checklist-for-assignment",
    state: { id: row.id }
  });
};

const getMode = (row) => {
  const isApprover = row.approverEmployeeId === loginUserEmployeeId;
  const status = row.status?.dropDownValue;

  return !isApprover || status !== "Submitted"
    ? "view"
    : "edit";
};

const handleViewClick = (row) => {
  const isAssignee = row.assignedToEmployeeId === loginUserEmployeeId;
  let mode = getMode(row);
  if (isAssignee) {
    mode = "view";
  }
  setActiveRowIdInLocalStorage(row.id);

  router.push({
    path: "/sop-assignment/checklist-for-approval",
    state: { id: row.id, mode }
  });
};

const getTooltip = (row) => {
  const status = row.status?.dropDownValue;
  const mode = getMode(row);

  if (mode === "edit") return "Approve/Reject Checklist";
  if (status === "Approved") return "Already Approved";
  if (status === "Rejected") return "Sent for Rework";

  return "View";
};
// ==========================================================================================

// Search variables
const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

const getInitials = (name) => {
  if (!name) return "";
  return name
    .split(" ")
    .map(word => word[0])
    .join("")
    .toUpperCase();
};

// Search variables
const search = ref({
  searchText: getFilterValue("searchText", ""),
  name: getFilterValue("name", ""),
  templateIds: getFilterValue("templateIds", []),
  assignedToEmployeeIds: getFilterValue("assignedToEmployeeIds", []),
  approverEmployeeIds: getFilterValue("approverEmployeeIds", []),
  statusIds: getFilterValue("statusIds", []),
  priorityIds: getFilterValue("priorityIds", [])
});

const onAdvanceSearch = () => { refreshSOPAssignmentList(); };

// Clear search
const onAdvanceClear = () => {
  search.value.name = "";
  search.value.templateIds = [];
  search.value.assignedToEmployeeIds = [];
  search.value.approverEmployeeIds = [];
  search.value.statusIds = [];
  search.value.priorityIds = [];
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

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
const getAllSOPAssignmentList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    ...search.value,
    assignedToEmployeeIds: search.value.assignedToEmployeeIds.map(x => x.value),
    approverEmployeeIds: search.value.approverEmployeeIds.map(x => x.value),
    templateIds: search.value.templateIds.map(x => x.value),
    statusIds: search.value.statusIds.map(x => x.value),
    priorityIds: search.value.priorityIds.map(x => x.value)
  };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  sopAssignmentService.getAllSOPAssignmentList(payload).then((resp) => {
    rows.value = resp.data.map(data => {
      return {
        ...data
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

const refreshSOPAssignmentList = () => {
  getAllSOPAssignmentList({ pagination: pagination.value });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initSOPTemplateAssignmentDialogs(activeRowId);
initSOPAssignmentActions(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- Applied Filter Labels.
// ------------------------------------------------------------------------------------
const mapFilterToLabel = (items, list, label) => {
  if (!Array.isArray(items) || !items.length) return {};

  const text = items
    .map(item => {
      const id = typeof item === "object" ? item.value : item;

      const match = list.value.find(opt => opt.value === id);
      return match ? match.text : id;
    })
    .join(", ");

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...(search.value.name ? { Name: search.value.name } : {}),
  ...mapFilterToLabel(search.value.templateIds, sopTemplatesDropdown.list, "Template"),
  ...mapFilterToLabel(search.value.assignedToEmployeeIds, activeEmployeesDropdown.list, "Assigned To"),
  ...mapFilterToLabel(search.value.approverEmployeeIds, activeEmployeesDropdown.list, "Approver"),
  ...mapFilterToLabel(search.value.statusIds, sopAssignmentStatusesDropdown.list, "Status"),
  ...mapFilterToLabel(search.value.priorityIds, sopAssignmentPrioritiesDropdown.list, "Priority")
}));

function getFilterCount (key) {
  switch (key) {
  case "Name": return search.value.name?.length || 0;
  case "Template": return search.value.templateIds?.length || 0;
  case "Assigned To": return search.value.assignedToEmployeeIds?.length || 0;
  case "Approver": return search.value.approverEmployeeIds?.length || 0;
  case "Status": return search.value.statusIds?.length || 0;
  case "Priority": return search.value.priorityIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Name") {
    search.value.name = "";
  } else if (key === "Template") {
    search.value.templateIds = [];
  } else if (key === "Assigned To") {
    search.value.assignedToEmployeeIds = [];
  } else if (key === "Approver") {
    search.value.approverEmployeeIds = [];
  } else if (key === "Status") {
    search.value.statusIds = [];
  } else if (key === "Priority") {
    search.value.priorityIds = [];
  }
  delete appliedFilters.value[key];
  refreshSOPAssignmentList();
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { activeEmployeesDropdown } = employeeModule();
const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();
const { sopAssignmentPrioritiesDropdown, sopAssignmentStatusesDropdown } = sopAssignmentModule();
const {
  sopTemplatesDropdown
} = sOPTemplateModule();

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  if (search.value.searchText) searchLoader.value = true;
  refreshSOPAssignmentList();
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// On page rendering
onMounted(async () => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdown.load(user.siteId);

  // Admin:- Manage all Release-Tracking Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("SOP-Template");
  sopAssignmentPrioritiesDropdown.load("SOP Assignment Priority");
  sopAssignmentStatusesDropdown.load("SOP Assignment Status");
  sopTemplatesDropdown.load();

  if (!activeRowId.value && highlightProjectId) {
    activeRowId.value = highlightProjectId;
  }
  document.addEventListener("click", handleDocumentClick);
});

</script>
