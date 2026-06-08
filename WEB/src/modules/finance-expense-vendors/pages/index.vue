<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-3 col-sm-2 col-md-3 col-lg-4 col-xl-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <!-- <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" /> -->
              <q-breadcrumbs-el label="Expenses" />
              <q-breadcrumbs-el label="Vendors" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-6 col-sm-8 col-md-7 col-lg-6 col-xl-6">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center q-mr-xs">
                <div class="search-container position-relative">
                    <searchFilterBar
                      v-model="searchText"
                      :loading="searchLoader"
                      :applied-filters="appliedFilters"
                      @toggle-filter="showFilter = !showFilter"
                    />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Vendor Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.vendorName"
                            class="q-mx-sm w-100 h-auto"
                            fill-input :dense="true"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Vendor Email</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.vendorEmail"
                            class="q-mx-sm w-100 h-auto"
                            fill-input
                            :dense="true"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.vendorIds"
                        label="Owner Name"
                        :options="ownerNameDropdown.list.value"
                        :filter="ownerNameDropdown.filter"
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
                <q-btn icon="o_add" outline label="Add Vendor Account" no-caps class="text-primary btnRounded" @click="onVendorAdd(refreshVendorList)" />
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
        @request="getAllVendorList"
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
            <q-td style="width: 15%;">{{ props.row.vendorCode }}</q-td>
            <q-td style="width: 15%;">{{ props.row.vendorName }}</q-td>
            <q-td style="width: 20%;">{{ props.row.vendor_Email }}</q-td>
            <q-td style="width: 10%;">{{ props.row.vendor_Phone }}</q-td>
            <q-td style="width: 15%;">{{ props.row.person.fullName }}</q-td>
            <q-td style="width: 20%;">{{ props.row.address.addressCountry.name }}</q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onVendorView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onVendorEdit(props.row.id, refreshVendorList)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onSubmitVendorDelete(props.row.id, props.row.vendorName, refreshVendorList)">
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
import { ref, onMounted, watch, computed } from "vue";
import expenseVendorBankAccountService from "modules/finance-expense-vendors/financeExpenseVendors.service";
import { getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Dropdowns
import financeExpenseVendorsModule from "src/modules/finance-expense-vendors/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Vendor Dialogs
import {
  initVendorDialogs,
  onVendorView,
  onVendorAdd,
  onVendorEdit,
} from "src/modules/finance-expense-vendors/utils/dialogs.js";

// Shared Vendor Actions
import {
  initVendorActions,
  onSubmitVendorDelete,
} from "src/modules/finance-expense-vendors/utils/actions.js";

// ------------------------------------------------------------------------------------
// Common variables
// ------------------------------------------------------------------------------------

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

// ------------------------------------------------------------------------------------
// Local storage values
// ------------------------------------------------------------------------------------

const localStorageKey = "Vendor";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const vendorName = filterLocalStorage ? filterLocalStorage.vendorName : "";
const vendorEmail = filterLocalStorage ? filterLocalStorage.vendorEmail : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get Vendor List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const getAllVendorList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  expenseVendorBankAccountService.getAllVendorList(payload).then((resp) => {
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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshVendorList = () => {
   getAllVendorList({ pagination: pagination.value });
};

// ------------------------------------------------------------------------------------
// DataTable:- Columns
// ------------------------------------------------------------------------------------

const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "vendorCode", label: "Vendor Code", field: "vendorCode", align: "left", sortable: true },
  { name: "vendorName", label: "Vendor Name", field: "vendorName", align: "left", sortable: true },
  { name: "vendor_Email", label: "Vendor Email", field: "vendor_Email", align: "left", sortable: true },
  { name: "vendor_Phone", label: "Vendor Phone", field: "vendor_Phone", align: "left", sortable: true },
  { name: "person.firstName", label: "Owner Name", field: "person.fullName", align: "left", sortable: true },
  { name: "address.addressCountry.name", label: "Country", field: "address.addressCountry.name", align: "left", sortable: true }
]);

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initVendorDialogs(activeRowId);
initVendorActions(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel
// ------------------------------------------------------------------------------------

const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

// Search variables
const search = ref({
  searchText,
  vendorIds: getFilterValue("vendorIds", []),
  vendorName,
  vendorEmail
});

const onAdvanceSearch = () => { refreshVendorList(); };

// Clear search
const onAdvanceClear = () => {
  search.value.vendorName = "";
  search.value.vendorIds = [];
  search.value.vendorEmail = "";
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
  ...mapFilterToLabel(search.value.vendorIds, ownerNameDropdown.list, "Owner Name"),
  ...(search.value.vendorName ? { "Vendor Name": search.value.vendorName } : {}),
  ...(search.value.vendorEmail ? { "Vendor Email": search.value.vendorEmail } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Owner Name": return search.value.vendorIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Owner Name") {
    search.value.vendorIds = [];
  } else if (key === "Vendor Name") {
    search.value.vendorName = "";
  } else if (key === "Vendor Email") {
    search.value.vendorEmail = "";
  }
  delete appliedFilters.value[key];
  refreshVendorList()
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { ownerNameDropdown } = financeExpenseVendorsModule();

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  ownerNameDropdown.load(true);
  refreshVendorList();
});

// ------------------------------------------------------------------------------------
// On Change
// ------------------------------------------------------------------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  refreshVendorList();
});

</script>
