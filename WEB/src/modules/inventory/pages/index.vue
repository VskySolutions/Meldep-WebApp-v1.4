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
              <q-breadcrumbs-el label="Infrastructure" />
              <q-breadcrumbs-el label="Inventories" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center q-ml-lg">
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
                      <!-- <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Device Type</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.ItemTypeIds" push class="q-mx-sm w-100 h-auto" use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="itemTypeList" option-value="value" option-label="text" emit-value map-options @filter="filterFn1"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div> -->
                      <multiSelectDropdown
                        v-model="search.ItemTypeIds"
                        label="Device Type"
                        :options="itemTypeForDropdown.list.value"
                        :filter="itemTypeForDropdown.filter"
                        :isShowAll="true"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Device No</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input 
                            v-model="search.code"
                            class="q-mx-sm w-100 h-auto"
                            fill-input
                            :dense="true"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.officeLocationIds"
                        label="Office Location"
                        :options="officeLocationForDropdown.list.value"
                        :filter="officeLocationForDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.inventoryStatusIds"
                        label="Status"
                        :options="inventoryStatusForDropdown.list.value"
                        :filter="inventoryStatusForDropdown.filter"
                        :isShowAll="true"
                      />
                      <!-- <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Office Location</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.officeLocationIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            :dense="true"
                            multiple
                            fill-input
                            input-debounce="0"
                            :options="officeLocationList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            @filter="getAllOfficeLocationDropdownForFilter"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div> -->
                      <!-- <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Status</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.inventoryStatusIds" push class="q-mx-sm w-100 h-auto" use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="inventoryStatusList" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" @filter="filterFn2"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div> -->
                      <multiSelectDropdown
                        v-model="search.employeeIds"
                        label="Employee Name"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                        :isShowAll="true"
                      />
                      <!-- <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Employee Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.employeeIds" push class="q-mx-sm w-100 h-auto" clearable use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="employeeList" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" @filter="filterFn3"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div> -->
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
              <div class="q-ml-xs">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add Inventory"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onInventoryAdd(refreshInventoryList)"
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
                  class="btnRounded q-ml-xs"
                  @click="showSortDialog = true"
                >
                  <q-badge v-if="selectedSortCount > 0" color="green" floating class="q-ml-xs">
                    {{ selectedSortCount }}
                  </q-badge>
                  <q-tooltip>Sort</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="table-scroll-container">
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
          @request="getInventorys"
        >
          <template #loading>
            <q-inner-loading showing color="primary">
              <q-spinner-ios size="40px" class="q-mt-xl" />
            </q-inner-loading>
          </template>
          <template #header="props">
            <q-tr :props="props" class="bg-primary text-white">
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
                 <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
              </q-th>
              <q-th auto-width class="text-center">Actions</q-th>
            </q-tr>
          </template>
          <template #body="props">
            <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
              <q-td v-if="selectedColumnNames.includes('inventorycode')">
                {{ props.row.inventorycode }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('officeLocation.dropDownValue')">
                {{ props.row.officeLocation.dropDownValue }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('itemType.name')">
                {{ props.row.itemType.name }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('modelNameORNumber')">
                {{ props.row.modelNameORNumber }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('processorType')">
                {{ props.row.processorType }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('memoryORRAM')">
                {{ props.row.memoryORRAM }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('serviceCode')">
                {{ props.row.serviceCode }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('inventoryAssignmentList.employee.person.fullName')">
                {{ props.row.inventoryAssignmentList.map(a => a.employee.person.fullName).join(', ') }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('inventoryStatus.dropDownValue')">
                <q-chip :color="getStatusColor(props.row.inventoryStatus.dropDownValue)" name="o_done" class="rounded q-px-lg" text-color="black">
                  {{ props.row.inventoryStatus.dropDownValue }}
                </q-chip>
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('createdBy.person.firstName')"
                class="common-q-td"
              >
                {{ props.row.createdBy.person.fullName }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('createdOnUtc')"
                class="common-q-td"
              >
                {{ props.row.createdOnUtc }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('updatedBy.person.firstName')"
                class="common-q-td"
              >
                {{ props.row.updatedBy.person.fullName }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('updatedOnUtc')"
                class="common-q-td"
              >
                {{ props.row.updatedOnUtc }}
              </q-td>
              <q-td auto-width class="text-center actions">
                <!-- <a style="position: relative;" class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md" 
                  @click="onAddNote(props.row.id, 'Inventory', props.row.id, props.row.name, props.row.name)">
                  <q-tooltip anchor="bottom middle" self="top middle">Note</q-tooltip>
                  <q-icon name="o_assignment" />
                  <q-badge v-if="props.row.inventoryNotesCount > 0" 
                    style="position: absolute; right: -16px; top: -15px;" color="green" text-color="white" 
                    :label="props.row.inventoryNotesCount" />
                </a> -->
                <a
                  style="position: relative;"
                  class="q-icon notranslate cursor-pointer q-mr-md"
                  @click="onNoteAdd(props.row.id, 'Inventory', props.row.id, props.row.name, props.row.name, `${props.row.name}`, refreshInventoryList)"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">
                    Note
                  </q-tooltip>
                  <q-icon name="o_assignment" />
                  <q-badge
                    v-if="props.row.inventoryNotesCount > 0"
                    style="position: absolute; right: -16px; top: -15px;"
                    color="green"
                    text-color="white"
                    :label="props.row.inventoryNotesCount"
                  />
                </a>
                <q-icon
                  name="o_visibility"
                  class="cursor-pointer q-mr-sm"
                  size="xs"
                  @click="onInventoryView(props.row.id)"
                >
                  <q-tooltip>View</q-tooltip>
                </q-icon>
                <q-icon
                  name="o_edit"
                  class="cursor-pointer q-mr-sm"
                  size="xs"
                  @click="onInventoryEdit(props.row.id, refreshInventoryList)"
                >
                  <q-tooltip>Edit</q-tooltip>
                </q-icon>
                <q-icon
                  name="o_delete_outline"
                  class="cursor-pointer"
                  color="negative"
                  size="xs"
                  @click="onSubmitInventoryDelete(props.row.id, props.row.itemType.name, refreshInventoryList)"
                >
                  <q-tooltip>Delete</q-tooltip>
                </q-icon>
              </q-td>
            </q-tr>
            <q-separator />
          </template>
        </q-table>
      </div>
    </q-card>
  </q-page>
  <!-- Multi-Column Level Sorting -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="sortableColumns"
    :multi-sort="multiSort"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { useAuthStore } from "stores/auth";

import inventoryService from "modules/inventory/inventory.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import inventoryModule from "src/modules/inventory/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

import {
  initInventoryDialogs,
  onInventoryAdd,
  onInventoryEdit,
  onInventoryView
} from "src/modules/inventory/utils/dialogs.js";

// Shared Common Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared inventory Actions
import {
  initInventoryActions,
  onSubmitInventoryDelete
} from "src/modules/inventory/utils/actions.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const showSortDialog = ref(false);
const authStore = useAuthStore();
const user = authStore.user;

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "inventorycode", label: "Device No", field: "inventorycode", align: "left", sortable: true, default: true },
  { name: "officeLocation.dropDownValue", label: "Office Location", field: "officeLocation.dropDownValue", align: "left", sortable: true, default: true },
  { name: "itemType.name", label: "Device Type", field: "itemType.name", align: "left", sortable: true, default: true },
  { name: "modelNameORNumber", label: "Model No", field: "modelNameORNumber", align: "left", sortable: true, default: true },
  { name: "processorType", label: "Proccessor", field: "processorType", align: "left", sortable: true, default: true },
  { name: "memoryORRAM", label: "RAM(GB)", field: "memoryORRAM", align: "left", sortable: true, default: true },
  { name: "serviceCode", label: "Service Tag", field: "serviceCode", align: "left", sortable: true, default: true },
  { name: "inventoryAssignmentList.employee.person.fullName", label: "Allocate To", field: "inventoryAssignmentList.employee.person.fullName", align: "left", sortable: false, default: true },
  { name: "inventoryStatus.dropDownValue", label: "Status", field: "inventoryStatus.dropDownValue", align: "left", sortable: true, default: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created On", field: "createdOnUtc", align: "left", sortable: true, default: false },
  { name: "updatedBy.person.firstName", label: "Updated By", field: "updatedBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "left", sortable: true, default: false }
]);

// Get/Map inventory list to table
const getInventorys = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  loading.value = true;
  const sorts = {};
  const multi = multiSort.value;
  for (let i = 0; i < multi.length; i++) {
    const s = multi[i];
    if (s.column && s.direction) {
      sorts[s.column] = s.direction;
    }
  }
  // advanceSearchEnable.value = hasActiveFilters(search.value);
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, sorts, ...search.value };
  saveDataTableState({
    search: search.value,
    pagination: p,
    activeRowId: activeRowId.value,
    sorts
  });
  inventoryService.getInventorys(payload).then((resp) => {
    rows.value = resp.data;
    pagination.value = {
      ...pagination.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const refreshInventoryList = () => {
  getInventorys({ pagination: pagination.value });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  resizeWidths,
  selectedColumnNames,
  saveDataTableState,
  saveResizableWidthState,
  saveColumnsState
} = useSiteTableState({
  storageKey: "inventory-Index",
  siteId: user?.siteId,

  defaultSearch: {
    searchText: "",
    ItemTypeIds: [],
    code: "",
    employeeIds: [],
    inventoryStatusIds: [],
    officeLocationIds: []
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  defaultSorts: {},

  defaultResizableWidth: {},

  defaultColumns: columns.value
    .filter(col => col.default === true)
    .map(col => col.name)
});

const lsSorts = sorts.value || null;
const sortableColumns = computed(() =>
  columns.value.filter(col => col.sortable)
);
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
    refreshInventoryList();
  }
});

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initInventoryDialogs(activeRowId);
initInventoryActions(activeRowId);
initCommonDialogs(activeRowId);

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshInventoryList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.ItemTypeIds = [];
  search.value.code = "";
  search.value.inventoryStatusIds = [];
  search.value.employeeIds = [];
  search.value.officeLocationIds = [];
  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  itemTypeForDropdown,
  officeLocationForDropdown,
  inventoryStatusForDropdown
 } = inventoryModule();
const { activeEmployeesDropdown } = employeeModule();

// Added colors for task status dropdown list
function getStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Assigned":
      return "yellow-5";
    case "Available":
      return "green-5";
    case "Damaged":
      return "red-5";
    case "Donate":
      return "light-blue-5";
    default:
      return "#ffffff";
    }
  }
}
// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshInventoryList();
});

watch(activeRowId, (val) => {
  const formattedSorts = {};

  for (const s of multiSort.value) {
    if (s.column && s.direction) {
      formattedSorts[s.column] = s.direction;
    }
  }

  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    activeRowId: val,
    sorts: formattedSorts
  });
});
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
  ...mapFilterToLabel(search.value.ItemTypeIds, itemTypeForDropdown.list, "Device Type"),
  ...mapFilterToLabel(search.value.inventoryStatusIds, inventoryStatusForDropdown.list, "Status"),
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Employee Name"),
  ...mapFilterToLabel(search.value.officeLocationIds, officeLocationForDropdown.list, "Office Location"),
  ...(search.value.code ? { "Device No": search.value.code } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Device Type": return search.value.ItemTypeIds?.length || 0;
  case "Status": return search.value.inventoryStatusIds?.length || 0;
  case "Employee Name": return search.value.employeeIds?.length || 0;
  case "Office Location": return search.value.officeLocationIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Device Type") {
    search.value.ItemTypeIds = [];
  } else if (key === "Device No") {
    search.value.code = "";
  } else if (key === "Status") {
    search.value.inventoryStatusIds = [];
  } else if (key === "Employee Name") {
    search.value.employeeIds = [];
  } else if (key === "Office Location") {
    search.value.officeLocationIds = [];
  }
  delete appliedFilters.value[key];
  refreshInventoryList();
}

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdown.load();
  itemTypeForDropdown.load();
  inventoryStatusForDropdown.load("Inventory Status");
  officeLocationForDropdown.load(user.siteId, "Employee OrgLocation");
});

</script>
<style scoped>
.Custom-DataTable {
  min-width: max-content;
}
</style>
