<template>
  <q-page padding>
    <q-card>
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <!-- <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" /> -->
              <q-breadcrumbs-el label="User Management" />
              <q-breadcrumbs-el label="Employees" />
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
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel">Status</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.employeeStatus"
                            clearable
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            use-input
                            :dense="true"
                            :options="employeeStatusList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Employee Code</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.employeeCode"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.employeeIds"
                        label="Name"
                        :options="allEmployeesForDropdown.list.value"
                        :filter="allEmployeesForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Email</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.primaryEmailAddress"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="email"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.employeeTypeIds"
                        label="Type"
                        :options="employeeTypeForDropdown.list.value"
                        :filter="employeeTypeForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.employeeDepartmentIds"
                        label="Department"
                        :options="departmentNameDropdown.list.value"
                        :filter="departmentNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.employeeDesignationIds"
                        label="Designation"
                        :options="employeeDesignationForDropdown.list.value"
                        :filter="employeeDesignationForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.orgLocationIds"
                        label="Org Location"
                        :options="employeeOrgLocationForDropdown.list.value"
                        :filter="employeeOrgLocationForDropdown.filter"
                        :isShowAll="true"
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
              <div class="q-ml-xs">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add Employee"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onEmployeeAdd(refreshEmployeeList)"
                />
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
        @request="getEmployees"
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
          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''">
            <q-td>{{ props.row.employeeCode }}</q-td>
            <q-td style="width: 20%;">{{ props.row.person.fullName }}</q-td>
            <q-td style="width: 20%;">{{ props.row.person.primaryEmailAddress }}</q-td>
            <q-td>{{ toPhone(props.row.person.primaryPhoneNumber, props.row.person.address.addressCountry.twoLetterIsoCode) }}</q-td>
            <q-td>{{ props.row.employeeType.map(type => type.employeeTypeDropdown.dropDownValue).join(', ') }}</q-td>
            <q-td>{{ props.row.employeeDepartment.map(dep => dep.department.name).join(', ') }}</q-td>
            <q-td>{{ props.row.employeeDesignation.map(des => des.designation.dropDownValue).join(', ') }}</q-td>
            <q-td>{{ props.row.employeeOrgLocation.map(org => org.orgLocation.dropDownValue).join(', ') }}</q-td>
            <q-td style="width: 10%;" class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                @click="onEmployeeView(props.row.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onEmployeeEdit(props.row.id, refreshEmployeeList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onSubmitEmployeeDelete(props.row.id, props.row.person.fullName, refreshEmployeeList)"
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
import { ref, onBeforeUnmount, onMounted, computed, watch } from "vue";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";

import employeesService from "modules/employee/employee.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import departmentModule from "src/modules/department/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Employee Dialogs
import {
  initEmployeeDialogs,
  onEmployeeView,
  onEmployeeAdd,
  onEmployeeEdit
} from "src/modules/employee/utils/dialogs.js";

// Shared Employee Actions
import {
  initEmployeeActions,
  onSubmitEmployeeDelete
} from "src/modules/employee/utils/actions.js";

// ----------------------------
// Common variables
// ----------------------------
const { toPhone } = useFilters();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);

// ----------------------------
// local storage values
// ----------------------------
const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "employee-Index",
  siteId,
  defaultSearch: {
    searchText: "",
    employeeIds: [],
    employeeCode: "",
    primaryEmailAddress: "",
    employeeTypeIds: [],
    employeeDepartmentIds: [],
    employeeDesignationIds: [],
    orgLocationIds: [],
    employeeStatus: "Active"
  },
  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const highlightedId = computed(() => activeRowId.value);

// ----------------------------
// Table variables
// ----------------------------
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "employeeCode", label: "Employee Code", field: "employeeCode", align: "left", sortable: true },
  { name: "person.fullName", label: "Name", field: "person.fullName", align: "left", sortable: true },
  { name: "person.primaryEmailAddress", label: "Email Address", field: "person.primaryEmailAddress", align: "left", sortable: true },
  { name: "person.primaryPhoneNumber", label: "Phone Number", field: "person.primaryPhoneNumber", align: "left", sortable: true },
  { name: "employeeTypeDropdown.dropDownValue", label: "Type", field: "employeeTypeDropdown.dropDownValue", align: "left", sortable: false },
  { name: "department.name", label: "Department", field: "department.name", align: "left", sortable: false },
  { name: "designation.dropDownValue", label: "Designation", field: "designation.dropDownValue", align: "left", sortable: false },
  { name: "orgLocation.dropDownValue", label: "Org Location", field: "orgLocation.dropDownValue", align: "left", sortable: false }
]);

