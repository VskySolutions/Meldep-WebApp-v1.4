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
              <q-breadcrumbs-el label="Expenses" />
              <q-breadcrumbs-el label="Approve Expenses" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center q-ml-lg">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-6 col-sm-8 col-md-7 col-lg-6 col-xl-6">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Created By</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.createdBy"
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            use-input
                            :dense="true"
                            :options="createdByList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Expense No</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.expenseNumber" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.payeeIds"
                        label="Vendor"
                        :options="vendorNameDropdown.list.value"
                        :filter="vendorNameDropdown.filter"
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
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="q-table-container q-pa-none">
        <q-table
          ref="tableRef"
          v-model:pagination="pagination"
          :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
          bordered
          :loading="loading"
          :rows="rows"
          :columns="columns"
          row-key="id"
          separator="cell"
          no-data-label="No data available"
          binary-state-sort
          :rows-per-page-options="[20, 50, 100, 200, 500]"
          @request="getAllApproveExpenseList"
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
              <q-td style="width: 3%;">{{ props.row.expenseNumber }}
                <q-icon
                  v-if="props.row.isEdited"
                  name="o_edit"
                  size="xs"
                  color="primary"
                  class="q-ml-xs"
                >
                  <q-tooltip>Expense Updated</q-tooltip>
                </q-icon>
              </q-td>
              <q-td class="text-center" style="width: 10%;">{{ props.row.date.replace(/-/g, '/') }}</q-td>
              <q-td style="width: 10%;">{{ props.row.payee }}</q-td>
              <q-td style="width: 20%;">{{ props.row.paymentType }}</q-td>
              <q-td style="width: 0%;">
                <q-badge :style="{ backgroundColor: props.row.bgColor, color:props.row.color }">
                  <span v-if="props.row.status === 'Submitted'">Waiting for Pre-Approver</span>
                  <span v-else-if="props.row.status === 'Pre-Approved'">Waiting for Approver</span>
                  <span v-else>{{ props.row.status }}</span>
                </q-badge>
              </q-td>
              <q-td style="width: 10%;">{{ props.row.createdById }}</q-td>
              <q-td style="text-align: right;width: 0%;">{{ props.row.amount }}</q-td>
              <q-td style="width: 0%;" class="text-center actions">
                <div>
                  <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onExpenseView(props.row.id, false, refreshApproveExpenseList)">
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                  <q-icon v-if="(roles.includes('finance-approver') || roles.includes('finance-preapprove') || roles.includes('finance-paidby') || role) && props.row.status != 'Paid' && props.row.status != 'Declined' && props.row.status != 'Cancelled'" name="o_double_arrow" class="cursor-pointer q-mr-sm" @click="onExpenseView(props.row.id, true, refreshApproveExpenseList)">
                    <q-tooltip>
                      {{ props.row.status === 'Approved' ? 'View Details' : 'Approve/Decline Expense' }}
                    </q-tooltip>
                  </q-icon>
                  <q-icon v-if="(roles.includes('finance-approver') || roles.includes('finance-preapprove') || role) && props.row.status == 'Request For Cancellation'" name="o_cancel" class="cursor-pointer q-mr-sm" @click="onCancelExpenseRequest(props.row, 'Cancelled', refreshApproveExpenseList)">
                    <q-tooltip>Cancel Expense Request</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_history"
                    class="cursor-pointer"
                    size="xs"
                    @click.stop="onSiteModifiedLog(props.row.id, props.row.expenseNumber, 'Expense Status')"
                  >
                    <q-tooltip>Data Change Log</q-tooltip>
                  </q-icon>
                </div>
              </q-td>
            </q-tr>
            <q-tr v-if="props.pageIndex === paginatedRows.length - 1 || props.pageIndex === rows.length - 1">
              <q-td colspan="6" class="text-right font-bold"><b>Total Amount:</b></q-td>
              <q-td class="text-right"><b>{{ totalAmountForPage }}</b></q-td>
              <q-td colspan="3" />
            </q-tr><q-separator />
          </template>
        </q-table>
      </div>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import { useQuasar } from "quasar";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { useAuthStore } from "stores/auth";
import financeExpenseService from "modules/finance-expense/financeExpense.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Dropdowns
import financeExpenseVendorsModule from "src/modules/finance-expense-vendors/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Expense Dialogs
import {
  initExpenseDialogs,
  onExpenseView,
} from "src/modules/finance-expense/utils/dialogs.js";

// Shared Site Dialogs
import {
  initSiteDialogs,
  onSiteModifiedLog
} from "src/modules/sites/utils/dialogs.js";

// Shared Expense Actions
import {
  initExpenseActions,
  onCancelExpenseRequest,
} from "src/modules/finance-expense/utils/actions.js";

