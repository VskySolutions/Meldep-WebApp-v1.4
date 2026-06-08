<template>
  <q-page padding>
    <q-card class="project7">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-3 col-sm-2 col-md-3 col-lg-4 col-xl-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Expenses" />
              <q-breadcrumbs-el label="List of Bank Accounts" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center q-ml-lg">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-6 col-sm-8 col-md-7 col-lg-6 col-xl-6">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center q-mr-xs">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Bank Account Number</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.accountNumber" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Bank Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.bankName" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Branch Location</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.branchName" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
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
                <q-btn icon="o_add" outline label="Add Bank Account" no-caps class="text-primary btnRounded" @click="onBankAccountAdd(refreshBankAccountList)" />
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
        @request="getAllBankAccountList"
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
            <q-td>{{ props.rowIndex + 1 }}</q-td>
            <q-td><q-badge v-if="!props.row.accountNumber" color="red" square class="justify-center">No Data</q-badge>
              {{ props.row.accountNumber }}
            </q-td>
            <q-td><q-badge v-if="!props.row.bankname" color="red" square class="justify-center">No Data</q-badge>{{ props.row.bankname }}</q-td>
            <q-td><q-badge v-if="!props.row.ifscCode" color="red" square class="justify-center">No Data</q-badge>{{ props.row.ifscCode }}</q-td>
            <q-td><q-badge v-if="!props.row.accountTypeDropDown" color="red" square class="justify-center">No Data</q-badge>{{ props.row.accountTypeDropDown }}</q-td>
            <q-td>
              <q-icon
                :name="props.row.activeStatus === 'Active' ? 'o_check_circle' : 'o_cancel'"
                :color="props.row.activeStatus === 'Active' ? 'positive' : 'negative'"
                class="cursor-pointer"
                size="sm"
                style="margin-left: 45%;"
              >
                <q-tooltip>{{ props.row.activeStatus === 'Active' ? 'Active' : 'Inactive' }}</q-tooltip>
              </q-icon>
            </q-td>
            <q-td><q-badge v-if="!props.row.branchName" color="red" square class="justify-center">No Data</q-badge>{{ props.row.branchName }}</q-td>
            <q-td auto-width class="text-center actions">
              <q-icon name="o_visibility" class="cursor-pointer" @click="onBankAccountView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-ml-sm" @click="onBankAccountEdit(props.row.id, refreshBankAccountList)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer q-ml-sm" color="negative" @click="onSubmitBankAccountDelete(props.row.id, props.row.accountNumber, refreshBankAccountList)">
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
              <q-icon
                :name="props.row.activeStatus === 'Active' ? 'o_block' : 'o_check_circle_outline'"
                :color="props.row.activeStatus === 'Active' ? 'negative' : 'positive'"
                class="cursor-pointer q-ml-sm"
                @click="onSubmitBankAccountStatus(props.row, refreshBankAccountList)"
              >
                <q-tooltip>{{ props.row.activeStatus === 'Active' ? 'Make Inactive' : 'Set Active' }}</q-tooltip>
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
import { ref, onMounted, watch, computed } from "vue";
import financeBankAccountService from "modules/finance-bank-account/financeBankAccount.service";
import {  setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Bank Account Dialogs
import {
  initBankAccountDialogs,
  onBankAccountView,
  onBankAccountAdd,
  onBankAccountEdit,
} from "src/modules/finance-bank-account/utils/dialogs.js";

// Shared Bank Account Actions
import {
  initBankAccountActions,
  onSubmitBankAccountDelete,
  onSubmitBankAccountStatus,
} from "src/modules/finance-bank-account/utils/actions.js";

// ------------------------------------------------------------------------------------
// Common variables
// ------------------------------------------------------------------------------------

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

// ------------------------------------------------------------------------------------
// Local storage values
// ------------------------------------------------------------------------------------

const localStorageKey = "Bank Accounts";
const filterLocalStorage = getLocalStorage(localStorageKey);
const accountNumber = filterLocalStorage ? filterLocalStorage.accountNumber : "";
const bankName = filterLocalStorage ? filterLocalStorage.bankName : "";
const branchName = filterLocalStorage ? filterLocalStorage.branchName : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get  Bank Account List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const getAllBankAccountList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  financeBankAccountService.getAllBankAccountList(payload).then((resp) => {
    rows.value = resp.data.map((item, index) => ({
      id: item.id,
      bankname: item.bankName,
      accountNumber: item.accountNumber,
      ifscCode: item.ifscCode,
      accountType: item.accountType,
      activeStatus: item.isActive ? "Active" : "Inactive", // Convert boolean to status
      branchName: item.branchName,
      accountTypeDropDown: item.accountTypeDropDown.dropDownValue
    }));
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
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshBankAccountList = () => {
   getAllBankAccountList({ pagination: pagination.value });
};

// ------------------------------------------------------------------------------------
// DataTable:- Columns
// ------------------------------------------------------------------------------------

const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "srNo", label: "Sr No.", field: "srNo", align: "left", sortable: false },
  { name: "accountNumber", label: "Bank Account Number", field: "accountNumber", align: "left", sortable: true },
  { name: "bankName", label: "Bank Name", field: "bankName", align: "left", sortable: true },
  { name: "ifscCode", label: "IFSC Code", field: "ifscCode", align: "left", sortable: true },
  { name: "accountTypeDropDown.dropDownValue", label: "Account Type", field: "accountTypeDropDown.dropDownValue", align: "left", sortable: true },
  { name: "activeStatus", label: "Active/Inactive", field: "activeStatus", align: "center" },
  { name: "branchName", label: "Branch Location", field: "branchName", align: "left", sortable: true }
]);

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initBankAccountDialogs(activeRowId);
initBankAccountActions(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel
// ------------------------------------------------------------------------------------

const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

// Search variables
const search = ref({
  searchText: getFilterValue("searchText", ""),
  accountNumber,
  bankName,
  branchName
});

const onAdvanceSearch = () => { refreshBankAccountList(); };

// Clear search
const onAdvanceClear = () => {
  search.value.accountNumber = "";
  search.value.bankName = "";
  search.value.branchName = "";
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// Advance Filter :- Applied Filter Labels.
// ------------------------------------------------------------------------------------
const appliedFilters = computed(() => ({
  ...(search.value.accountNumber ? { "Bank Account Number": search.value.accountNumber } : {}),
  ...(search.value.bankName ? { "Bank Name": search.value.bankName } : {}),
  ...(search.value.branchName ? { "Branch Location": search.value.branchName } : {})
}));

function onClearFilters (key) {
  if (key === "Bank Account Number") {
    search.value.accountNumber = "";
  } else if (key === "Bank Name") {
    search.value.bankName = "";
  } else if (key === "Branch Location") {
    search.value.branchName = "";
  }
  delete appliedFilters.value[key];
  refreshBankAccountList();
}

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  refreshBankAccountList();
});

// ------------------------------------------------------------------------------------
// On Change
// ------------------------------------------------------------------------------------

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshBankAccountList();
});
</script>