const refreshEmployeeList = () => {
  getEmployees({ pagination: pagination.value });
};

const handleDocumentClick = (event) => {
  if (event.target.closest(".q-dialog")) {
    return;
  }

  const highlightElement = document.querySelector(".highlight");

  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;

    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: null
    });
  }
};
// -----------------------------------------------------------------------------
// Get/Map employee list to table
// -----------------------------------------------------------------------------

const getEmployees = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  employeesService.getEmployees(payload).then((resp) => {
    rows.value = resp.data;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
    saveDataTableState({
      search: search.value,
      pagination: props.pagination,
      activeRowId: activeRowId.value
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel
// ------------------------------------------------------------------------------------

const onAdvanceSearch = () => {
  refreshEmployeeList();
};

// ----------------------------
// Clear search
// ----------------------------
const onAdvanceClear = () => {
  search.value.employeeIds = [];
  search.value.employeeCode = "";
  search.value.primaryEmailAddress = "";
  search.value.employeeTypeIds = [];
  search.value.employeeDepartmentIds = [];
  search.value.employeeDesignationIds = [];
  search.value.orgLocationIds = [];
  saveDataTableState({
    search: { ...search.value }
  });
  // Set default "Active Employee" status if available
  search.value.employeeStatus = "Active";
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initEmployeeDialogs(activeRowId);
initEmployeeActions(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const { departmentNameDropdown } = departmentModule();

const {
  allEmployeesForDropdown,
  employeeTypeForDropdown,
  employeeDesignationForDropdown,
  employeeOrgLocationForDropdown
} = employeeModule();

const employeeStatusList = ref(["Active", "All", "Ex-Employee"]);

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
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapSingleFilterToLabel(search.value.employeeStatus, employeeStatusList, "Employee Status"),
  ...mapFilterToLabel(search.value.employeeIds, allEmployeesForDropdown.list, "Employee Name"),
  ...mapFilterToLabel(search.value.employeeTypeIds, employeeTypeForDropdown.list, "Type"),
  ...mapFilterToLabel(search.value.employeeDepartmentIds, departmentNameDropdown.list, "Department"),
  ...mapFilterToLabel(search.value.employeeDesignationIds, employeeDesignationForDropdown.list, "Designation"),
  ...mapFilterToLabel(search.value.orgLocationIds, employeeOrgLocationForDropdown.list, "Org Location"),
  ...(search.value.employeeCode ? { "Employee Code": search.value.employeeCode } : {}),
  ...(search.value.primaryEmailAddress ? { "Employee Email": search.value.primaryEmailAddress } : {})
}));

function onClearFilters (key) {
  if (key === "Employee Status") {
    search.value.employeeStatus = null;
  } else if (key === "Employee Name") {
    search.value.employeeIds = [];
  } else if (key === "Type") {
    search.value.employeeTypeIds = [];
  } else if (key === "Department") {
    search.value.employeeDepartmentIds = [];
  } else if (key === "Designation") {
    search.value.employeeDesignationIds = [];
  } else if (key === "Org Location") {
    search.value.orgLocationIds = [];
  } else if (key === "Employee Code") {
    search.value.employeeCode = "";
  } else if (key === "Employee Email") {
    search.value.primaryEmailAddress = "";
  }
  delete appliedFilters.value[key];
  refreshEmployeeList();
}

function getFilterCount (key) {
  switch (key) {
  case "Employee Name": return search.value.employeeIds?.length || 0;
  case "Type": return search.value.employeeTypeIds?.length || 0;
  case "Department": return search.value.employeeDepartmentIds?.length || 0;
  case "Designation": return search.value.employeeDesignationIds?.length || 0;
  case "Org Location": return search.value.orgLocationIds?.length || 0;
  default: return null;
  }
}

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshEmployeeList();
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

onMounted(() => {
  tableRef.value.requestServerInteraction();
  allEmployeesForDropdown.load();
  departmentNameDropdown.load();
  employeeTypeForDropdown.load("EmploymentType");
  employeeDesignationForDropdown.load("Employee Designation");
  employeeOrgLocationForDropdown.load("Employee OrgLocation");
  document.addEventListener("click", handleDocumentClick);
});

</script>
