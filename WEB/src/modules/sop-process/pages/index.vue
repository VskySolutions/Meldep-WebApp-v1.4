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
                      <multiSelectDropdown
                        v-model="search.categoryIds"
                        label="Category"
                        :options="sopProcessCategoriesDropdown.list.value"
                        :filter="sopProcessCategoriesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.subCategoryIds"
                        label="Subcategory"
                        :options="sopProcessSubCategoriesDropdown.list.value"
                        :filter="sopProcessSubCategoriesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Status"
                        :options="sopProcessStatusesDropdown.list.value"
                        :filter="sopProcessStatusesDropdown.filter"
                      />
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
              <div>
                <q-btn
                  v-if="role === 'editor' || role === 'both'"
                  icon="o_add"
                  outline label="Add Process"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onSOPProcessAdd(refreshSOPProcessList)"
                />
                <!-- Reset Column Width -->
                <q-btn
                  icon="o_refresh"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="resetColumnsWidth()"
                >
                  <q-tooltip>Reset Columns Width</q-tooltip>
                </q-btn>
                <!-- Column Hide/Show -->
                <columnVisibilityMenu
                  :all-column-names="allColumnNames"
                  :selected-column-names="selectedColumnNames"
                  @update:selected-column-names="selectedColumnNames = $event"
                  @select-all-columns="selectAllColumns"
                  @default-columns="defaultColumns"
                />
                <!-- Button to Open Sorting Dialog -->
                <q-btn
                  color="primary"
                  icon="o_sort"
                  class="btnRounded q-ml-sm"
                  @click="showSortDialog = true"
                >
                  <q-badge v-if="selectedSortCount > 0" color="green" floating class="q-ml-xs">
                    {{ selectedSortCount }}
                  </q-badge>
                  <q-tooltip>Sort</q-tooltip>
                </q-btn>
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
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
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
        :columns="computedColumns"
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
            <q-th
              auto-width
              class="text-center"
            />

            <!-- Visible Columns -->
            <q-th
              v-for="col in props.cols"
              :key="col.name"
              :props="props"
              :style="{
                width: (resizeWidths?.[col.name] || 120) + 'px',
                minWidth: '80px',
                position: 'relative'
              }"
              @click="!isResizing && col.sortable"
            >
              {{ col.label }}

              <div
                class="resize-handle"
                @mousedown.stop="startResize($event, col.name)"
              >
              </div>
            </q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr
            :props="props"
            :class="activeRowId == props.row.id ? 'highlight'
                  : (props.row.statusText?.toLowerCase() === 'submitted'
                      ? 'bg-cyan-1'
                      : '')"
          >
            <q-td
              auto-width
              class="text-center"
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
              ></div>
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('sopProcessNumber')"
              class="common-q-td"
            >
              {{ props.row.sopProcessNumber }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('title')"
              class="common-q-td hoverable-cell"
              :style="{
                width: (resizeWidths?.title || 120) + 'px',
                minWidth: '80px',
                maxWidth: (resizeWidths?.title || 120) + 'px'
              }"
            >
              <span
                class="answer-text"
                @click="() => {
                  setActiveRow(props.row.id);
                  onSOPProcessView(props.row.id);
                }"
              >
                {{ props.row.title }}

                <q-tooltip>
                  View Full Process
                </q-tooltip>
              </span>
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('purpose')"
              class="common-q-td hoverable-cell"
              :style="{
                width: (resizeWidths?.purpose || 120) + 'px',
                minWidth: '80px',
                maxWidth: (resizeWidths?.purpose || 120) + 'px'
              }"
            >
              <span
                class="answer-text"
                @click="() => {
                  setActiveRow(props.row.id);
                  onSOPProcessView(props.row.id);
                }"
              >
                {{ props.row.purpose }}

                <q-tooltip>
                  View Full Process
                </q-tooltip>
              </span>
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('version')"
              class="common-q-td"
            >
              {{ props.row.version }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('category.type')"
              class="common-q-td"
            >
              {{ props.row.category?.type }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('subCategory.dropDownValue')"
              class="common-q-td"
            >
              {{ props.row.subCategory?.dropDownValue }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('statusId')"
              class="common-q-td hoverable-cell"
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
                    onStatusChange(
                      rowId,
                      value,
                      props.row
                    )
                "
                @history="() => onSOPProcessStatusLog(props.row.id)"
              />
                <!-- @submit="
                  ({ rowId, value }) =>
                    onSubmitSOPProcessStatus(
                      rowId,
                      value,
                      refreshSOPProcessList
                    )
                " -->
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('shortDescription')"
              class="common-q-td hoverable-cell"
              :style="{
                width: (resizeWidths?.shortDescription || 120) + 'px',
                minWidth: '80px',
                maxWidth: (resizeWidths?.shortDescription || 120) + 'px'
              }"
            >
              <span
                class="answer-text"
                @click="() => {
                  setActiveRow(props.row.id);
                  onSOPProcessView(props.row.id);
                }"
              >
                {{ props.row.shortDescription }}

                <q-tooltip>
                  View Full Process
                </q-tooltip>
              </span>
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('updatedByName')"
              class="common-q-td"
            >
              {{ props.row.updatedBy?.person.fullName }}
            </q-td>
          <q-td
            v-if="selectedColumnNames.includes('updatedOnUtc')"
            class="common-q-td text-center"
          >
              {{ toDate(props.row.updatedOnUtc) }}
          </q-td>
          <!-- <q-td style="width: 5%;" class="actions" align="left"> -->
          <q-td
            style="width: 5%;"
            class="actions text-left"
          >
              <q-icon
                v-if="
                  canEdit(props.row) &&
                  props.row.statusText?.toLowerCase() === 'approved'
                "
                name="o_cloud_upload"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="() => {
                  setActiveRow(props.row.id);

                  onSubmitSOPProcessPublished(
                    props.row.id,
                    props.row.title,
                    refreshSOPProcessList,
                    sopProcessStatusDropdownSingleSelect
                  );
                }"
                >
                <q-tooltip>Publish SOP Process</q-tooltip>
              </q-icon>
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="() => {
                setActiveRow(props.row.id);
                onSOPProcessView(props.row.id);
              }">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                v-if="canEdit(props.row) ||
                  (
                    canApprove() &&
                    (props.row.statusText?.toLowerCase() === 'submitted' || props.row.statusText?.toLowerCase() === 'draft')
                  )
                "
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="() => {
                  setActiveRow(props.row.id);
                  onSOPProcessEdit(props.row.id, refreshSOPProcessList);
                }">
                <q-tooltip>
                  {{
                    canApprove() &&
                    (props.row.statusText?.toLowerCase() === 'submitted' || props.row.statusText?.toLowerCase() === 'draft')
                      ? 'Review & Edit SOP Process'
                      : 'Edit'
                  }}
                </q-tooltip>
              </q-icon>
              <q-icon
                v-if="
                  canApprove() &&
                  props.row.statusText?.toLowerCase() === 'submitted'
                "
                name="o_check_box"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="() => {
                  setActiveRow(props.row.id);
                  onApproveSOPProcess(props.row);
                }"
              >
                <q-tooltip>Approve SOP Process</q-tooltip>
              </q-icon>
              <q-icon
                v-if="canEdit(props.row)"
                :name="props.row.isActive ? 'o_check_circle' : 'o_cancel'"
                :color="props.row.isActive ? 'positive' : 'negative'"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="() => {
                  setActiveRow(props.row.id);

                  onSubmitSOPProcessActiveInActiveToggle(
                    props.row.id,
                    props.row.isActive,
                    refreshSOPProcessList
                  );
                }"
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
    <!-- <N8nChatbot /> -->
  </q-page>
  <!-- Multi-Column Level Sorting -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="columns"
    :multi-sort="multiSort"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";
