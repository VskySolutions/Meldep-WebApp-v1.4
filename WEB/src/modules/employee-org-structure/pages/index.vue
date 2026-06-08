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
              <q-breadcrumbs-el label="Org Management" />
              <q-breadcrumbs-el label="Org Structure" />
            </q-breadcrumbs>
          </div>
           <div class="col-12 col-md-4">
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Year</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.years" fill-input dense mask="####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" v-model="isPopupVisible" transition-show="scale" transition-hide="scale">
                                    <q-date ref="date3ref" v-model="search.years" default-view="Years" emit-immediately minimal mask="YYYY" class="myDate" :options="onlyYears" @update:model-value="onUpdateMv2" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                       <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Level</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                      <q-select
                        v-model="search.level"
                        class="q-mx-sm w-100 h-auto"
                        clearable
                        use-input
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :options="employeeLevelList"
                        option-value="value"
                        option-label="text"
                        emit-value
                        map-options
                        @filter="getAllLevelListDropdownForFilter"
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
                      <multiSelectDropdown
                        v-model="search.departmentIds"
                        label="Department"
                        :options="departmentNameDropdown.list.value"
                        :filter="departmentNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.employeeDesignationIds"
                        label="Role"
                        :options="employeeDesignationsDropdown.list.value"
                        :filter="employeeDesignationsDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.managerIds"
                        label="Manager"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.employeeIds"
                        label="Employee"
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
              <div class="q-ml-sm">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add Org Structure"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onEmployeeOrgStructureAdd(refreshEmployeeOrgStructureList)"
                />
                <q-btn icon="o_preview" outline label="Preview Org Structure" no-caps class="q-ml-sm text-primary btnRounded" @click="$router.push('/employee-org-structure-preview')" />
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
        @request="getAllEmployeeOrgStructureList"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td style="width: 5%;">{{ props.row.level }}</q-td>
            <q-td style="width: 20%;">{{ props.row.manager.person.fullName }}</q-td>
            <q-td style="width: 20%;">{{ props.row.employee.person.fullName }}</q-td>
            <q-td style="width: 20%;">{{ props.row.department.name }}</q-td>
            <q-td style="width: 20%;"  class="common-q-td">
              <span v-if="props.row.employeeOrgStructureDesignationMapping.length > 0">
                {{ props.row.employeeOrgStructureDesignationMapping
                    .map(item => item.employeeDesignation.dropDownValue)
                    .join(', ') }}
              </span>
            </q-td>
            <q-td style="width: 5%;">{{ props.row.sortOrder }}</q-td>
            <q-td style="width: 10%;" class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                @click="onEmployeeOrgStructureView(props.row.id, refreshEmployeeOrgStructureList)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onEmployeeOrgStructureEdit(props.row.id, refreshEmployeeOrgStructureList)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative" @click="onDelete(props.row)">
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
import { useAuthStore } from "stores/auth";
import { ref, onMounted, watch, computed } from "vue";
import { zwConfirmDelete, notifySuccess, setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";

import orgStructureService from "modules/employee-org-structure/employeeOrgStructure.service";
import commonService from "services/common.service";

// Shared Dropdowns
import departmentModule from "src/modules/department/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import employeeOrgStructureModule from "src/modules/employee-org-structure/utils/dropdowns.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Employee Org Structure Dialogs
import {
  initEmployeeOrgStructureDialogs,
  onEmployeeOrgStructureView,
  onEmployeeOrgStructureAdd,
  onEmployeeOrgStructureEdit
} from "src/modules/employee-org-structure/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const authStore = useAuthStore();
const user = authStore.user;
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const isPopupVisible = ref(false);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "EmployeeOrgStructure";
const filterLocalStorage = getLocalStorage(localStorageKey);
const years = filterLocalStorage ? filterLocalStorage.years : getCurrentMonthYear();
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Employee OrgStructures
// ----------------------------------------------------------------------------------------------------------------

const getAllEmployeeOrgStructureList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.years = search.value.years === "" ? getCurrentMonthYear() : search.value.years;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  orgStructureService.getAllEmployeeOrgStructureList(payload).then((resp) => {
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

const onUpdateMv2 = (val) => {
  // Update the selected year and close the popup
  search.value.years = val; // Update the reactive property with the selected year
  isPopupVisible.value = false; // Close the popup
};

function getCurrentMonthYear () {
  const today = new Date();
  const year = today.getFullYear();
  return `${year}`; // Format as 'Month-YYYY'
}


const refreshEmployeeOrgStructureList = () => {
  getAllEmployeeOrgStructureList({ pagination: pagination.value });
};
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "level", label: "Level", field: "level", align: "left", sortable: true },
  { name: "manager.person.firstName", label: "Manager", field: "manager.person.fullName", align: "left", sortable: true },
  { name: "employee.person.firstName", label: "Employee", field: "employee.person.fullName", align: "left", sortable: true },
  { name: "department.name", label: "Department", field: "department.name", align: "left", sortable: true },
  { name: "employeeOrgStructureDesignationMapping.employeeDesignation.dropDownValue", label: "Role", field: "employeeOrgStructureDesignationMapping.employeeDesignation.dropDownValue", align: "left", sortable: false },
  { name: "sortOrder", label: "Sort Order", field: "sortOrder", align: "left", sortable: true }
]);

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initEmployeeOrgStructureDialogs(activeRowId);


// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { departmentNameDropdown } = departmentModule();
const { employeeDesignationsDropdown } = employeeOrgStructureModule();
const { activeEmployeesDropdown } = employeeModule();

// Get all employee level list for dropdown
const employeeLevelList = ref([]);
const employeeLevelListFilter = ref([]);
function getAllEmployeeLevelListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropDownText, value: item.dropdownValue }));
    employeeLevelList.value = responseData;
    employeeLevelListFilter.value = responseData;
  });
}

function getAllLevelListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeLevelList.value = employeeLevelListFilter.value;
    } else {
      employeeLevelList.value = employeeLevelListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.manager.person.fullName}, ${item.employee.person.fullName}` }, () => {
    orgStructureService.deleteEmployeeOrgStructure(item.id).then(resp => {
      notifySuccess({ message: "Org structure is deleted successfully." });
      refreshEmployeeOrgStructureList();
    });
  }, () => {
    activeRowId.value = null;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

const search = ref({
  searchText: getFilterValue("searchText", ""),
  departmentIds: getFilterValue("departmentIds", []),
  employeeDesignationIds: getFilterValue("employeeDesignationIds", []),
  managerIds: getFilterValue("managerIds", []),
  employeeIds: getFilterValue("employeeIds", []),
  level: getFilterValue("level", ""),
  years
});

const onAdvanceSearch = () => {
  refreshEmployeeOrgStructureList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.level = "";
  search.value.departmentIds = [];
  search.value.employeeDesignationIds = [];
  search.value.managerIds = [];
  search.value.employeeIds = [];
  search.value.years = getCurrentMonthYear();
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// Advance Filter :- Applied Filter Labels.
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
  ...mapFilterToLabel(search.value.level, employeeLevelList, "Level"),
  ...mapFilterToLabel(search.value.departmentIds, departmentNameDropdown.list, "Department"),
  ...mapFilterToLabel(search.value.employeeDesignationIds, employeeDesignationsDropdown.list, "Role"),
  ...mapFilterToLabel(search.value.managerIds, activeEmployeesDropdown.list, "Manager"),
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Employee"),
  ...(search.value.years ? { Year: search.value.years } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Department": return search.value.departmentIds?.length || 0;
   case "Role": return search.value.employeeDesignationIds?.length || 0;
  case "Manager": return search.value.managerIds?.length || 0;
  case "Employee": return search.value.employeeIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Level") {
    search.value.level = "";
  } else if (key === "Department") {
    search.value.departmentIds = [];
  }  else if (key === "Role") {
    search.value.employeeDesignationIds = [];
  } else if (key === "Manager") {
    search.value.managerIds = [];
  } else if (key === "Employee") {
    search.value.employeeIds = [];
  }
  if (key !== "Year") {
    delete appliedFilters.value[key];
  }
  refreshEmployeeOrgStructureList();
}


// ------------------------------------------------------------------------------------
// On Change
// ------------------------------------------------------------------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshEmployeeOrgStructureList();
});

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------
onMounted(() => {
  tableRef.value.requestServerInteraction();
  departmentNameDropdown.load();
  employeeDesignationsDropdown.load("Employee Designation");
  getAllEmployeeLevelListForDropDown("Level Type");
  activeEmployeesDropdown.load(user.siteId);
});

</script>
