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
              <q-breadcrumbs-el label="Org Management" />
              <q-breadcrumbs-el label="Leave Credit" />
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
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.employeeIds"
                        label="Employee Name"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Leave Credit Year</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.years"
                            class="q-mx-sm w-100 h-auto"
                            use-input
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="YearOptions"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          >
                            <template #option="{ itemProps, opt }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
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
                  label="Add Leave Credits"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onLeaveCreditAdd(refreshLeaveCreditList)"
                />
              </div>
            </div>
          </div>
        </div></q-card-section>
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
        @request="getLeaveCredits"
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
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td>
              {{ props.row.employeeName }}
            </q-td>
            <q-td class="text-right">
              {{ props.row.casualLeaves }}
            </q-td>
            <q-td class="text-right">
              {{ props.row.usedLeaves }}
            </q-td>
            <q-td class="text-right">
              {{ props.row.remainingLeaves }}
            </q-td>
            <q-td auto-width class="text-center actions">
              <q-icon
                v-if="props.row.employeeActiveStatus"
                name="o_edit"
                :class="{ 'cursor-not-allowed text-grey': !props.row.employeeActiveStatus, 'cursor-pointer': props.row.employeeActiveStatus }"
                @click="onLeaveCreditEdit(props.row.id, refreshLeaveCreditList)"
              >
                <q-tooltip v-if="props.row.employeeActiveStatus">Edit</q-tooltip>
              </q-icon>
              <q-icon v-else name="o_edit" :class="'cursor-not-allowed text-grey'">
                <q-tooltip>Ex-Employee</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>

import { ref, onMounted, computed, watch } from "vue";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { useAuthStore } from "stores/auth";

import leaveService from "../leave.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Leave Dialogs
import {
  initLeaveDialogs,
  onLeaveCreditAdd,
  onLeaveCreditEdit
} from "src/modules/leave/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const user = authStore.user;

// Year Filter
const Year = new Date().getFullYear();
const NextThirtyYear = parseInt(Year) + parseInt(5);
const YearOptions = [];
for (let i = Year; i <= NextThirtyYear; i++) {
  YearOptions.push({ text: i, value: i });
}

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Leave Credit";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "employee.person.firstName", label: "Employee Name", field: "employee.person.firstName", align: "left", sortable: true },
  { name: "casualLeaves", label: "Total Leaves", field: "casualLeaves", align: "right", sortable: true },
  { name: "usedLeaves", label: "Used Leaves", field: "usedLeaves", align: "right", sortable: true },
  { name: "remainingLeaves", label: "Remaining Leaves", field: "remainingLeaves", align: "right", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Leave Credits
// ----------------------------------------------------------------------------------------------------------------

const getLeaveCredits = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  leaveService.getLeaveCredits(payload).then((resp) => {
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

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initLeaveDialogs(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search variables
const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

const search = ref({
  searchText: getFilterValue("searchText", ""),
  employeeIds: getFilterValue("employeeIds", []),
  years: Year
});

const refreshLeaveCreditList = () => {
  getLeaveCredits({ pagination: pagination.value });
};

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshLeaveCreditList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.employeeIds = [];
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const { activeEmployeesDropdown } = employeeModule();

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

const mapSingleFilterToLabel = (id, list, label) => {
  const items = Array.isArray(list) ? list : (list?.value || []);
  if (id == null || id === "") return {};
  const match = items.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Employee Name"),
  ...mapSingleFilterToLabel(search.value.years, YearOptions, "Leave Credit Year")
}));

function getFilterCount (key) {
  switch (key) {
  case "Employee Name": return search.value.employeeIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Employee Name") {
    search.value.employeeIds = [];
  } else if (key === "Leave Credit Year") {
    search.value.years = Year;
  }
  delete appliedFilters.value[key];
  refreshLeaveCreditList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshLeaveCreditList();
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdown.load(user.siteId);

  refreshLeaveCreditList();
});

</script>