import { zwConfirm } from "assets/utils";

import sopProcessService from "../sopProcess.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";

// SOP Change :- Shared Dropdowns
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";
import sOPProcessModule from "src/modules/sop-process/utils/dropdowns.js";

// SOP Change :- Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
// import N8nChatbot from 'src/modules/sop-process/components/_sopChatAssistant.vue';

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

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
const showSortDialog = ref(false);

const authStore = useAuthStore();
const user = authStore.user;
const loggedUserId = user.userId;
const siteId = computed(() => authStore.user?.siteId);

// check login user role
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const editorRoles = ["sop editor"];
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

// permission
const canEdit = (row) =>
  role === "both" && loggedUserId === row.originalCreatedById ||
  (role === "editor" && loggedUserId === row.originalCreatedById);

const canApprove = () =>
  role === "both" ||
  role === "approver";

const canDelete = (row) =>
  role === "both" ||
  (role === "editor" && loggedUserId === row.originalCreatedById);

const showManageDropdownOptions = ref(false);
const { toDate } = useFilters();
const manageDropDownTypes = ref([]);

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "sopProcessNumber", label: "SOP Id", field: "sopProcessNumber", align: "left", sortable: true, default: true },
  { name: "title", label: "Process Title", field: "title", align: "left", sortable: true, default: true },
  { name: "purpose", label: "Purpose", field: "purpose", align: "left", sortable: true, default: true },
  { name: "version", label: "Version", field: "version", align: "left", sortable: true, default: true },
  { name: "category.type", label: "Category", field: "category.type", align: "left", sortable: true, default: true },
  { name: "subCategory.dropDownValue", label: "Subcategory", field: "subCategory.dropDownValue", align: "left", sortable: true, default: true },
  { name: "statusId", label: "Status", field: "statusId", align: "left", sortable: true, default: true },
  { name: "shortDescription", label: "Short Description", field: "shortDescription", align: "center", sortable: true, default: false },
  { name: "updatedByName", label: "Updated By", field: "updatedByName", align: "left", sortable: true, default: true },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "center", sortable: true, default: true }
]);

