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
              <q-breadcrumbs-el label="Standard Operating Procedures (SOPs)" />
              <q-breadcrumbs-el label="List" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon v-if="key !== 'Active/Inactive'" name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
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
                          <label class="Cutomlabel q-mt-sm fs-13">Title</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.title" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Active/Inactive</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-option-group
                            v-model="search.isActive"
                            :options="[
                              { label: 'Active', value: true },
                              { label: 'Inactive', value: false }
                            ]"
                            type="radio"
                            inline
                            dense
                          />
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
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
              <div class="q-ml-xs">
                <q-btn v-if="role === 'editor' || role === 'both'" icon="o_add" outline label="Add Process" no-caps class="text-primary btnRounded" @click="onSOPProcessAdd(refreshSOPProcessList)" />
                <q-btn
                  v-if="adminRole === 'admin'"
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
        @request="getAllSOPProcessList"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th auto-width class="text-center" />
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr
            :props="props"
            :class="highlightedId == props.row.id ? 'highlight' : ''"
          >
            <q-td
              style="width: 2%; position: relative;"
            >
              <div
                v-if="canEdit(props.row)"
                :class="['dot-circle q-mr-xs hoverable-cell', props.row.isActive ? 'dot-active' : 'dot-inactive']"
                @click="() => { onSubmitSOPProcessActiveInActiveToggle(props.row.id, props.row.isActive, refreshSOPProcessList) }"
              >
                <q-tooltip v-if="!props.row.isActive">Set Active?</q-tooltip>
                <q-tooltip v-else>Set Inactive?</q-tooltip>
              </div>
              <div
                v-else
                :class="['dot-circle q-mr-xs', props.row.isActive ? 'dot-active' : 'dot-inactive']" style="cursor: default;"
              />
            </q-td>
            <q-td style="width: 17%;" class="hoverable-cell">
              <span
                class="cursor-pointer"
                @click="onSOPProcessView(props.row.id)"
              >
                {{ truncateText(props.row.title) }}
              </span>
              <q-icon
                v-if="shouldShowMore(props.row.title)"
                name="o_more_horiz"
                class="cursor-pointer text-primary three-dot"
                @click.stop="onSOPProcessView(props.row.id)"
              >
                <q-tooltip>View Full Process</q-tooltip>
              </q-icon>
            </q-td>
            <q-td style="width: 25%;">
              <span>{{ truncateText(props.row.purpose) }}</span>
              <q-icon
                v-if="shouldShowMore(props.row.purpose)"
                name="o_more_horiz"
                class="cursor-pointer text-primary three-dot"
                @click.stop="onSOPProcessView(props.row.id)"
              >
                <q-tooltip>View Full Process</q-tooltip>
              </q-icon>
            </q-td>
            <q-td style="width: 5%;">
              {{ props.row.version }}
            </q-td>
            <q-td style="width: 8%;">
              {{ props.row.category.type }}
            </q-td>
            <q-td style="width: 8%;">
              {{ props.row.subCategory.dropDownValue }}
            </q-td>
            <q-td
              style="width: 14%;"
              class="hoverable-cell common-q-td"
              @click="
                isSOPProcessStatusEditable(
                  props.row,
                  role,
                  loggedUserId,
                  sopProcessStatusDropdownSingleSelect.list.value
                )
                  ? activeEdit = { rowId: props.row.id, field: 'status' }
                  : null
              "
            >
              <quickEditSingleSelect
                field="status"
                :row-id="props.row.id"
                :value="props.row.statusId"
                :display-value="
                  props.row.statusText?.toLowerCase() === 'submitted' &&
                  (role === 'approver' || role === 'both')
                    ? 'Waiting for Approval'
                    :props.row.statusText?.toLowerCase() === 'approved' &&
                    role === 'editor'
                      ? 'Waiting for Published'
                      : props.row.statusText
                "
                :editable="
                  isSOPProcessStatusEditable(
                    props.row,
                    role,
                    loggedUserId,
                    sopProcessStatusDropdownSingleSelect.list.value
                  )
                "
                :options="
                  getVisibleStatusOptionsByRole(
                    props.row,
                    role,
                    loggedUserId,
                    sopProcessStatusDropdownSingleSelect.list.value
                  )
                "
                :active-edit="activeEdit"
                :show-history="role === 'editor' || role === 'approver' || role === 'both'"
                @cancel="activeEdit = { rowId: null, field: null }"
                @submit="
                  ({ rowId, value }) =>
                    onSubmitSOPProcessStatus(
                      rowId,
                      value,
                      refreshSOPProcessList
                    )
                "
                @history="() => onSOPProcessStatusLog(props.row.id)"
              />
            </q-td>
            <q-td class="hidden">
              <span>{{ truncateText(props.row.shortDescription) }}</span>
              <q-icon
                v-if="shouldShowMore(props.row.shortDescription)"
                name="o_more_horiz"
                class="cursor-pointer text-primary three-dot"
                @click.stop="onSOPProcessView(props.row.id)"
              >
                <q-tooltip>View Full Process</q-tooltip>
              </q-icon>
            </q-td>
            <q-td style="width: 8%;">
              {{ props.row.updatedBy.person.fullName }}
            </q-td>
            <q-td class="text-center" style="width: 8%;">
              {{ toDate(props.row.updatedOnUtc) }}
            </q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon
                v-if="
                  canEdit(props.row) &&
                  props.row.statusText?.toLowerCase() === 'approved'
                "
                name="o_cloud_upload"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="onSubmitSOPProcessPublished(props.row.id, props.row.title, refreshSOPProcessList, sopProcessStatusDropdownSingleSelect)">
                <q-tooltip>Publish SOP Process</q-tooltip>
              </q-icon>
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onSOPProcessView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                v-if="canEdit(props.row) ||
                  (
                    canApprove() &&
                    props.row.statusText?.toLowerCase() === 'submitted'
                  )
                "
                :name="
                  canApprove() &&
                  props.row.statusText?.toLowerCase() === 'submitted'
                    ? 'o_check_box'
                    : 'o_edit'
                "
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="onSOPProcessEdit(props.row.id, refreshSOPProcessList)">
                <q-tooltip>
                  {{
                    canApprove() &&
                    props.row.statusText?.toLowerCase() === 'submitted'
                      ? 'Approve SOP Process'
                      : 'Edit'
                  }}
                </q-tooltip>
              </q-icon>
              <q-icon
                v-if="canEdit(props.row)"
                :name="props.row.isActive ? 'o_check_circle' : 'o_cancel'"
                :color="props.row.isActive ? 'positive' : 'negative'"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="
                  onSubmitSOPProcessActiveInActiveToggle(
                    props.row.id,
                    props.row.isActive,
                    refreshSOPProcessList
                  )
                "
              >
                <q-tooltip>
                  {{
                    props.row.isActive
                      ? 'Set Inactive?'
                      : 'Set Active?'
                  }}
                </q-tooltip>
              </q-icon>
              <q-icon
                v-if="canDelete(props.row)"
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                size="xs"
                @click="onSubmitSOPProcessDelete(props.row.id, props.row.title, refreshSOPProcessList)">
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
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";
import { getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

import sopProcessService from "../sopProcess.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";

// SOP Change :- Shared Dropdowns
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";
import sOPProcessModule from "src/modules/sop-process/utils/dropdowns.js";

// SOP Change :- Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";

// SOP Change :- Shared Project Dialogs
import {
  initSOPProcessDialogs,
  onSOPProcessView,
  onSOPProcessAdd,
  onSOPProcessEdit,
  onSOPProcessStatusLog
} from "src/modules/sop-process/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initSOPProcessActions,
  onSubmitSOPProcessDelete,
  onSubmitSOPProcessStatus,
  getVisibleStatusOptionsByRole,
  isSOPProcessStatusEditable,
  onSubmitSOPProcessPublished,
  onSubmitSOPProcessActiveInActiveToggle
} from "src/modules/sop-process/utils/actions.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const activeEdit = ref({ rowId: null, field: null });