const authStore = useAuthStore();
const user = authStore.user;
const roles = user?.roles || [];
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const createdByList = ref(["Created By Me", "View All"]);

const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);


// ------------------------------------------------------------------------------------
// Local storage values
// ------------------------------------------------------------------------------------
const localStorageKey = "Expense";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const expenseNumber = filterLocalStorage ? filterLocalStorage.expenseNumber : "";
const createdBy = filterLocalStorage ? filterLocalStorage.createdBy : "Created By Me";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get Approve Expense List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const getAllApproveExpenseList = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;

  try {
    const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
    setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });

    const resp = await financeExpenseService.getAllApproveExpenseList(payload);
    const Expenses = resp.data;
    rows.value = Expenses.map((expense, index) => ({
      index: index + 1,
      id: expense.id,
      expenseNumber: expense.expenseNumber,
      date: expense.expenseDate,
      payee: expense.expenseVendors.vendorName,
      refNo: expense.ref_no,
      amount: expense.amount || 0,
      status: expense.expenseStatus?.dropDownValue,
      color: expense.expenseStatus?.color,
      bgColor: expense.expenseStatus?.bgColor,
      createdById: expense.createdBy.person.fullName,
      paymentType: expense.expenseVendorBankAccounts.paymentType.dropDownValue,
      bankAccount: expense.expenseVendorBankAccounts.accountNumber
        ? expense.expenseVendorBankAccounts.accountNumber
        : expense.expenseVendorBankAccounts.upI_ID,
      isEdited: expense.isEdited
    }));

    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  } catch (error) {
    console.error("Failed to fetch expenses:", error);
    $q.notify({ color: "negative", message: "Failed to fetch expenses." });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshApproveExpenseList = () => {
   getAllApproveExpenseList({ pagination: pagination.value });
};

// Toatal Amount  calculation
const paginatedRows = computed(() => {
  if (pagination.value.rowsPerPage === 0) {
    // If rowsPerPage is 0, show all rows
    return rows.value;
  }
  const start = (pagination.value.page - 1) * pagination.value.rowsPerPage;
  const end = pagination.value.page * pagination.value.rowsPerPage;
  return rows.value.slice(start, end);
});
const totalAmountForPage = computed(() => {
  return paginatedRows.value
    .reduce((sum, row) => sum + (parseFloat(row.amount) || 0), 0);
});

// ------------------------------------------------------------------------------------
// DataTable:- Columns
// ------------------------------------------------------------------------------------
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "expenseNumber", label: "Expense No", field: "expenseNumber", align: "left", sortable: true },
  { name: "expenseDate", label: "Date", field: "expenseDate", align: "center", sortable: true },
  { name: "expenseVendors.vendorName", label: "Vendor", field: "expenseVendors.vendorName", align: "left", sortable: true },
  { name: "expenseVendorBankAccounts.paymentType.dropDownValue", label: "Vendor Payment Method", field: "expenseVendorBankAccounts.paymentType.dropDownValue", align: "left", sortable: true },
  { name: "expenseStatus.dropDownValue", label: "Status", field: "expenseStatus.dropDownValue", align: "left", sortable: true },
  { name: "createdById", label: "Created By", field: "createdById", align: "left", sortable: true },
  { name: "amount", label: "Amount", field: "amount", align: "right", sortable: false }
]);

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initExpenseDialogs(activeRowId);
initSiteDialogs(activeRowId);
initExpenseActions(activeRowId);

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
  payeeIds: getFilterValue("payeeIds", []),
  statusName: "Draft",
  expenseNumber,
  createdBy
});

const onAdvanceSearch = () => { refreshApproveExpenseList(); };

// Clear search
const onAdvanceClear = () => {
  search.value.expenseNumber = "";
  search.value.bankAccountIds = [];
  search.value.payeeIds = [];
  search.value.createdBy = "";
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

const mapFilterToSingleLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.payeeIds, vendorNameDropdown.list, "Vendor"),
  ...(search.value.expenseNumber ? { "Expense No": search.value.expenseNumber } : {}),
  ...mapFilterToSingleLabel(search.value.createdBy, createdByList, "Created By")
}));

function getFilterCount (key) {
  switch (key) {
  case "Vendor": return search.value.payeeIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Created By") {
    search.value.createdBy = "";
  } else if (key === "Bank Account") {
    search.value.bankAccountIds = [];
  } else if (key === "Vendor") {
    search.value.payeeIds = [];
  } else if (key === "Expense No") {
    search.value.expenseNumber = "";
  }
  delete appliedFilters.value[key];
  refreshApproveExpenseList();
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { vendorNameDropdown } = financeExpenseVendorsModule();

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  vendorNameDropdown.load();
  refreshApproveExpenseList();
});

// ------------------------------------------------------------------------------------
// On Change
// ------------------------------------------------------------------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  refreshApproveExpenseList();
});


</script>