// const highlightProjectId = filterLocalStorage?.activeRowId || null;
// const activeRowId = ref(highlightProjectId);
// const highlightedId = computed(() => {
//   return activeRowId.value;
// });

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
// const getFilterValue = (key, defaultValue) => {
//   const val = filterLocalStorage?.[key];
//   return val && val.length > 0 ? val : defaultValue;
// };

// Search variables
// const search = ref({
//   searchText: getFilterValue("searchText", ""),
//   title: getFilterValue("title", ""),
//   isActive: getFilterValue("isActive", true)
// });

// const handleDocumentClick = (event) => {
//   const highlightElement = document.querySelector(".highlight");
//   // Check if clicked inside the highlighted row or icons
//   if (highlightElement && !highlightElement.contains(event.target)) {
//     activeRowId.value = null;
//     const storedData = getLocalStorage(localStorageKey) || {};
//     setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
//   }
// };
// const setActiveRow = (id) => {
//   activeRowId.value = id;
//   saveDataTableState();
// };

const setActiveRow = (id) => {
  activeRowId.value = id;

  saveDataTableState({
    search: { ...search.value },
    pagination: { ...pagination.value },
    activeRowId: id,
    sorts: getCurrentSorts()
  });
};

const clearActiveRow = () => {
  activeRowId.value = null;

  saveDataTableState({
    search: { ...search.value },
    pagination: { ...pagination.value },
    activeRowId: null,
    sorts: getCurrentSorts()
  });
};

const getCurrentSorts = () => {
  const formattedSorts = {};

  for (const s of multiSort.value) {
    if (s.column && s.direction) {
      formattedSorts[s.column] = s.direction;
    }
  }

  return formattedSorts;
};
// const handleDocumentClick = (event) => {
//   const highlightElement = document.querySelector(".highlight");

//   if (highlightElement && !highlightElement.contains(event.target)) {
//     activeRowId.value = null;
//     saveDataTableState();
//   }
// };
const handleDocumentClick = (event) => {
  const highlightedRow = event.target.closest("tr.highlight");

  // Keep active row if clicking anywhere inside the active row
  if (highlightedRow) {
    return;
  }

  // Clear active row when clicking outside the active row
  if (activeRowId.value !== null) {
    clearActiveRow();
  }
};

const defaultSearch = {
  searchText: "",
  title: "",
  categoryIds: [],
  subCategoryIds: [],
  statusIds: [],
  isActive: true
};

const defaultPagination = {
  sortBy: "updatedOnUtc",
  descending: true,
  rowsPerPage: 20,
  page: 1
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  resizeWidths,
  selectedColumnNames,
  getTableState,
  saveDataTableState,
  saveResizableWidthState,
  saveColumnsState
} = useSiteTableState({
  storageKey: "sop-Process-Index",
  siteId,
  defaultSearch,
  defaultPagination,
  defaultSorts: {},
  defaultResizableWidth: {},
  defaultColumns: columns.value
    .filter(col => col.default === true)
    .map(col => col.name)
});