const authStore = useAuthStore();
const user = authStore.user;
const loggedUserId = user.userId;

// check login user role
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const editorRoles = ["admin", "site-super-admin", "system-super-admin", "sop editor"];
const approverRoles = ["sop approver"];

const hasAdminRole = user?.roles?.some(r => adminRoles.includes(r));
const hasEditorRole = user?.roles?.some(r => editorRoles.includes(r));
const hasApproverRole = user?.roles?.some(r => approverRoles.includes(r));

const adminRole = hasAdminRole ? "admin" : "";

// Role priority handling
const role =
  hasEditorRole && hasApproverRole
    ? "both"
    : hasApproverRole
      ? "approver"
      : hasEditorRole
        ? "editor"
        : "";

// permissions
const canEdit = (row) =>
  role === "both" && loggedUserId === row.createdBy.id ||
  (role === "editor" && loggedUserId === row.createdBy.id);

const canApprove = () =>
  role === "both" ||
  role === "approver";

const canDelete = (row) =>
  role === "both" ||
  (role === "editor" && loggedUserId === row.createdBy.id);

const showManageDropdownOptions = ref(false);
const { toDate } = useFilters();
const manageDropDownTypes = ref([]);

// local storage values
const localStorageKey = "SOP Process";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "title", label: "Process Title", field: "title", align: "left", sortable: true },
  { name: "purpose", label: "Purpose", field: "purpose", align: "left", sortable: true },
  { name: "version", label: "Version", field: "version", align: "left", sortable: true },
  { name: "category.type", label: "Category", field: "category.type", align: "left", sortable: true },
  { name: "subCategory.dropDownValue", label: "SubCategory", field: "subCategory.dropDownValue", align: "left", sortable: true },
  { name: "statusId", label: "Status", field: "statusId", align: "left", sortable: true },
  // { name: "shortDescription", label: "Short Description", field: "shortDescription", align: "center", sortable: true },
  { name: "updatedByName", label: "Updated By", field: "updatedByName", align: "left", sortable: true },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "center", sortable: true }
]);

