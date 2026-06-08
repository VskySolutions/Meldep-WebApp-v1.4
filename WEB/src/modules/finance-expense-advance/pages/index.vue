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
              <q-breadcrumbs-el label="Finance" />
              <q-breadcrumbs-el label="List of Advance Expenses" />
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
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <singleSelectDropdown
                          v-model="search.employeeId"
                          label="Requested By"
                          :options="activeEmployeesDropdownSingleSelect.list.value"
                          :filter="activeEmployeesDropdownSingleSelect.filter"
                        />
                        <multiSelectDropdown
                          v-model="search.referenceId"
                          label="ReferenceId"
                          :options="advanceExpenseReferenceIdDropdown.list.value"
                          :filter="advanceExpenseReferenceIdDropdown.filter"
                        />
                       <multiSelectDropdown
                          v-model="search.statusId"
                          label="Status"
                          :options="advanceExpenseStatusDropdown.list.value"
                          :filter="advanceExpenseStatusDropdown.filter"
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
                <q-btn icon="o_add" outline label="Add Advance Expense" no-caps class="text-primary btnRounded" @click="onAdvanceExpenseAdd(refreshAdvanceExpenseList)" />
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
          @request="getAllAdvanceExpenseList"
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
              <q-td style="width: 10%;">{{ props.row.referenceId }}</q-td>
              <q-td class="text-center" style="width: 10%;">{{ props.row.requestDate.replace(/-/g, '/') }}</q-td>
              <q-td style="width: 16%;">{{ props.row.itemCategory }}</q-td>
              <q-td style="width: 16%;">{{ props.row.paymentType }}</q-td>
              <q-td style="width: 10%;">{{ props.row.requestedEmployee }}</q-td>
              <q-td style="width: 10%;">
                <q-badge :style="{ backgroundColor: props.row.bgColor, color:props.row.color }">{{ props.row.advanceExpenseStatus }}</q-badge>
              </q-td>
              <q-td style="width: 0%;">{{ props.row.createdBy }}</q-td>
              <q-td style="text-align: right;width: 0%;">{{ props.row.amount }}</q-td>
              <q-td style="width: 10%;" class="actions text-center">
                <div>
                  <q-icon name="o_forward" class="cursor-pointer q-mr-sm" :class="(props.row.advanceExpenseStatus === 'Draft' ) ? '' : 'hidden'" @click="onForwardToApprover(props.row, 'Submitted')">
                    <q-tooltip>Forward To Submitted</q-tooltip>
                  </q-icon>
                  <q-icon name="o_forward" class="cursor-pointer q-mr-sm" :class="props.row.advanceExpenseStatus !== 'Approved' && props.row.advanceExpenseStatus === 'Submitted' && props.row.advanceExpenseStatus !== 'Cancelled' && props.row.advanceExpenseStatus !== 'Request For Cancellation' ? '' : 'hidden'" @click="onForwardToApprover(props.row, 'Request For Cancellation')">
                    <q-tooltip>Request For Cancellation</q-tooltip>
                  </q-icon>
                  <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onAdvanceExpenseView(props.row.id, false, refreshAdvanceExpenseList)">
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                  <q-icon name="o_edit" class="cursor-pointer q-mr-sm" :class="['Draft', 'Submitted'].includes(props.row.advanceExpenseStatus) ? '' : 'hidden'" @click="onAdvanceExpenseEdit(props.row.id, refreshAdvanceExpenseList)">
                    <q-tooltip>Edit</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_history"
                    class="cursor-pointer q-mr-sm"
                    size="xs"
                    @click.stop="onStatusLog(props.row.id, props.row.referenceId, 'Advance Expense Status')"
                  >
                    <q-tooltip>Data Change Log</q-tooltip>
                  </q-icon>
                  <q-icon name="o_delete_outline" class="cursor-pointer" :class="(props.row.advanceExpenseStatus === 'Draft' ) ? '' : 'hidden'" color="negative" @click="onSubmitAdvanceExpenseDelete(props.row.id, props.row.referenceId, refreshAdvanceExpenseList)">
                    <q-tooltip>Delete</q-tooltip>
                  </q-icon>
                </div>
              </q-td>
            </q-tr>
            <q-tr v-if="props.pageIndex === paginatedRows.length - 1 || props.pageIndex === rows.length - 1">
              <q-td colspan="7" class="text-right font-bold"><b>Total Amount:</b></q-td>
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
import { zwConfirmDelete, notifySuccess, setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";

import advanceExpensesService from "../financeExpenseAdvance.service";
import siteStatusLog from "modules/sites/components/_siteModifiedLogs.vue";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import advanceExpenseModule from "src/modules/finance-expense-advance/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// Shared Expense Advance Dialogs
import {
  initAdvanceExpenseDialogs,
  onAdvanceExpenseView,
  onAdvanceExpenseAdd,
  onAdvanceExpenseEdit,
} from "src/modules/finance-expense-advance/utils/dialogs.js";

// Shared Expense Advance Actions
import {
  initAdvanceExpenseActions,
  onSubmitAdvanceExpenseDelete,
} from "src/modules/finance-expense-advance/utils/actions.js";


// ------------------------------------------------------------------------------------
// Common variables
// ------------------------------------------------------------------------------------

const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

// ------------------------------------------------------------------------------------
// Local storage values
// ------------------------------------------------------------------------------------

const localStorageKey = "Advance Expense Request";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get Advance Expense List
// --------------------------------------------------------------------------------------------------------------------------------------------------
const getAllAdvanceExpenseList = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  try {
    const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
    setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });

    // Await API call so loader works correctly
    const resp = await advanceExpensesService.getAllAdvanceExpenseRequests(payload);
    rows.value = resp.data.map((advanceExpense, index) => ({
      index: index + 1,
      id: advanceExpense.id,
      paymentTypeId: advanceExpense.paymentTypeId,
      referenceId: advanceExpense.referenceId,
      requestDate: advanceExpense.requestDate,
      paymentType: advanceExpense.paymentType.dropDownValue,
      requestedBy: advanceExpense.requestedBy,
      statusId: advanceExpense.statusId,
      requestedEmployee: advanceExpense.requestedEmployee.person.fullName,
      amount: advanceExpense.amount || 0,
      advanceExpenseStatus: advanceExpense.advanceExpenseStatus.dropDownValue,
      createdBy: advanceExpense.createdBy.person.fullName,
      bgColor: advanceExpense.advanceExpenseStatus.bgColor,
      color: advanceExpense.advanceExpenseStatus.color,
      itemCategoryId: advanceExpense.itemCategoryId,
      itemCategory: advanceExpense.itemCategory.type
    }));

    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  } catch (error) {
    $q.notify({ color: "negative", message: "Failed to fetch data." });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

const paginatedRows = computed(() => {
  if (pagination.value.rowsPerPage === 0) {
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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshAdvanceExpenseList = () => {
   getAllAdvanceExpenseList({ pagination: pagination.value });
};

// ------------------------------------------------------------------------------------
// DataTable:- Columns
// ------------------------------------------------------------------------------------
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "referenceId", label: "ReferenceId", field: "referenceId", align: "left", sortable: true },
  { name: "requestDate", label: "Request Date", field: "requestDate", align: "center", sortable: true },
  { name: "itemCategoryId", label: "Item Category", field: "itemCategoryId", align: "left", sortable: true },
  { name: "paymentTypeId", label: "Payment Type", field: "paymentTypeId", align: "left", sortable: true },
  { name: "requestedBy", label: "Requested By", field: "requestedBy", align: "left", sortable: true },
  { name: "statusId", label: "Status", field: "statusId", align: "left", sortable: true },
  { name: "createdById", label: "Created By", field: "createdById", align: "left", sortable: true },
  { name: "amount", label: "Amount", field: "amount", align: "right", sortable: false }

]);

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initAdvanceExpenseDialogs(activeRowId);
initAdvanceExpenseActions(activeRowId);


const onStatusLog = (id, name, columnName) => {
  activeRowId.value = id;
  $q.dialog({
    component: siteStatusLog,
    componentProps: { id, name, columnName }
  }).onOk(() => {
    getAllAdvanceExpenseRequests({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const onForwardToApprover = (row, approver) => {
  activeRowId.value = row.id;
  row.approver = approver;
  const payload = {
    id: row.id,
    approver
  };
  $q.dialog({
    title: "<span class=\"text-primary\">Confirm</span>",
    message: approver === "Request For Cancellation" ? "Are you sure you want to send request for cancellation ?" : approver === "Submitted" ? "Are you sure you want to submit expense?" : "",
    html: true,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(() => {
    loading.value = true;
    advanceExpensesService.forwardAdvanceExpenseToApprovers(payload)
      .then(() => {
        notifySuccess({
          message: approver === "Request For Cancellation" ? "Request Sent Successfully!" : approver === "Submitted" ? "Submitted Successfully!" : ""
        });
        loading.value = false;
        $q.loading.hide();
        activeRowId.value = null;
        getAllAdvanceExpenseRequests({ pagination: pagination.value });
      })
      .catch(() => {
        loading.value = false;
        $q.loading.hide();
        activeRowId.value = null;
        $q.notify({
          type: "negative",
          message: "Failed to update expense."
        });
      });
  }).onCancel(() => {
    activeRowId.value = null;
  });
};

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
  referenceId: getFilterValue("referenceId", []),
  statusId: getFilterValue("statusId", []),
  employeeId: getFilterValue("employeeId", null),
});

const onAdvanceSearch = () => { refreshAdvanceExpenseList(); };

const onAdvanceClear = () => {
  search.value.referenceId = [];
  search.value.statusId = [];
  search.value.employeeId = null;
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { activeEmployeesDropdownSingleSelect } = employeeModule();
const { advanceExpenseReferenceIdDropdown, advanceExpenseStatusDropdown  } = advanceExpenseModule();

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
  ...mapFilterToLabel(search.value.referenceId, advanceExpenseReferenceIdDropdown.list, "Advance Expense Request"),
  ...mapFilterToLabel(search.value.statusId, advanceExpenseStatusDropdown.list, "Status"),
  ...mapFilterToSingleLabel(search.value.employeeId, activeEmployeesDropdownSingleSelect.list, "Requested By")
}));

function getFilterCount (key) {
  switch (key) {
  case "Advance Expense Request": return search.value.referenceId?.length || 0;
  case "Status": return search.value.statusId?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Requested By") {
    search.value.employeeId = "";
  } else if (key === "Advance Expense Request") {
    search.value.referenceId = [];
  } else if (key === "Status") {
    search.value.statusId = [];
  }
  delete appliedFilters.value[key];
  refreshAdvanceExpenseList();
}

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  advanceExpenseReferenceIdDropdown.load();
  activeEmployeesDropdownSingleSelect.load();
  advanceExpenseStatusDropdown.load("Advance Expense Status");
  refreshAdvanceExpenseList();
});

// ------------------------------------------------------------------------------------
// On Change
// ------------------------------------------------------------------------------------

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshAdvanceExpenseList();
});

</script>
