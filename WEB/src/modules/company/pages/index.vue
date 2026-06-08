<template>
  <q-page padding>
    <q-card class="q-pa-sm">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="CRM" />
              <q-breadcrumbs-el label="Company" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs">
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
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <singleSelectDropdown
                        v-model="search.companyId"
                        label="Company Name"
                        :options="companyNameDropdownSingleSelect.list.value"
                        :filter="companyNameDropdownSingleSelect.filter"
                      />
                      <singleSelectDropdown
                        v-model="search.businessTypeId"
                        label="Business Type"
                        :options="businessTypeDropdown.list.value"
                        :filter="businessTypeDropdown.filter"
                      />
                      <singleSelectDropdown
                        v-model="search.employeeId"
                        label="Primary Employee Person"
                        :options="primaryEmployeesForDropdownSingleSelect.list.value"
                        :filter="primaryEmployeesForDropdownSingleSelect.filter"
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
              <q-menu v-model="showManageDropdownOptions" anchor="bottom right" self="top right" no-parent-event style="width: 320px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Manage Dropdown Options</div>
                  <q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in dropdownTypes"
                      :key="opt.id"
                      clickable
                      :active="selectedField === opt.id"
                      active-class="bg-primary text-white"
                      @click="$router.push({ path: '/manage-dropdowns', state: { id: opt.id, groupName: opt.groupName, moduleName: opt.moduleName } })"
                    >
                      <q-item-section>{{ opt.type }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add Company" no-caps class="text-primary btnRounded" @click="onCompanyAdd(refreshCompanyNameDropdown, refreshCompanyList)" />
                <q-btn v-if="role === 'admin'" icon="o_playlist_add" outline no-caps class="text-primary btnRounded q-ml-sm" @click="showManageDropdownOptions = !showManageDropdownOptions">
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-separator />
      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        flat
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        :filter="filter"
        binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getCompanies"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th auto-width class="text-center hidden" />
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''">
            <q-td auto-width class="text-center hidden">
              <q-icon :name="isExpanded(props.row.id) ? '-' : '+'" class="cursor-pointer custom-plus-minus-icon" @click="toggleExpand(props.row.id)">
                <q-tooltip>{{ isExpanded(props.row.id) ? 'Collapse' : 'Expand' }}</q-tooltip>
              </q-icon>
            </q-td>
            <q-td>
              {{ props.row.name }}
            </q-td>
            <q-td>
              {{ props.row.website ? props.row.website : "" }}
            </q-td>
            <q-td>
              {{ props.row.emailAddress ? props.row.emailAddress : "" }}
            </q-td>
            <q-td>
              {{ props.row.phoneNumber }}
            </q-td>
            <q-td style="width: 10%;">
              <q-select
                v-model="props.row.status.id"
                outlined
                stack-label
                hide-bottom-space
                :dense="true"
                :options="companyStatusList"
                class="company-list"
                option-value="value"
                option-label="text"
                emit-value
                map-options
                :disable="props.row.status.dropDownValue === 'Converted'"
                :class="props.row.status.dropDownValue === 'Converted' ? 'Converted text-black'
                : (props.row.status.dropDownValue === 'Hot Prospect' ? 'Hot-Prospect'
                : (props.row.status.dropDownValue === 'Cold Prospect' ? 'Cold-Prospect'
                : (props.row.status.dropDownValue === 'Warm Prospect' ? 'Warm-Prospect': '')))"
                @update:model-value="onSubmitCompanyStatus(props.row.id, props.row.status.id, refreshCompanyList)"
              />
            </q-td>
            <q-td auto-width class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                @click="onCompanyView(props.row.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                v-if="props.row.status.dropDownValue !== 'Converted'"
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onCompanyEdit(props.row.id, refreshCompanyList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <a
                style="position: relative;"
                class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                @click="onNoteAdd(props.row.id, 'Company', props.row.id, props.row.name, props.row.name, refreshCompanyList)"
              >
                <q-tooltip anchor="bottom middle" self="top middle">
                  Note
                </q-tooltip>
                <q-icon name="o_assignment" />
                <q-badge
                  v-if="props.row.companyCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.companyCount"
                />
              </a>
              <q-icon
                v-if="props.row.status.dropDownValue !== 'Converted'"
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onSubmitCompanyDelete(props.row.id, props.row.name, refreshCompanyList)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-tr v-if="expandedRows.includes(props.row.id)" :props="props">
            <q-td colspan="100%">
              <div class="text-left">
                <q-card>
                  <q-card-section class="card-header with-tools">
                    <h3>Company Contacts</h3>
                  </q-card-section>
                  <q-card-section>
                    <q-table :rows="props.row.companyContacts" :columns="contactColumns" row-key="id">
                      <template #header="contactProps">
                        <q-tr :props="contactProps" class="bg-primary text-white">
                          <q-th v-for="col in contactProps.cols" :key="col.name" :props="contactProps">{{ col.label
                          }}</q-th>
                        </q-tr>
                      </template>
                      <template #body="contactProps">
                        <q-tr :props="contactProps">
                          <q-td>{{ contactProps.row.person.firstName }}</q-td>
                          <q-td>{{ contactProps.row.person.lastName }}</q-td>
                          <q-td>{{ contactProps.row.person.primaryEmailAddress }}</q-td>
                          <q-td>{{ contactProps.row.person.primaryPhoneNumber }}</q-td>
                        </q-tr>
                      </template>
                    </q-table>
                  </q-card-section>
                </q-card>
              </div>
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>

import { ref, onBeforeUnmount, onMounted, computed, watch } from "vue";
import { useAuthStore } from "stores/auth";

import companyService from "modules/company/company.service";
import commonService from "services/common.service";
import manageDropdownsService from "modules/dropdown/dropdown.service";

// Shared Dropdowns
import companyModule from "src/modules/company/utils/dropdowns.js";

// Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared DataTable Features
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Shared Company Dialogs
import {
  initCompanyDialogs,
  onCompanyView,
  onCompanyAdd,
  onCompanyEdit
} from "src/modules/company/utils/dialogs.js";

// Shared Notes Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared Company Actions
import {
  initCompanyActions,
  onSubmitCompanyStatus,
  onSubmitCompanyDelete
} from "src/modules/company/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const authStore = useAuthStore();
const loading = ref(true);
const rows = ref([]);
const filter = ref("");
const expandedRows = ref([]);
const searchLoader = ref(false);
const showFilter = ref(false);
const dropdownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const columns = ref([
  { name: "name", label: "Company Name", field: "name", align: "left", sortable: true },
  { name: "website", label: "Website", field: "website", align: "left", sortable: true },
  { name: "emailAddress", label: "Email Address", field: "emailAddress", align: "left", sortable: true },
  { name: "phoneNumber", label: "Phone Number", field: "phoneNumber", align: "left", sortable: true },
  { name: "statusId", label: "Company Status", field: "statusId", align: "left", sortable: true }
]);

const contactColumns = ref([
  { name: "firstName", label: "First Name", field: "firstName", align: "left" },
  { name: "lastName", label: "Last Name", field: "lastName", align: "left" },
  { name: "emailAddress", label: "Email", field: "emailAddress", align: "left" },
  { name: "phoneNumber", label: "Phone Number", field: "phoneNumber", align: "left" }
]);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const siteId = computed(() => authStore.user?.siteId);

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "company-Index",
  siteId,

  defaultSearch: {
    searchText: "",
    companyId: null,
    businessTypeId: null,
    employeeId: null
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const highlightedId = computed(() => activeRowId.value);
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Company
// ----------------------------------------------------------------------------------------------------------------

const getCompanies = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  companyService.getCompanys(payload).then((resp) => {
    rows.value = resp.data;
    pagination.value = {
      ...pagination.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };

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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const toggleExpand = (rowId) => {
  if (expandedRows.value.includes(rowId)) {
    expandedRows.value = expandedRows.value.filter(id => id !== rowId);
  } else {
    expandedRows.value.push(rowId);
  }
};

function getDropdownTypeByModuleName (moduleName) {
  manageDropdownsService.getDropdownTypeByModuleName(moduleName).then((resp) => {
    dropdownTypes.value = resp;
  });
}

const isExpanded = (rowId) => {
  return expandedRows.value.includes(rowId);
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

const refreshCompanyList = () => {
  getCompanies({ pagination: pagination.value });
};

const refreshCompanyNameDropdown = () => {
  companyNameDropdownSingleSelect.load();
};

const onAdvanceSearch = () => {
  refreshCompanyList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.companyId = null;
  search.value.businessTypeId = null;
  search.value.employeeId = null;

  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
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
// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initCompanyDialogs(activeRowId);
initCommonDialogs(activeRowId);
initCompanyActions(activeRowId);

// ----------------------------
// Applied Filter Labels.
// ----------------------------

const mapFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};

  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.companyId, companyNameDropdownSingleSelect.list, "Company Name"),
  ...mapFilterToLabel(search.value.businessTypeId, businessTypeDropdown.list, "Business Type"),
  ...mapFilterToLabel(search.value.employeeId, primaryEmployeesForDropdownSingleSelect.list, "Primary Employee Person")
}));

function onClearFilters (key) {
  if (key === "Company Name") {
    search.value.companyId = null;
  } else if (key === "Business Type") {
    search.value.businessTypeId = null;
  } else if (key === "Primary Employee Person") {
    search.value.employeeId = null;
  }
  delete appliedFilters.value[key];
  refreshCompanyList();
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  companyNameDropdownSingleSelect,
  businessTypeDropdown,
  primaryEmployeesForDropdownSingleSelect
} = companyModule();

// Get all project status list
const companyStatusList = ref([]);
const options2 = ref([]);
function getDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    companyStatusList.value = responseData;
    options2.value = responseData;
  });
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshCompanyList();
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

onMounted(() => {
  tableRef.value.requestServerInteraction();

  companyNameDropdownSingleSelect.load();
  businessTypeDropdown.load("Business Type");
  primaryEmployeesForDropdownSingleSelect.load();
  getDropdownTypeByModuleName("CRM");
  getDropDown("Company Status");
  document.addEventListener("click", handleDocumentClick);
});

</script>
<style>
.q-field__marginal, .q-field__control, .q-field--auto-height .q-field__native
{
  min-height: 40px !important;
}
</style>