// Get/Map project list to table
const getAllSOPProcessList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;

  const sorts = {};
  const multi = multiSort.value;
  for (let i = 0; i < multi.length; i++) {
    const s = multi[i];
    if (s.column && s.direction) {
      sorts[s.column] = s.direction;
    }
  }

  const payload = { page, pageSize: rowsPerPage, sortBy, descending, sorts, ...search.value };

  saveDataTableState({
    search: search.value,
    pagination: props.pagination,
    activeRowId: activeRowId.value,
    sorts
  });

  sopProcessService.getAllSOPProcessList(payload).then((resp) => {
    rows.value = resp.sopProcessesList.map(data => {
      return {
        ...data
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
      activeRowId: activeRowId.value,
      sorts
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

const onApproveSOPProcess = (row) => {
  const approvedStatus = sopProcessStatusDropdownSingleSelect.list.value.find(
    status => status.text?.toLowerCase() === "approved"
  );

  if (!approvedStatus) {
    return;
  }

  // Confirmation popup
  zwConfirm(
    {
      title: "Confirmation",
      message:`Are you sure you want to approve "${row.title}"?`,
      okLabel: "Yes",
      cancelLabel: "No"
    },
    () => {
      onSubmitSOPProcessStatus(
        row.id,
        approvedStatus.value,
        refreshSOPProcessList
      );
    }
  );
};

const resetActiveEdit = () => {
  activeEdit.value = { rowId: null, field: null };
};

const onStatusChange = (rowId, value, row) => {
  const selectedStatus =
    sopProcessStatusDropdownSingleSelect.list.value.find(
      status => status.value === value
    );

  if (selectedStatus?.text?.toLowerCase() === "approved") {
    resetActiveEdit();

    zwConfirm(
      {
        title: "Confirmation",
        message: `Are you sure you want to approve "${row.title}"?`,
        okLabel: "Yes",
        cancelLabel: "No"
      },
      () => {
        onSubmitSOPProcessStatus(
          rowId,
          value,
          refreshSOPProcessList
        );

        resetActiveEdit();
      }
    );

    return;
  }

  onSubmitSOPProcessStatus(
    rowId,
    value,
    refreshSOPProcessList
  );

  resetActiveEdit();
};

//   $q.dialog({
//     title: "Approve SOP Process",
//     message: `Are you sure you want to approve "${row.title}"?`,
//     cancel: true,
//     persistent: true,
//     ok: {
//       label: "Approve",
//       color: "positive",
//       noCaps: true
//     },
//     cancel: {
//       label: "Cancel",
//       flat: true,
//       noCaps: true
//     }
//   }).onOk(() => {
//     onSubmitSOPProcessStatus(
//       row.id,
//       approvedStatus.value,
//       refreshSOPProcessList
//     );
//   });

// function loadSopAssistant() {
//   // Prevent loading the script multiple times
//   if (document.getElementById("sop-assistant-script")) {
//     return;
//   }

//   const script = document.createElement("script");
//   script.id = "sop-assistant-script";
//   // script.src = "https://api-sowbuddy-prasad-local.prasadsawant.site/sop-agent/cdn/vsky_sop_assistant.js";
//   script.src = process.env.AI_Chat_Assistant_Cdn,
//   // script.dataset.apiBase = "https://api-sowbuddy-prasad-local.prasadsawant.site";
//   script.dataset.apiBase = process.env.AI_Chat_Assistant_ApiBase,
//   // script.dataset.apiKey = "ag_067dc77f-6520-42ed-9e10-36c34b122434";
//   script.dataset.apiKey = process.env.AI_Chat_Assistant_ApiKey,
//   script.dataset.title = "SOP Assistant";
//   script.dataset.subtitle = "Workplace process guidance";
//   script.dataset.welcome =
//     "Hello! How can I help with your workplace process?";
//   script.dataset.placeholder = "Ask a process question...";
//   script.dataset.primaryColor = "#123a55";
//   script.dataset.width = "500";
//   script.dataset.height = "820";
//   script.dataset.launcherSize = "60";
//   script.dataset.borderRadius = "20";
//   script.dataset.side = "right";
//   script.dataset.offsetX = "24";
//   script.dataset.offsetY = "24";
//   script.dataset.showSources = "true";

//   document.body.appendChild(script);
// }

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshSOPProcessList = () => {
  getAllSOPProcessList({ pagination: pagination.value });
};

// Search records as per parameters
// const onSearch = () => {
//   refreshSOPProcessList();
// };
const onSearch = () => {
  saveDataTableState();
  refreshSOPProcessList();
};
// Clear search
const onClear = () => {
  search.value.title = "";
  search.value.categoryIds = [];
  search.value.subCategoryIds = [];
  search.value.statusIds = [];
  saveDataTableState();
  onSearch();
};

const lsSorts = sorts.value || null;
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Column resize functionality (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  startResize,
  resetColumnsWidth,
  isResizing
} = useColumnResize({
  columns,
  resizeWidths,
  saveResizableWidthState
});
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Hide/Show Columns (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  selectAllColumns,
  defaultColumns,
  allColumnNames,
  computedColumns
} = useColumnManager({
  columns,
  selectedColumnNames,
  saveColumnsState,
  isResizing
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Sort Filter (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  multiSort,
  addSortLevel,
  removeSortLevel,
  applyMultiSort,
  selectedSortCount
} = useMultiSort({
  lsSorts,
  saveDataTableState,
  onApplySort: () => {
    refreshSOPProcessList();
  }
});
// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initSOPProcessDialogs(activeRowId);
initSOPProcessActions(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  sopProcessCategoriesDropdown,
  sopProcessSubCategoriesDropdown,
  sopProcessStatusDropdownSingleSelect,
  sopProcessStatusesDropdown
} = sOPProcessModule();

const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();

// ------------------------------------------------------------------------------------
// Applied Filter Labels.
// ------------------------------------------------------------------------------------

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
  ...(search.value.title ? { "Process Title": search.value.title } : {}),
  ...mapFilterToLabel(search.value.categoryIds, sopProcessCategoriesDropdown.list, "Category"),
  ...mapFilterToLabel(search.value.subCategoryIds, sopProcessSubCategoriesDropdown.list, "Subcategory"),
  ...mapFilterToLabel(search.value.statusIds, sopProcessStatusesDropdown.list, "Status"),
  ...(search.value.isActive !== null && search.value.isActive !== undefined
    ? {
        "Active/Inactive": search.value.isActive ? "Active" : "Inactive"
      }
    : {})
}));

function onClearFilters (key) {
  if (key === "Process Title") {
    search.value.title = "";
  } else if (key === "Category") {
    search.value.categoryIds = [];
    search.value.subCategoryIds = [];
  } else if (key === "Subcategory") {
    search.value.subCategoryIds = [];
  } else if (key === "Status") {
    search.value.statusIds = [];
  } else if (key === "Active/Inactive") {
    search.value.isActive = null;
  }
  saveDataTableState();
  refreshSOPProcessList();
}

function getFilterCount (key) {
  switch (key) {
  case "Category": return search.value.categoryIds?.length || 0;
  case "Subcategory": return search.value.subCategoryIds?.length || 0;
  case "Status": return search.value.statusIds?.length || 0;
  case "Active/Inactive":
    return search.value.isActive !== null && search.value.isActive !== undefined ? 1 : 0;
  default: return null;
  }
}
// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => search.value.searchText, () => {
  if (search.value.searchText) {
    searchLoader.value = true;
  }

  pagination.value.page = 1;
  saveDataTableState();
  refreshSOPProcessList();
});

watch(
  () => search.value.categoryIds,
  async (newValue, oldValue) => {
    if (JSON.stringify(newValue) === JSON.stringify(oldValue)) {
      return;
    }

    if (!newValue?.length) {
      search.value.subCategoryIds = [];
    } else {
      await sopProcessSubCategoriesDropdown.load(newValue);

      // Remove selected subcategories that are no longer available
      const availableIds =
        sopProcessSubCategoriesDropdown.list.value.map(
          item => item.value
        );

      search.value.subCategoryIds =
        search.value.subCategoryIds.filter(id =>
          availableIds.includes(id)
        );
    }

    saveDataTableState();
  },
  { deep: true }
);

watch(activeRowId, (val) => {
  saveDataTableState({
    search: { ...search.value },
    pagination: { ...pagination.value },
    activeRowId: val,
    sorts: getCurrentSorts()
  });
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// On page rendering
onMounted(async () => {
  // Admin:- Manage all SOP-Process Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("SOP Process");

  document.addEventListener("click", handleDocumentClick);
  sopProcessCategoriesDropdown.load("SOP Process Category");
  if (search.value.categoryIds?.length > 0) {
    await sopProcessSubCategoriesDropdown.load(
      search.value.categoryIds
    );
  }
  sopProcessStatusDropdownSingleSelect.load("SOP Process Status");
  sopProcessStatusesDropdown.load("SOP Process Status");

  tableRef.value.requestServerInteraction();
  // loadSopAssistant();
});

</script>
<style>
.three-dot {
  font-size: 13px;
  margin-left: 0px;
}
.resizable-cell-content {
  display: block;
  width: 100%;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  cursor: pointer;
}
.answer-text {
  display: block;
  width: 100%;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  cursor: pointer;
}
</style>