const highlightProjectId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightProjectId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

// truncate text after 50 characters
const truncateText = (text, length = 60) => {
  if (!text) return "";

  return text.length > length
    ? text.slice(0, length)
    : text;
};

// Show three-dot icon only when text is actually truncated
const shouldShowMore = (text, length = 60) => {
  return !!text && text.length > length;
};

// Search variables
const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

// Search variables
const search = ref({
  searchText: getFilterValue("searchText", ""),
  title: getFilterValue("title", ""),
  isActive: getFilterValue("isActive", true)
});

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
const getAllSOPProcessList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  sopProcessService.getAllSOPProcessList(payload).then((resp) => {
    rows.value = resp.sopProcessesList.map(data => {
      return {
        ...data
      };
    });
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;

    setLocalStorage(localStorageKey, {
      ...search.value,
      pagination: pagination.value,
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
const refreshSOPProcessList = () => {
  getAllSOPProcessList({ pagination: pagination.value });
};

// Search records as per parameters
const onSearch = () => {
  refreshSOPProcessList();
};

// Clear search
const onClear = () => {
  search.value.title = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initSOPProcessDialogs(activeRowId);
initSOPProcessActions(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  sopProcessStatusDropdownSingleSelect
} = sOPProcessModule();

const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();

// ------------------------------------------------------------------------------------
// Applied Filter Labels.
// ------------------------------------------------------------------------------------
const appliedFilters = computed(() => ({
  ...(search.value.title ? { "Process Title": search.value.title } : {}),
  ...(search.value.isActive !== null && search.value.isActive !== undefined
    ? {
        "Active/Inactive": search.value.isActive ? "Active" : "Inactive"
      }
    : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Active/Inactive":
    return search.value.isActive !== null && search.value.isActive !== undefined ? 1 : 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Process Title") {
    search.value.title = "";
  } else if (key === "isActive") {
    search.value.isActive = search.value.isActive;
  }
  delete appliedFilters.value[key];
  refreshSOPProcessList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => search.value.searchText, () => {
  if (search.value.searchText) searchLoader.value = true;
  refreshSOPProcessList();
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// On page rendering
onMounted(async () => {
  tableRef.value.requestServerInteraction();

  // Admin:- Manage all SOP-Process Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("SOP Process");

  if (!activeRowId.value && highlightProjectId) {
    activeRowId.value = highlightProjectId;
  }
  document.addEventListener("click", handleDocumentClick);
  sopProcessStatusDropdownSingleSelect.load("SOP Process Status");
});

</script>
<style>
.three-dot {
  font-size: 13px;
  margin-left: 0px;
}
</style>
